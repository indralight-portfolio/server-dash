using Common.Utility;
using Dash;
using Dash.Model.Rdb;
using Dash.Protocol;
using Dash.StaticInfo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server_dash.Lobby.Services
{
    using Dash.Server.Dao.Cache;
    using Dash.StaticData;
    using Dash.Types;
    using server_dash.Internal.Services;
    using System;

    public class UpgradeService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private DaoCache _daoCache;
        private IMemCache _memCache;
        private ISingleDBCache<Auth> _authCache;
        private ISingleDBCache<Account> _accountCache;
        private IMultipleDBCache<Hero> _heroCache;
        private IMultipleDBCache<Equipment> _equipmentCache;
        private IMultipleDBCache<EquipmentSlot> _equipmentSlotCache;
        private IMultipleDBCache<Consume> _consumeCache;

        private RewardService _rewardService;

        public UpgradeService(DaoCache daoCache, RewardService rewardService)
        {
            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _authCache = daoCache.GetSingle<Auth>();
            _accountCache = daoCache.GetSingle<Account>();
            _heroCache = daoCache.GetMultiple<Hero>();
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _equipmentSlotCache = daoCache.GetMultiple<EquipmentSlot>();
            _consumeCache = daoCache.GetMultiple<Consume>();
            _rewardService = rewardService;
        }
        public async Task<LevelUpEquipmentResponse> LevelUpEquipment(ulong oidAccount, int itemSerial, bool useJewel = false)
        {
            LevelUpEquipmentResponse response = new LevelUpEquipmentResponse();
            if (oidAccount == 0 || itemSerial == 0)
            {
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }

            (Account account, Equipment equipment, List<Consume> consumes) =
                await TaskUtility.WaitAll3(
                    _accountCache.Get(oidAccount),
                    _equipmentCache.Get(oidAccount, Equipment.MakeSubKeysWithName(itemSerial)),
                    _consumeCache.GetAll(oidAccount));

            if (account == null || equipment == null || consumes == null)
            {
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }

            var equipmentInfo = StaticInfo.Instance.EquipmentInfo[equipment.Id];
            if (equipmentInfo == null)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                return response;
            }
            var levelUpConsumeInfo = StaticInfo.Instance.EquipmentLevelUpConsumeInfo[equipment.Level];
            if (levelUpConsumeInfo == null)
            {
                response.ErrorCode = ErrorCode.InternalError;
                _logger.Error($"[LevelUpEquipment]{oidAccount.LogOid()} LevelUpConsumeInfo is null. {equipment.Level}");
                return response;
            }
            if (StaticInfo.Instance.EquipmentLevelUpItems.ContainsKey(equipmentInfo.Type) == false)
            {
                response.ErrorCode = ErrorCode.InternalError;
                _logger.Error($"[LevelUpEquipment]{oidAccount.LogOid()} EquipmentLevelUpItems is empty. {equipmentInfo.Type}");
                return response;
            }

            int needGold = levelUpConsumeInfo.Gold;
            int needJewel = 0;
            if (account.Gold < needGold)
            {
                if (useJewel == true)
                {
                    int deficitGold = needGold - account.Gold;
                    needGold = account.Gold;
                    needJewel = StaticInfo.Instance.CalcJewelForBuyGold(deficitGold);
                    if (account.Jewel < needJewel)
                    {
                        response.SetResult(ErrorCode.NotEnoughJewel);
                        return response;
                    }
                }
                else
                {
                    response.SetResult(ErrorCode.NotEnoughGold);
                    return response;
                }
            }
            ItemGrade equipmentGrade = EnumInfo<ItemGrade>.ConvertUshort(equipment.Grade);
            int nextLevel = equipment.Level + 1;
            var nextId = new IdItemGradeKeyType(equipment.Id, equipmentGrade);
            if (StaticInfo.Instance.EquipmentLevelTableInfo.Exist(nextId) == false)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = "Already MaxLevel";
                return response;
            }

            // TODO:보통은 한개일듯 하지만 여러개이면 어떤것을 먼저 소비 할지 정해야 한다.
            var consumeItemIds = StaticInfo.Instance.EquipmentLevelUpItems[equipmentInfo.Type];
            var targetItems = consumes.FindAll((e) =>
            {
                return consumeItemIds.FindIndex(c => c == e.Id) != -1;
            });

            if (targetItems.Sum(e => e.Count) < levelUpConsumeInfo.ConsumeItemCount)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = "Not Enough Item";
                return response;
            }

            var trans = DaoCache.Instance.Transaction();
            var logAndProgress = new LogAndProgress(account);
            response.ChangedColumns = new ChangedColumns();
            response.ChangedColumns.Add(AccountColumns.Gold, account.Gold - needGold);
            response.ChangedColumns.Add(AccountColumns.Jewel, account.Jewel - needJewel);

            await _accountCache.Change(account, response.ChangedColumns.ToDBChanged(), trans);
            Statistics.GameLogger.AccountChange(account, ChangeReason.LevelUpEquipment, response.ChangedColumns, logAndProgress);
            ProgressService.AccountChange(account, response.ChangedColumns, logAndProgress);
            int consumeCount = levelUpConsumeInfo.ConsumeItemCount;
            List<Consume> removeConsumes = new List<Consume>();
            List<Consume> changedConsumes = new List<Consume>();
            for (int i = 0; i < targetItems.Count; ++i)
            {
                if (consumeCount >= targetItems[i].Count)
                {
                    consumeCount -= targetItems[i].Count;
                    targetItems[i].Count = 0;
                    removeConsumes.Add(targetItems[i]);
                }
                else
                {
                    Consume changedConsume = new Consume(targetItems[i]);
                    changedConsume.Count -= consumeCount;
                    consumeCount = 0;
                    changedConsumes.Add(changedConsume);
                    await _consumeCache.Change(targetItems[i], changedConsume, trans);
                }
                if (consumeCount <= 0)
                {
                    break;
                }
            }
            if (removeConsumes.Count != 0)
            {
                await _consumeCache.DelMany(removeConsumes, trans);
            }

            Equipment changedEquipemnt = new Equipment(equipment);
            changedEquipemnt.Level = nextLevel;
            await _equipmentCache.Change(equipment, changedEquipemnt, trans);
            if (await trans.Execute() == false)
            {
                response.ErrorCode = ErrorCode.DbError;
                _logger.Error($"[LevelUpEquipment]{oidAccount.LogOid()} transaction failed.");
                return response;
            }
            response.ProgressChanges = logAndProgress.GetProgressChanges();
            response.ChangedEquipmentItem = changedEquipemnt;
            response.ChangedConsumeItems = new List<Consume>();
            response.ChangedConsumeItems.AddRange(changedConsumes);
            response.ChangedConsumeItems.AddRange(removeConsumes);
            //response.ProgressChanges.Put(Dash.ProgressKey.UpgradeItem, 1);

            Statistics.GameLogger.ConsumeChanges(oidAccount, account.Level, ChangeReason.LevelUpEquipment.ToString(), targetItems, response.ChangedConsumeItems, logAndProgress);
            Statistics.GameLogger.EquipmentChange(oidAccount, account.Level, ChangeReason.LevelUpEquipment.ToString(), changedEquipemnt, true, logAndProgress);

            await logAndProgress.Execute();
            return response;
        }

        public async Task<CombineEquipmentResponse> CombineEquipment(ulong oidAccount, int targetItemSerial, int consumeItemSerial1, int consumeItemSerial2)
        {
            CombineEquipmentResponse response = new CombineEquipmentResponse();
            if (oidAccount == 0 || targetItemSerial == 0 || consumeItemSerial1 == 0 || consumeItemSerial2 == 0)
            {
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }
            (Account account, Equipment targetEquipment, Equipment consumeEquipment1, Equipment consumeEquipment2, List<EquipmentSlot> slots) =
                await TaskUtility.WaitAll5(
                    _accountCache.Get(oidAccount),
                    _equipmentCache.Get(oidAccount, Equipment.MakeSubKeysWithName(targetItemSerial)),
                    _equipmentCache.Get(oidAccount, Equipment.MakeSubKeysWithName(consumeItemSerial1)),
                    _equipmentCache.Get(oidAccount, Equipment.MakeSubKeysWithName(consumeItemSerial2)),
                    _equipmentSlotCache.GetAll(oidAccount)
                    );
            if (account == null || targetEquipment == null || consumeEquipment1 == null || consumeEquipment2 == null)
            {
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }
            if (targetEquipment.Id != consumeEquipment1.Id || targetEquipment.Id != consumeEquipment2.Id)
            {
                _logger.Error($"[CombineEquipemnt]{oidAccount.LogOid()} Not Same Item. {targetEquipment.Id}/{consumeEquipment1.Id}/{consumeEquipment2.Id}");
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }
            if (targetEquipment.Grade != consumeEquipment1.Grade || targetEquipment.Grade != consumeEquipment2.Grade)
            {
                _logger.Error($"[CombineEquipemnt]{oidAccount.LogOid()} Not Same Grade. {targetEquipment.Grade}/{consumeEquipment1.Grade}/{consumeEquipment2.Grade}");
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }
            EquipmentInfo targetEquipmentInfo = StaticInfo.Instance.EquipmentInfo[targetEquipment.Id];
            ItemGrade equipmentGrade = EnumInfo<ItemGrade>.ConvertUshort(targetEquipment.Grade);
            ItemGrade nextGrade = EnumInfo<ItemGrade>.Next(equipmentGrade);
            if (nextGrade == ItemGrade.Undefined)
            {
                _logger.Error($"[CombineEquipment{oidAccount.LogOid()} Already Max Grade. {equipmentGrade}->{nextGrade}");
                return response;
            }
            Equipment changedTargetEquipment = new Equipment(targetEquipment);
            List<Equipment> removedEquipments = new List<Equipment>() { consumeEquipment1, consumeEquipment2 };
            changedTargetEquipment.Grade = (ushort)nextGrade;

            RewardInfo returnReward = new RewardInfo();
            RewardInfo excessReward = new RewardInfo();

            var trans = DaoCache.Instance.Transaction();
            var logAndProgress = new LogAndProgress(account);
            ////소비되는 장비의 업글 아이템을 반환해준다.
            await _equipmentCache.Change(targetEquipment, changedTargetEquipment, trans);// TODO: 등급 변경 로그 추가

            await _equipmentCache.DelMany(removedEquipments, trans);
            Statistics.GameLogger.EquipmentChanges(oidAccount, account.Level, ChangeReason.CombineEquipment.ToString(), null, removedEquipments, logAndProgress);

            Consume changedConsume = null;

            EquipmentLevelupConsumeInfo consumeInfo = StaticInfo.Instance.TotalEquipmentLevelupConsumeInfo(consumeEquipment1.Level);
            EquipmentLevelupConsumeInfo consumeInfo2 = StaticInfo.Instance.TotalEquipmentLevelupConsumeInfo(consumeEquipment2.Level);

            int returnConsumeItemCount = consumeInfo.ConsumeItemCount + consumeInfo2.ConsumeItemCount;
            int returnGold = consumeInfo.Gold + consumeInfo2.Gold;

            if (returnConsumeItemCount > 0)
            {
                var consumeItemId = StaticInfo.Instance.EquipmentLevelUpItems[targetEquipmentInfo.Type].First();
                var consume = await _consumeCache.Get(oidAccount, Consume.MakeSubKeysWithName(consumeItemId));
                int deltaCount = Internal.Services.RewardService.GetRewardableConsumeCount(consume?.Count ?? 0, returnConsumeItemCount);

                if (deltaCount > 0)
                {
                    changedConsume = new Consume();
                    changedConsume.Id = consumeItemId;
                    changedConsume.OidAccount = oidAccount;
                    changedConsume.Count = (consume?.Count ?? 0) + returnConsumeItemCount;
                    await Internal.Services.InventoryService.UpdateConsume(oidAccount, _consumeCache, consume, changedConsume, trans);
                    Statistics.GameLogger.ConsumeChange(oidAccount, account.Level, ChangeReason.CombineEquipment.ToString(), consume, changedConsume, logAndProgress);

                    returnReward.AddItem(consumeItemId, deltaCount);
                }
                var excessCount = Math.Max(returnConsumeItemCount - deltaCount, 0);
                if (excessCount > 0) { excessReward.AddItem(consumeItemId, excessCount); }
            }

            if (returnGold > 0)
            {
                int deltaGold = Internal.Services.RewardService.GetRewardableGold(account.Gold, returnGold);
                if (deltaGold != 0)
                {
                    response.ChangedColumns = new ChangedColumns();
                    response.ChangedColumns.Add(AccountColumns.Gold, account.Gold + deltaGold);
                    await _accountCache.Change(account, response.ChangedColumns.ToDBChanged(), trans);
                    Statistics.GameLogger.AccountChange(account, ChangeReason.CombineEquipment, response.ChangedColumns, logAndProgress);
                    ProgressService.AccountChange(account, response.ChangedColumns, logAndProgress);

                    returnReward.Gold = deltaGold;
                }
                excessReward.Gold = Math.Max(returnGold - deltaGold, 0);
            }

            response.ChangedConsume = changedConsume;
            response.ChangedEquipment = changedTargetEquipment;
            response.RemovedEquipments.Add(consumeEquipment1);
            response.RemovedEquipments.Add(consumeEquipment2);
            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].EquipmentSerial == consumeEquipment1.Serial || slots[i].EquipmentSerial == consumeEquipment2.Serial)
                {
                    EquipmentSlot changedSlot = new EquipmentSlot(slots[i]);
                    slots[i].EquipmentSerial = 0;
                    _equipmentSlotCache.Change(changedSlot, slots[i], trans);
                }
            }
            response.ReturnReward = returnReward;

            if (await trans.Execute() == false)
            {
                _logger.Error($"[CombineEquipment]{oidAccount.LogOid()} Transaction failed.");
                response.ChangedColumns.Clear();
                response.RemovedEquipments.Clear();
                response.ChangedEquipment = null;
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }
            await logAndProgress.Execute();

            response.IsSendMail = excessReward.IsEmpty() == false;
            if (response.IsSendMail)
            {
                //우편으로 보내줌
                await _rewardService.SendMailExcessReward(account.OidAccount, excessReward);
            }
            return response;
        }
        public async Task<UpgradeCharacterResponse> UpgradeCharacter(ulong oidAccount, int characterId, bool useJewel = false)
        {
            UpgradeCharacterResponse response = new UpgradeCharacterResponse();
            if (oidAccount == 0 || characterId == 0)
            {
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }
            (Account account, List<Hero> heroes, List<Consume> consumes) =
                   await TaskUtility.WaitAll3(
                       _accountCache.Get(oidAccount),
                       _heroCache.GetAll(oidAccount),
                       _consumeCache.GetAll(oidAccount));

            if (account == null || heroes == null)
            {
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }
            var findHero = heroes.Find(e => e.Id == characterId);
            if (findHero == null)
            {
                _logger.Error($"[UpgradeCharacter]{oidAccount.LogOid()} character not found. {characterId}");
                response.ErrorCode = ErrorCode.InvalidParameter;
                return response;
            }
            var characterInfo = StaticInfo.Instance.CharacterInfo[findHero.Id];
            int currentLevel = findHero.HeroLevel;
            if (currentLevel >= Dash.StaticData.Entity.CharacterInfo.MaxLevel)
            {
                _logger.Error($"[UpgradeCharacter]{oidAccount.LogOid()} Id : {findHero.Id} Already MaxLevel");
                response.ErrorCode = ErrorCode.WrongRequest;
                return response;
            }
            int nextLevel = currentLevel + 1;
            CharacterUpgradeCostInfo costInfo = StaticInfo.Instance.CharacterUpgradeCostInfo[nextLevel];

            int needGold = costInfo.Gold;
            int needJewel = 0;
            if (account.Gold < needGold)
            {
                if (useJewel == true)
                {
                    int deficitGold = needGold - account.Gold;
                    needGold = account.Gold;
                    needJewel = StaticInfo.Instance.CalcJewelForBuyGold(deficitGold);
                    if (account.Jewel < needJewel)
                    {
                        response.SetResult(ErrorCode.NotEnoughJewel);
                        return response;
                    }
                }
                else
                {
                    response.SetResult(ErrorCode.NotEnoughGold);
                    return response;
                }
            }

            int commonSoulChangeCount = 0;
            Consume commonSoulItem = null;
            if (consumes != null)
            {
                commonSoulItem = consumes.Find(e => e.Id == StaticInfo.Instance.CommonSoulItemId);
            }

            if (findHero.SoulCount + (commonSoulItem?.Count ?? 0) < costInfo.SoulCount)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = "NotEnoughCharacterCount";
                return response;
            }
            int remainConsumeCount = costInfo.SoulCount;
            Hero changedHero = new Hero(findHero);
            changedHero.HeroLevel = (ushort)nextLevel;
            if (changedHero.SoulCount >= costInfo.SoulCount)
            {
                remainConsumeCount = 0;
                changedHero.SoulCount -= costInfo.SoulCount;
            }
            else
            {
                remainConsumeCount -= changedHero.SoulCount;
                changedHero.SoulCount = 0;
            }
            if (changedHero.EmblemId == 0 && characterInfo.FirstEmblemGrade <= characterInfo[changedHero.HeroLevel].Grade)
            {
                changedHero.EmblemId = (ushort)characterInfo.FirstEmblemId;
            }

            if (nextLevel == Dash.StaticData.Entity.CharacterInfo.MaxLevel && changedHero.SoulCount > 0)
            {
                //남은 갯수는 commonSoul로 변경
                commonSoulChangeCount += changedHero.SoulCount;
                changedHero.SoulCount = 0;
            }

            //소비
            commonSoulChangeCount -= remainConsumeCount;
            if (costInfo.Gold > 0)
            {
                response.ChangedColumns.Add(AccountColumns.Gold, account.Gold - needGold);
                response.ChangedColumns.Add(AccountColumns.Jewel, account.Jewel - needJewel);
            }
            List<Task<bool>> tasks = new List<Task<bool>>();
            LogAndProgress logAndProgress = new LogAndProgress(account);

            response.ChangedHeroes.Add(changedHero);
            tasks.Add(_heroCache.Change(findHero, changedHero));
            Statistics.GameLogger.HeroChange(oidAccount, account.Level, ChangeReason.UpgradeCharacter.ToString(), findHero, changedHero, logAndProgress);

            if (commonSoulChangeCount != 0)
            {
                Consume changedCommonSoul = null;
                changedCommonSoul = new Consume
                {
                    OidAccount = oidAccount,
                    Id = StaticInfo.Instance.CommonSoulItemId,
                    Count = (commonSoulItem?.Count ?? 0) + commonSoulChangeCount
                };
                if (changedCommonSoul.Count >= 0)
                {
                    tasks.Add(Internal.Services.InventoryService.UpdateConsume(oidAccount, _consumeCache, commonSoulItem, changedCommonSoul));
                    Statistics.GameLogger.ConsumeChange(oidAccount, account.Level, ChangeReason.UpgradeCharacter.ToString(), commonSoulItem, changedCommonSoul, logAndProgress);
                    response.ChangedConsumeItems.Add(changedCommonSoul);
                }
            }

            if (response.ChangedColumns.Count != 0)
            {
                tasks.Add(_accountCache.Change(account, response.ChangedColumns.ToDBChanged()));
                ProgressService.AccountChange(account, response.ChangedColumns, logAndProgress);
                Statistics.GameLogger.AccountChange(account, ChangeReason.UpgradeCharacter, response.ChangedColumns, logAndProgress);
            }
            await Task.WhenAll(tasks);

            await logAndProgress.Execute();
            return response;
        }
    }
}
