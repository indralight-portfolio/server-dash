using Common.StaticInfo;
using Common.Utility;
using Dash;
using Dash.Model;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using Dash.Server.Statistics;
using Dash.StaticData;
using Dash.StaticInfo;
using Dash.Types;
using server_dash.Context;
using server_dash.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Internal.Services
{
    //RewardService는 로비, Battle 양쪽에서 사용할수 있다.
    public class RewardService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        protected DaoCache _daoCache;
        protected IMemCache _memCache;
        protected ISingleDBCache<Account> _accountCache;
        private ISingleDBCache<IssueSerial> _itemSerialCache;

        private IMultipleDBCache<Consume> _consumeCache;
        private IMultipleDBCache<Equipment> _equipmentCache;
        private IMultipleDBCache<Hero> _heroCache;
        private IMultipleDBCache<Talent> _talentCache;
        private IMultipleDBCache<BoxKey> _boxKeyCache;
        private IMultipleDBCache<TimeReward> _timeRewardCache;
        private IMultipleDBCache<ChapterAchievement> _chapterAchievementCache;
        private IMultipleDBCache<CompletedMission> _completedMissionCache;

        protected MailService _mailService;

        public static readonly int MaxGold;
        public static readonly int MaxJewel;
        public static readonly int MaxStamina;
        public static readonly int MaxEquipmentCount;
        public static readonly int MaxConsumeStackCount;
        public static readonly int MaxBoxKeyCount;
        public static readonly int NeedSecondsForRestoreStamina;
        public static readonly int JewelForStamina;
        public static readonly int StaminaByJewel;
        public static readonly int StaminaByAds;
        public static readonly int StaminaByAdsDailyLimit;

        static RewardService()
        {
            ServiceLogicInfo serviceLogicInfo = StaticInfo.Instance.ServiceLogicInfo.Get();
            MaxGold = serviceLogicInfo.MaxGold;
            MaxJewel = serviceLogicInfo.MaxJewel;
            MaxStamina = serviceLogicInfo.MaxStamina;
            MaxEquipmentCount = serviceLogicInfo.MaxEquipmentCount;
            MaxConsumeStackCount = serviceLogicInfo.MaxConsumeStackCount;
            MaxBoxKeyCount = serviceLogicInfo.MaxBoxKeyCount;
            NeedSecondsForRestoreStamina = serviceLogicInfo.NeedSecondsForRestoreStamina;
            JewelForStamina = serviceLogicInfo.JewelForStatmina;
            StaminaByJewel = serviceLogicInfo.StaminaByJewel;
            StaminaByAds = serviceLogicInfo.StaminaByAds;
            StaminaByAdsDailyLimit = serviceLogicInfo.StaminaByAdsDailyLimit;
        }


        public RewardService(DaoCache daoCache, MailService mailService)
        {
            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _accountCache = daoCache.GetSingle<Account>();
            _itemSerialCache = daoCache.GetSingle<IssueSerial>();
            _consumeCache = daoCache.GetMultiple<Consume>();
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _heroCache = daoCache.GetMultiple<Hero>();
            _talentCache = daoCache.GetMultiple<Talent>();
            _boxKeyCache = daoCache.GetMultiple<BoxKey>();
            _timeRewardCache = daoCache.GetMultiple<TimeReward>();
            _chapterAchievementCache = daoCache.GetMultiple<ChapterAchievement>();
            _completedMissionCache = daoCache.GetMultiple<CompletedMission>();

            _mailService = mailService;
        }

        private static void SetResultAndLog(ulong oidAccount, HttpResponseModel model, ErrorCode errorCode, string errorText)
        {
            if (errorCode != ErrorCode.Success)
            {
                _logger.Error($"{oidAccount.LogOid()} | {errorCode} : {errorText}");
            }
            else
            {
                _logger.Info($"{oidAccount.LogOid()} | {errorText}");
            }

            model.SetResult(errorCode, errorText);
        }

        public static int GetRewardableGold(int currentGold, int rewardGold)
        {
            int added = currentGold + rewardGold;
            added = Math.Clamp(added, 0, MaxGold);
            return Math.Max(added - currentGold, 0);
        }

        public static int GetRewardableJewel(int currentJewel, int rewardJewel)
        {
            int added = currentJewel + rewardJewel;
            added = Math.Clamp(added, 0, MaxJewel);
            return Math.Max(added - currentJewel, 0);
        }

        public static int GetRewardableStamina(int currentStamina, int rewardStamina)
        {
            int added = currentStamina + rewardStamina;
            //added = Math.Clamp(added, 0, MaxStamina);
            added = Math.Max(added, 0);
            return Math.Max(added - currentStamina, 0);
        }

        public static int GetRewardableConsumeCount(int currentCount, int rewardCount)
        {
            int added = currentCount + rewardCount;
            added = Math.Clamp(added, 0, MaxConsumeStackCount);
            return Math.Max(added - currentCount, 0);
        }
        public static int GetRewardableCharacterSoulCount(int currentCount, int rewardCount, int currentLevel)
        {
            int added = currentCount + rewardCount;
            added = Math.Clamp(added, 0, StaticInfo.Instance.MaxSoulCountByLevel(currentLevel));
            return Math.Max(added - currentCount, 0);
        }
        public static ushort GetRewardableBoxKeyCount(int currentCount, int rewardCount)
        {
            int added = currentCount + rewardCount;
            added = Math.Clamp(added, 0, MaxBoxKeyCount);
            return (ushort)Math.Max(added - currentCount, 0);
        }

        public async Task<ReceiveTalentTimeRewardModel> ReceiveTalentTimeReward(ulong oidAccount)
        {
            ReceiveTalentTimeRewardModel model = new ReceiveTalentTimeRewardModel();
            model.ErrorCode = ErrorCode.Success;
            int maxGold = StaticInfo.Instance.ServiceLogicInfo.Get().MaxGold;
            var talentTimeRewardInfo = StaticInfo.Instance.TalentTimeRewardInfo.Get();
            if (talentTimeRewardInfo == null)
            {
                _logger.Error($"[AddTimeReward]{oidAccount.LogOid()} talentTimeRewardInfo not found.");
                model.ErrorCode = ErrorCode.InternalError;
                return model;
            }
            //재능 체크
            (Account account, Talent talent, TimeReward timeReward) = await TaskUtility.WaitAll3(
                _accountCache.Get(oidAccount),
                _talentCache.Get(oidAccount, Talent.MakeSubKeysWithName((int)TalentType.AddTimeRewardGold)),
                _timeRewardCache.Get(oidAccount, TimeReward.MakeSubKeysWithName(Dash.Types.TimeRewardType.Talent.ToString()))
            );
            if (talent == null || account == null || timeReward == null)
            {
                _logger.Error($"[AddTimeReward]{oidAccount.LogOid()} db error.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }
            DateTime lastReceiveRewardTime = timeReward.RewardTime;
            //시간체크
            int elapsedSeconds = (int)(DateTime.UtcNow - lastReceiveRewardTime).TotalSeconds;
            if (elapsedSeconds <= talentTimeRewardInfo.RewardableMinElapsedSeconds)
            {
                _logger.Debug($"[AddTimeReward]{oidAccount.LogOid()} invalid elapsed time.");
                model.ErrorCode = ErrorCode.WrongRequest;
                return model;
            }
            if (talent.Level >= talentTimeRewardInfo.ValuesByLevel.Count)
            {
                _logger.Error($"[AddTimeReward]{oidAccount.LogOid()} talent level overflow. id : {talent.Id}, level : {talent.Level}");
                talent.Level = talentTimeRewardInfo.ValuesByLevel.Count - 1;
            }
            var trans = DaoCache.Instance.Transaction();
            RewardInfo rewardInfo = TalentTimeRewardInfoHelper.GetRewardInfo(talent.Level, lastReceiveRewardTime);
            model.LastReceiveTime = DateTime.UtcNow;
            TimeReward changed = new TimeReward(timeReward)
            {
                RewardTime = model.LastReceiveTime
            };
            LogAndProgress logAndProgress = new LogAndProgress(account);
            await _timeRewardCache.Change(timeReward, changed, trans);
            RewardInfo excessReward = null;
            (model.RewardModel, excessReward) = await GiveReward(account, rewardInfo, ChangeReason.TalentTimeReward, logAndProgress, trans);
            if (excessReward != null && excessReward.IsEmpty() == false) //못받는게 생겼다
            {
                model.ErrorCode = ErrorCode.OverLimit;
                return model;
            }
            if (await trans.Execute() == false)
            {
                _logger.Error($"[ReceiveTalentTimeReward]{oidAccount.LogOid()} transaction failed.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }
            await logAndProgress.Execute();
            return model;
        }

        public async Task<(int exp, int level)> RewardGamePlay(ulong oidAccount, int chapterId, LogAndProgress logAndProgress)
        {
            ChapterInfo chapterInfo = StaticInfo.Instance.ChapterInfo[chapterId];
            Account account = await _accountCache.Get(oidAccount);
            ChangedColumns changedColumns = await RewardExp(account, chapterInfo.RewardAccountExp, ChangeReason.RewardGamePlay, logAndProgress);
            _logger.Info($"{oidAccount.LogOid()} RewardGamePlay, Changed : {changedColumns}");
            if (changedColumns.TryGetValue(AccountColumns.Exp, out int exp) == false)
            {
                exp = account.Exp;
            }
            if (changedColumns.TryGetValue(AccountColumns.Level, out int level) == false)
            {
                level = account.Level;
            }
            return (exp, level);
        }

        public async Task<ChangedColumns> RewardExp(ulong oidAccount, int amount, ChangeReason reason, LogAndProgress logAndProgress)
        {
            Account account = await _accountCache.Get(oidAccount);
            return await RewardExp(account, amount, reason, logAndProgress);
        }

        // not tested
        public async Task<ChangedColumns> RewardExp(Account account, int amount, ChangeReason reason, LogAndProgress logAndProgress)
        {
            ulong oidAccount = account.OidAccount;

            ChangedColumns changedColumns = new ChangedColumns();
            if (account.Level >= StaticInfo.Instance.AccountLevelInfo.Count())
            {
                return changedColumns;
            }

            int remainGiveExp = amount;
            int newExp = account.Exp;
            int newLevel = account.Level;
            List<RewardInfo> levelUpRewardInfos = new List<RewardInfo>();
            while (remainGiveExp > 0)
            {
                int needExpToLevelUp = AccountLevelInfo.GetNeedExp(newLevel) - newExp;
                if (remainGiveExp >= needExpToLevelUp)
                {
                    remainGiveExp -= needExpToLevelUp;
                    ++newLevel;
                    newExp = 0;
                    levelUpRewardInfos.Add(StaticInfo.Instance.AccountLevelInfo[newLevel].RewardInfo);
                }
                else
                {
                    newExp += remainGiveExp;
                    remainGiveExp = 0;
                }

                if (newLevel >= StaticInfo.Instance.AccountLevelInfo.Count())
                {
                    break;
                }
            }
            if (newExp != account.Exp)
            {
                changedColumns.Add(AccountColumns.Exp, newExp);
            }
            if (newLevel != account.Level)
            {
                changedColumns.Add(AccountColumns.Level, newLevel);
            }

            if (changedColumns.Count > 0)
            {
                await _accountCache.Change(account, changedColumns.ToDBChanged());
                GameLogger.AccountChange(account, reason, changedColumns, logAndProgress);
                ProgressService.AccountChange(account, changedColumns, logAndProgress);
                account.Apply(changedColumns);
            }
            if (levelUpRewardInfos.Count > 0)
            {
                var combinedInfo = RewardInfoHelper.CombineRewardInfo(levelUpRewardInfos);
                await GiveReward(account, combinedInfo, ChangeReason.AccountLevelUpReward, logAndProgress);
            }
            return changedColumns;
        }

        public async Task<(RewardModel, RewardInfo)> GiveReward(ulong oidAccount, RewardInfo rewardInfo, ChangeReason reason, LogAndProgress logAndProgress, TransactionTask trans = null)
        {
            Account account = await _accountCache.Get(oidAccount);
            return await GiveReward(account, rewardInfo, reason, logAndProgress, trans);
        }
        //주의 : 한도초과시 보상을 받지 않게 처리를 해야한다면 반드시 TransactionTask 써야한다.
        public async Task<(RewardModel, RewardInfo)> GiveReward(Account account, RewardInfo rewardInfo, ChangeReason reason, LogAndProgress logAndProgress, TransactionTask trans = null)
        {
            ulong oidAccount = account.OidAccount;
            List<Consume> consumes = null;
            List<Equipment> equipments = null;
            List<Hero> heroes = null;
            List<BoxKey> boxKeys = null;
            if (trans != null)
            {
                (heroes, equipments, consumes, boxKeys) = await TaskUtility.WaitAll4(
                    _heroCache.GetAll(oidAccount), _equipmentCache.GetAll(oidAccount), _consumeCache.GetAll(oidAccount), _boxKeyCache.GetAll(oidAccount));
            }

            RewardModel rewardModel = new RewardModel();
            RewardInfo excessReward = new RewardInfo();//소지한도초과된것들
            rewardModel.ProgressChanges = new Dictionary<string, ulong>();
            ChangedColumns changedColumns = new ChangedColumns();

            //레벨업 보상이 있기 때문에 가장먼저 처리하고 레벨업 보상을 기존 RewardInfo에 합쳐서 이후 처리까지 한다.
            if (rewardInfo.Exp != 0 && account.Level < StaticInfo.Instance.AccountLevelInfo.Count())
            {
                List<RewardInfo> levelUpRewardInfos = new List<RewardInfo>();
                int remainGiveExp = rewardInfo.Exp;
                int newExp = account.Exp;
                int newLevel = account.Level;

                while (remainGiveExp > 0)
                {
                    int needExpToLevelUp = AccountLevelInfo.GetNeedExp(newLevel) - newExp;
                    if (remainGiveExp >= needExpToLevelUp)
                    {
                        remainGiveExp -= needExpToLevelUp;
                        ++newLevel;
                        newExp = 0;
                        levelUpRewardInfos.Add(StaticInfo.Instance.AccountLevelInfo[newLevel].RewardInfo);
                    }
                    else
                    {
                        newExp += remainGiveExp;
                        remainGiveExp = 0;
                    }

                    if (newLevel >= StaticInfo.Instance.AccountLevelInfo.Count())
                    {
                        break;
                    }
                }
                if (newExp != account.Exp)
                {
                    changedColumns.Add(AccountColumns.Exp, newExp);
                }
                if (newLevel != account.Level)
                {
                    changedColumns.Add(AccountColumns.Level, newLevel);
                }
                if (levelUpRewardInfos.Count > 0)
                {
                    levelUpRewardInfos.Add(rewardInfo);
                    rewardInfo = RewardInfoHelper.CombineRewardInfo(levelUpRewardInfos);
                }
            }
            int deltaGold = GetRewardableGold(account.Gold, rewardInfo.Gold);
            if (deltaGold != 0)
            {
                changedColumns.Add(AccountColumns.Gold, account.Gold + deltaGold);
            }
            excessReward.Gold = Math.Max(rewardInfo.Gold - deltaGold, 0);

            int deltaJewel = GetRewardableJewel(account.Jewel, rewardInfo.Jewel);
            if (deltaJewel != 0)
            {
                changedColumns.Add(AccountColumns.Jewel, account.Jewel + deltaJewel);
            }
            excessReward.Jewel = Math.Max(rewardInfo.Jewel - deltaJewel, 0);

            int deltaStamina = GetRewardableStamina(account.Stamina, rewardInfo.Stamina);
            if (deltaStamina != 0)
            {
                changedColumns.Add(AccountColumns.Stamina, account.Stamina + deltaStamina);
            }

            if (changedColumns.Count > 0)
            {
                rewardModel.ChangedColumns = changedColumns;
                await _accountCache.Change(account, changedColumns.ToDBChanged(), trans);

                Statistics.GameLogger.AccountChange(account, reason, changedColumns, logAndProgress);
                ProgressService.AccountChange(account, changedColumns, logAndProgress);


            }
            List<(int, ushort)> rewardBoxKeys = new List<(int, ushort)>();
            if ((rewardInfo.BoxKeys?.Count ?? 0) != 0)
            {
                (rewardModel.ChangedBoxKeys, excessReward.BoxKeys) = await RewardBoxKeys(rewardInfo.BoxKeys, account, reason, logAndProgress, boxKeys, trans);
            }
            if ((rewardInfo.Consumes?.Count ?? 0) != 0)
            {
                RewardConsumeContext context;
                (context, excessReward.Consumes) = await RewardConsumes(rewardInfo.Consumes, account, reason, logAndProgress, consumes, trans);
                rewardModel.ChangedConsumes = context?.ChangedConsumes;
            }

            if ((rewardInfo.Equipments?.Count ?? 0) != 0)
            {
                var context = await RewardEquipments(rewardInfo.Equipments, account, reason, logAndProgress, equipments, trans);
                rewardModel.ChangedEquipments = context.ChangedEquipments;
            }

            if ((rewardInfo.Heroes?.Count ?? 0) != 0)
            {
                Consume commonSoul = null;
                if (rewardModel.ChangedConsumes != null && rewardModel.ChangedConsumes.Count != 0)
                {
                    //만약 CommonSoul을 RewardConsumes 에서 지급했다면 변경된 정보를 토대로 RewardHeroes를 해야 한다.
                    commonSoul = rewardModel.ChangedConsumes.Find(e => e.Id == StaticInfo.Instance.CommonSoulItemId);
                }
                RewardHeroContext context = await RewardHeroes(rewardInfo.Heroes, account, reason, logAndProgress, heroes, commonSoul, trans);
                rewardModel.ChangedHeros = context.Heroes;
                if (context.CommonSoul != null)
                {
                    if (rewardModel.ChangedConsumes == null)
                    {
                        rewardModel.ChangedConsumes = new List<Consume>();
                    }
                    rewardModel.ChangedConsumes.Apply(context.CommonSoul);
                }
                rewardModel.ChangedCommonSouls = context.ChangedCommonSouls;
                if (context.ExceessCommonSoulCount != 0)
                {
                    excessReward.AddItem(StaticInfo.Instance.CommonSoulItemId, context.ExceessCommonSoulCount);
                }
            }
            rewardModel.ProgressChanges.Put(logAndProgress.GetProgressChanges());
            return (rewardModel, excessReward);
        }

        public async Task<(RewardConsumeContext, List<IntegerIdCountPair>)> RewardConsumes(List<IntegerIdCountPair> rewards, Account account, ChangeReason reason,
            LogAndProgress logAndProgress, IReadOnlyList<Consume> consumes = null, TransactionTask trans = null)
        {
            ulong oidAccount = account.OidAccount;
            if (consumes == null)
            {
                consumes = await _consumeCache.GetAll(oidAccount);
            }

            if (consumes == null)
            {
                consumes = new List<Consume>();
            }
            RewardConsumeContext context = new RewardConsumeContext();
            List<Consume> changedConsumes = new List<Consume>();
            List<Consume> addedConsumes = new List<Consume>();
            List<IntegerIdCountPair> excessConsumes = null;
            foreach (IntegerIdCountPair idCountPair in rewards)
            {
                Consume changed = changedConsumes.Find(c => c.Id == idCountPair.Id);
                Consume added = addedConsumes.Find(c => c.Id == idCountPair.Id);
                if (changed != null || added != null)
                {
                    _logger.Error($"Duplicate Reward Id not allowed, {idCountPair.Id}");
                    return (null, null);
                }

                Consume exist = consumes.FirstOrDefault(c => c.Id == idCountPair.Id);
                int add = GetRewardableConsumeCount(exist?.Count ?? 0, idCountPair.Count);
                if (exist != null)
                {
                    if (add > 0)
                    {
                        changed = new Consume(exist);
                        changed.Count += add;
                        changedConsumes.Add(changed);
                    }
                    logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)add);
                }
                else
                {
                    added = new Consume()
                    {
                        OidAccount = oidAccount,
                        Id = idCountPair.Id,
                        Count = add,
                    };

                    addedConsumes.Add(added);
                    logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)added.Count);
                }
                if (add < idCountPair.Count)
                {
                    if (excessConsumes == null)
                    {
                        excessConsumes = new List<IntegerIdCountPair>();
                    }
                    excessConsumes.Add(new IntegerIdCountPair()
                    {
                        Id = idCountPair.Id,
                        Count = idCountPair.Count - add,
                    });
                }
            }

            foreach (Consume changedConsume in changedConsumes)
            {
                await _consumeCache.Change(consumes.First(c => c.Id == changedConsume.Id), changedConsume, trans);
            }

            if (addedConsumes.Count > 0)
            {
                await _consumeCache.SetAll(addedConsumes, trans);
            }
            changedConsumes.AddRange(addedConsumes);
            context.ChangedConsumes = changedConsumes;
            GameLogger.ConsumeChanges(oidAccount, account.Level, reason.ToString(), consumes, changedConsumes, logAndProgress);
            return (context, excessConsumes);
        }

        public async Task<RewardEquipmentContext> RewardEquipments(List<IntegerIdGradeCount> rewards, Account account, ChangeReason reason,
            LogAndProgress logAndProgress, IReadOnlyList<Equipment> equipments = null, TransactionTask trans = null)
        {
            ulong oidAccount = account.OidAccount;
            RewardEquipmentContext context = new RewardEquipmentContext();
            if (equipments == null)
            {
                equipments = await _equipmentCache.GetAll(oidAccount);
            }

            if (equipments == null)
            {
                equipments = new List<Equipment>();
            }
            IssueSerial itemSerial = await _itemSerialCache.Get(oidAccount);
            IssueSerial newItemSerial = new IssueSerial(itemSerial);

            List<Equipment> addedEquipments = new List<Equipment>();
            foreach (IntegerIdGradeCount idCountPair in rewards)
            {
                for (int i = 0; i < idCountPair.Count; ++i)
                {
                    if (equipments.Count >= MaxEquipmentCount)
                    {
                        _logger.Info(
                            $"{oidAccount.LogOid()} MaxEquipmentCount reached, fail to reward [{idCountPair.Id}:{idCountPair.Count}]");
                        break;
                    }

                    var newEquipment = new Equipment()
                    {
                        OidAccount = oidAccount,
                        Serial = ++newItemSerial.Equipment,
                        Id = idCountPair.Id,
                        Grade = (ushort)idCountPair.Grade,
                        Level = Constants.EquipmentMinLevel,
                        DetailId = 0,
                    };
                    addedEquipments.Add(newEquipment);
                }
            }

            if (itemSerial.Equipment != newItemSerial.Equipment)
            {
                await _itemSerialCache.Change(itemSerial, newItemSerial, trans);
            }

            if (addedEquipments.Count > 0)
            {
                await _equipmentCache.SetAll(addedEquipments, trans);
                logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)addedEquipments.Count);
            }
            Statistics.GameLogger.EquipmentChanges(oidAccount, account.Level, reason.ToString(), addedEquipments, null, logAndProgress);
            context.ChangedEquipments = addedEquipments;
            return context;
        }
        public async Task<RewardHeroContext> RewardHeroes(List<IntegerIdGradeCount> rewards, Account account, ChangeReason reason,
            LogAndProgress logAndProgress, IReadOnlyList<Hero> heroes = null, Consume commonSoul = null, TransactionTask trans = null)
        {
            ulong oidAccount = account.OidAccount;
            if (heroes == null)
            {
                heroes = await _heroCache.GetAll(oidAccount);
            }
            if (commonSoul == null)
            {
                commonSoul = await _consumeCache.Get(oidAccount, Consume.MakeSubKeysWithName(StaticInfo.Instance.CommonSoulItemId));
            }
            Dictionary<int, int> convertCommonCount = new Dictionary<int, int>();
            foreach (IntegerIdGradeCount reward in rewards)
            {
                if (convertCommonCount.ContainsKey(reward.Id) == false)
                {
                    convertCommonCount.Add(reward.Id, 0);
                }
                //kds218 보상에서 Uncommon을 지급 한다고 되어있으면 11레벨 에 해당하는 캐릭터 소울 지급
                var level = StaticInfo.Instance.CharacterGradeLevelRange[reward.Grade].Item1;
                //1레벨은 1개 지급LevelByAccumulateSoulCount는 업그레이드 관련 데이터이기 때문에 1레벨은 0개로 되어있다.
                int accumulateSoulCount = StaticInfo.Instance.RewardSoulCountByLevel(level); //level == 1 ? 1 : StaticInfo.Instance.LevelByAccumulateSoulCount[level];
                convertCommonCount[reward.Id] += reward.Count * accumulateSoulCount;
            }
            RewardHeroContext context = new RewardHeroContext();
            List<Hero> changedHeros = new List<Hero>();
            List<Hero> addedHeros = new List<Hero>();
            Consume changedCommonSoul = null;
            foreach (var reward in convertCommonCount)
            {
                Hero changed = changedHeros.Find(c => c.Id == reward.Key);
                Hero added = addedHeros.Find(c => c.Id == reward.Key);
                if (changed != null || added != null)
                {
                    _logger.Error($"Duplicate Reward Id not allowed, {reward.Key}");
                    return null;
                }

                Hero exist = heroes.FirstOrDefault(c => c.Id == reward.Key);

                if (exist != null)
                {
                    int existLevel = exist.HeroLevel;
                    if (existLevel >= Dash.StaticData.Entity.CharacterInfo.MaxLevel)
                    {
                        var rewardableCommonSoulCount = GetRewardableConsumeCount(commonSoul?.Count ?? 0, reward.Value);
                        if (rewardableCommonSoulCount > 0)
                        {
                            if (changedCommonSoul == null)
                            {
                                changedCommonSoul = new Consume();
                                changedCommonSoul.OidAccount = oidAccount;
                                changedCommonSoul.Id = StaticInfo.Instance.CommonSoulItemId;
                                changedCommonSoul.Count = commonSoul?.Count ?? 0;
                            }
                            changedCommonSoul.Count += rewardableCommonSoulCount;
                            logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)rewardableCommonSoulCount);
                        }
                        context.ChangedCommonSouls.Add(exist.Id, rewardableCommonSoulCount);
                        Math.Max(context.ExceessCommonSoulCount = reward.Value - rewardableCommonSoulCount, 0);
                    }
                    else
                    {
                        //모두 캐릭터 소울로 변경
                        int add = GetRewardableCharacterSoulCount(exist.SoulCount, reward.Value, existLevel);
                        int commonSoulCount = reward.Value - add;
                        if (add > 0)
                        {
                            changed = new Hero(exist);
                            changed.SoulCount += add;
                            changedHeros.Add(changed);
                            logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)add);
                        }
                        if (commonSoulCount > 0)
                        {
                            //CommonSoul로 변경
                            var rewardableCommonSoulCount = GetRewardableConsumeCount(commonSoul?.Count ?? 0, commonSoulCount);
                            if (rewardableCommonSoulCount > 0)
                            {
                                if (changedCommonSoul == null)
                                {
                                    changedCommonSoul = new Consume();
                                    changedCommonSoul.OidAccount = oidAccount;
                                    changedCommonSoul.Id = StaticInfo.Instance.CommonSoulItemId;
                                    changedCommonSoul.Count = commonSoul?.Count ?? 0;
                                }
                                changedCommonSoul.Count += rewardableCommonSoulCount;
                                logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)commonSoulCount);
                            }
                            context.ChangedCommonSouls.Add(exist.Id, rewardableCommonSoulCount);
                            context.ExceessCommonSoulCount = Math.Max(commonSoulCount - rewardableCommonSoulCount, 0);
                        }
                    }
                }
                else
                {

                    int rewardCount = reward.Value;
                    rewardCount = GetRewardableCharacterSoulCount(0, rewardCount, 0);
                    int commonSoulCount = reward.Value - rewardCount;
                    added = new Hero()
                    {
                        OidAccount = oidAccount,
                        Id = reward.Key,
                        HeroLevel = 1,
                        SoulCount = rewardCount,
                        EmblemId = 0,
                    };
                    var characterInfo = StaticInfo.Instance.CharacterInfo[added.Id];
                    if (characterInfo.FirstEmblemGrade == ItemGrade.Common)
                    {
                        added.EmblemId = (ushort)characterInfo.FirstEmblemId;
                    }
                    addedHeros.Add(added);
                    logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)rewardCount);
                }
            }
            if (changedCommonSoul != null)
            {
                if (commonSoul != null)
                {
                    await _consumeCache.Change(commonSoul, changedCommonSoul, trans);
                }
                else
                {
                    await _consumeCache.Set(changedCommonSoul, trans);
                }
                Statistics.GameLogger.ConsumeChange(oidAccount, account.Level, reason.ToString(), commonSoul, changedCommonSoul, logAndProgress);
            }
            foreach (Hero changedHero in changedHeros)
            {
                await _heroCache.Change(heroes.First(c => c.Id == changedHero.Id), changedHero, trans);
            }

            if (addedHeros.Count > 0)
            {
                await _heroCache.SetAll(addedHeros, trans);
            }
            changedHeros.AddRange(addedHeros);
            Statistics.GameLogger.HeroChanges(oidAccount, account.Level, reason.ToString(), heroes, changedHeros, logAndProgress);
            context.Heroes = changedHeros;
            context.CommonSoul = changedCommonSoul;

            return context;
        }
        public async Task<(List<BoxKey>, List<IntegerIdCountPair>)> RewardBoxKeys(List<IntegerIdCountPair> rewards, Account account, ChangeReason reason,
            LogAndProgress logAndProgress, IReadOnlyList<BoxKey> boxKeys = null, TransactionTask trans = null)
        {
            ulong oidAccount = account.OidAccount;
            if (boxKeys == null)
            {
                boxKeys = await _boxKeyCache.GetAll(oidAccount);
            }

            if (boxKeys == null)
            {
                boxKeys = new List<BoxKey>();
            }
            List<BoxKey> changedKeys = new List<BoxKey>();
            List<BoxKey> addedKeys = new List<BoxKey>();
            List<IntegerIdCountPair> excessKeys = new List<IntegerIdCountPair>();
            foreach (var idCount in rewards)
            {
                int boxId = idCount.Id;
                ushort count = (ushort)idCount.Count;
                BoxKey changed = changedKeys.Find(c => c.BoxId == boxId);
                BoxKey added = addedKeys.Find(c => c.BoxId == boxId);
                if (changed != null || added != null)
                {
                    _logger.Error($"[RewardBoxKeys] Duplicate Reward Id not allowed, {boxId}");
                    return (null, null);
                }

                BoxKey exist = boxKeys.FirstOrDefault(c => c.BoxId == boxId);
                ushort add = GetRewardableBoxKeyCount(exist?.Count ?? 0, count);
                if (exist != null)
                {
                    if (add > 0)
                    {
                        changed = new BoxKey(exist);
                        changed.Count += add;
                        changedKeys.Add(changed);
                    }
                    logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)add);
                }
                else
                {
                    added = new BoxKey()
                    {
                        OidAccount = oidAccount,
                        BoxId = boxId,
                        Count = add,
                    };

                    addedKeys.Add(added);
                    logAndProgress.AddChangeProgress(ProgressKey.GainItem, (ulong)added.Count);
                }
                if (add < count)
                {
                    ushort excessCount = (ushort)(count - add);
                    excessKeys.Add(new IntegerIdCountPair() { Id = boxId, Count = excessCount });
                }
            }

            foreach (BoxKey changedKey in changedKeys)
            {
                await _boxKeyCache.Change(boxKeys.First(c => c.BoxId == changedKey.BoxId), changedKey, trans);
            }

            if (addedKeys.Count > 0)
            {
                await _boxKeyCache.SetAll(addedKeys, trans);
            }
            changedKeys.AddRange(addedKeys);
            GameLogger.BoxKeyChanges(oidAccount, account.Level, reason.ToString(), boxKeys, changedKeys, logAndProgress);
            return (changedKeys, excessKeys);
        }
        public async Task<OpenBoxWithTimeRewardResponse> OpenBoxWithTimeReward(ulong oidAccount, int productId)
        {
            OpenBoxWithTimeRewardResponse responseModel = new OpenBoxWithTimeRewardResponse();
            var freeBoxInfo = StaticInfo.Instance.ServiceLogicInfo.Get().TimeRewardBoxInfos.Find(e => e.ProductId == productId);
            if (freeBoxInfo == null)
            {
                return responseModel;
            }
            TimeRewardType rewardType = freeBoxInfo.TimeRewardType;
            (Account account, TimeReward timeReward) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _timeRewardCache.Get(oidAccount, TimeReward.MakeSubKeysWithName(rewardType.ToString())));
            //Todo : 10초 정도의 오차는 허용
            if (timeReward == null || timeReward.RewardTime.AddSeconds(freeBoxInfo.CoolTime) > DateTime.UtcNow)
            {
                return responseModel;
            }
            var trans = DaoCache.Instance.Transaction();
            LogAndProgress logAndProgress = new LogAndProgress(account);

            var boxId = StaticInfo.Instance.ProductInfo[productId].BoxId;
            TimeReward changedTimeReward = new TimeReward(timeReward);
            changedTimeReward.RewardTime = DateTime.UtcNow;
            _timeRewardCache.Change(timeReward, changedTimeReward, trans);

            var openBoxModel = await OpenBox(account, boxId, 1, logAndProgress, trans);
            if (await trans.Execute() == false)
            {
                _logger.Error($"[OpenBoxWithTimeReward]{oidAccount.LogOid()} transaction failed.");
                responseModel.ErrorCode = ErrorCode.DbError;
                return responseModel;
            }
            await logAndProgress.Execute();
            responseModel.OpenBoxModel = openBoxModel;
            responseModel.ChangedTimeReward = changedTimeReward;
            responseModel.ProgressChanges = logAndProgress.GetProgressChanges();
            return responseModel;
        }

        //public async Task<OpenBoxWithKeyResponse> OpenBoxWithKey(ulong oidAccount, ItemGrade keyGrade)
        //{
        //    OpenBoxWithKeyResponse responseModel = new OpenBoxWithKeyResponse();
        //    (Account account, BoxKey boxKey) = await TaskUtility.WaitAll2(
        //        _accountCache.Get(oidAccount), 
        //        _boxKeyCache.Get(oidAccount, BoxKey.MakeSubKeysWithName((int)keyGrade)));

        //    if (boxKey == null || boxKey.Count <= 0)
        //    {
        //        responseModel.ErrorCode = ErrorCode.InternalError;
        //        _logger.Error($"[OpenBox]{oidAccount.LogOid()} Empty KeyData grade : {keyGrade}");
        //        return responseModel;
        //    }

        //    ChangedColumns changeColumns = new ChangedColumns();
        //    var trans = DaoCache.Instance.Transaction();
        //    LogAndProgress logAndProgress = new LogAndProgress(oidAccount);
        //    BoxKey newBoxKey = new BoxKey(boxKey);
        //    newBoxKey.Count -= 1;
        //    await _boxKeyCache.Change(boxKey, newBoxKey, trans);
        //    // TODO: 박스키 소모에대한 로그, Progress 처리

        //    int boxId = StaticInfo.Instance.BoxKeyInfo[keyGrade].RandomBoxId;
        //    var openBoxModel = await OpenBox(oidAccount, boxId, 1, logAndProgress, trans);
        //    if (await trans.Execute() == false)
        //    {
        //        _logger.Error($"[OpenBox]{oidAccount.LogOid()} transaction failed.");
        //        responseModel.ErrorCode = ErrorCode.DbError;
        //        return responseModel;
        //    }
        //    await logAndProgress.Execute();
        //    responseModel.OpenBoxModel = openBoxModel;
        //    responseModel.ChangedBoxKey = newBoxKey;
        //    responseModel.ProgressChanges = logAndProgress.GetProgressChanges();
        //    return responseModel;
        //}
        public async Task<OpenBoxWithKeyResponse> OpenBoxWithKey(ulong oidAccount, int boxId)
        {
            OpenBoxWithKeyResponse responseModel = new OpenBoxWithKeyResponse();
            (Account account, BoxKey boxKey) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _boxKeyCache.Get(oidAccount, BoxKey.MakeSubKeysWithName(boxId)));

            if (boxKey == null || boxKey.Count <= 0)
            {
                responseModel.ErrorCode = ErrorCode.InternalError;
                _logger.Error($"[OpenBox]{oidAccount.LogOid()} Empty KeyData BoxId : {boxId}");
                return responseModel;
            }

            ChangedColumns changeColumns = new ChangedColumns();
            var trans = DaoCache.Instance.Transaction();
            LogAndProgress logAndProgress = new LogAndProgress(account);
            BoxKey newBoxKey = new BoxKey(boxKey);
            newBoxKey.Count -= 1;
            await _boxKeyCache.Change(boxKey, newBoxKey, trans);
            // TODO: 박스키 소모에대한 로그, Progress 처리

            var openBoxModel = await OpenBox(account, boxId, 1, logAndProgress, trans);
            if (await trans.Execute() == false)
            {
                _logger.Error($"[OpenBox]{oidAccount.LogOid()} transaction failed.");
                responseModel.ErrorCode = ErrorCode.DbError;
                return responseModel;
            }
            await logAndProgress.Execute();
            responseModel.OpenBoxModel = openBoxModel;
            responseModel.ChangedBoxKey = newBoxKey;
            responseModel.ProgressChanges = logAndProgress.GetProgressChanges();
            return responseModel;
        }

        public async Task<OpenBoxModel> OpenBox(Account account, int boxId, int boxCount, LogAndProgress logAndProgress, TransactionTask trans)
        {
            ulong oidAccount = account.OidAccount;
            OpenBoxModel responseModel = new OpenBoxModel();
            RewardInfo info = new RewardInfo
            {
                Equipments = new List<IntegerIdGradeCount>(),
                Heroes = new List<IntegerIdGradeCount>()
            };

            (List<Equipment> equipments, List<Hero> heroes, Progress boxProgress) = await TaskUtility.WaitAll3(
            _equipmentCache.GetAll(oidAccount),
            _heroCache.GetAll(oidAccount),
            ProgressService.GetProgress(oidAccount, ProgressKey.OpenBox));
            int changedBoxId = boxId;
            IntegerIdCountKeyType key = new IntegerIdCountKeyType();
            key.Id = boxId;
            for (int i = 0; i < boxCount; ++i)
            {
                key.Count = i + (int)(boxProgress?.Number ?? 0);
                if (StaticInfo.Instance.BoxOverrideInfo.TryGet(key, out var overrideInfo))
                {
                    changedBoxId = overrideInfo.OverrideBoxId;
                }
                (int itemId, ItemGrade selectedGrade) = SimulateOpenBox(equipments, heroes, changedBoxId);
                info.AddItem(itemId, selectedGrade);

                BoxOpenPayLoad logPaylaod = new BoxOpenPayLoad
                {
                    BoxId = boxId,
                    RewardInfo = info,
                };
                logAndProgress.AddGameLog(CommandType.BoxOpen, logPaylaod);
                logAndProgress.AddChangeProgress($"{ProgressKey.Type.OpenBox_}{boxId}", 1);
                logAndProgress.AddChangeProgress(ProgressKey.OpenBox, 1);
            }
            RewardInfo excessReward;
            (responseModel.RewardModel, excessReward) = await GiveReward(account, info, ChangeReason.OpenBox, logAndProgress, trans);
            responseModel.IsSendMail = excessReward != null && excessReward.IsEmpty() == false;
            if (responseModel.IsSendMail)
            {
                //우편으로 보내줌
                await SendMailExcessReward(account.OidAccount, excessReward);
            }
            return responseModel;
        }

        public async Task<RewardInfo> OpenBox(Account account, int boxId, int boxCount)
        {
            ulong oidAccount = account.OidAccount;
            RewardInfo info = new RewardInfo();
            info.Equipments = new List<IntegerIdGradeCount>();
            info.Consumes = new List<IntegerIdCountPair>();

            (List<Equipment> equipments, List<Hero> heroes) = await TaskUtility.WaitAll2(
            _equipmentCache.GetAll(oidAccount),
            _heroCache.GetAll(oidAccount));

            for (int i = 0; i < boxCount; ++i)
            {
                (int itemId, ItemGrade selectedGrade) = SimulateOpenBox(equipments, heroes, boxId);
                info.AddItem(itemId, selectedGrade);
            }
            return info;
        }

        public (int, ItemGrade) SimulateOpenBox(IReadOnlyList<Equipment> equipments, IReadOnlyList<Hero> heroes, int boxId)
        {
            var boxInfo = StaticInfo.Instance.RandomBoxInfo[boxId];
            var probabilityInfo = StaticInfo.Instance.RandomBoxProbabilities[boxInfo.RandomBoxProbabilityId];
            int selectId;

            if (boxInfo.UseAvgGrowth)
            {
                selectId = GetRandomItemId(equipments, heroes, probabilityInfo);
            }
            else
            {
                selectId = ThreadLocalRandom.Choose(probabilityInfo);
            }

            ItemGrade grade = ThreadLocalRandom.Choose(boxInfo.ItemGradeProbabilities);

            return (selectId, grade);
        }

        public async Task<ReceiveChapterAchievementRewardModel> ReceiveChapterAchievementReward(ulong oidAccount, ushort chapterId, ushort targetStage)
        {
            ReceiveChapterAchievementRewardModel model = new ReceiveChapterAchievementRewardModel();
            (Account account, ChapterAchievement chapterAchievement) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _chapterAchievementCache.Get(oidAccount, ChapterAchievement.MakeSubKeysWithName(chapterId))
            );

            if (account == null)
            {
                SetResultAndLog(oidAccount, model, ErrorCode.InternalError, "Account not found!");
                return model;
            }

            if (chapterAchievement == null)
            {
                SetResultAndLog(oidAccount, model, ErrorCode.WrongRequest, "No ChapterAchievement!");
                return model;
            }

            if (chapterAchievement.RewardedStage >= targetStage)
            {
                SetResultAndLog(oidAccount, model, ErrorCode.WrongRequest, "Already rewarded!");
                return model;
            }

            if (StaticInfo.Instance.ChapterAchievementInfo.TryGet(new IntegerIdCountKeyType(chapterId, targetStage),
                    out ChapterAchievementInfo info) == false)
            {
                SetResultAndLog(oidAccount, model, ErrorCode.WrongRequest, "No ChapterAchievementInfo!");
                return model;
            }
            TransactionTask trans = DaoCache.Instance.Transaction();
            LogAndProgress logAndProgress = new LogAndProgress(account);
            RewardInfo excessReward = null;
            (model.RewardModel, excessReward) = await GiveReward(account, info.RewardInfo, ChangeReason.ChapterAchievementReward, logAndProgress, trans);
            if (excessReward != null && excessReward.IsEmpty() == false)
            {
                model.ErrorCode = ErrorCode.OverLimit;
                return model;
            }

            ChapterAchievement newChapterAchievement = new ChapterAchievement(chapterAchievement);
            newChapterAchievement.RewardedStage = targetStage;
            await _chapterAchievementCache.Change(chapterAchievement, newChapterAchievement, trans);

            model.ChapterAchievement = newChapterAchievement;
            if (await trans.Execute() == false)
            {
                _logger.Error("[ReceiveChapterAchievementReward] Transaction failed.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }

            _logger.Info($"{oidAccount.LogOid()} ReceiveChapterAcievementReward.");
            await logAndProgress.Execute();
            return model;
        }
        public async Task<OpenBoxModel> TestOpenBox2(ulong oidAccount, int boxId, int count)
        {
            var account = await _accountCache.Get(oidAccount);
            var trans = _daoCache.Transaction();
            Account account1 = new Account(account);
            account1.Gold += 1;
            await _accountCache.Change(account, account1, trans);
            Account account2 = new Account(account);
            account2.Gold += 2;
            await _accountCache.Change(account, account2, trans);
            if (await trans.Execute() == false)
            {
                _logger.Error("Test Transaction failed.");
            }
            return new OpenBoxModel();
        }

        public async Task<OpenBoxModel> TestOpenBox(ulong oidAccount, int boxId, int count)
        {
            _logger.Info("Start TestOpenBox");
            var account = await _accountCache.Get(oidAccount);
            OpenBoxModel responseModel = new OpenBoxModel();
            var boxInfo = StaticInfo.Instance.RandomBoxInfo[boxId];
            var probabilityInfo = StaticInfo.Instance.RandomBoxProbabilities[boxInfo.RandomBoxProbabilityId];
            int selectId;
            List<Equipment> equipments = null;
            List<Hero> heroes = null;

            RewardInfo rewardInfo = new RewardInfo()
            {
                Equipments = new List<IntegerIdGradeCount>(),
                Heroes = new List<IntegerIdGradeCount>()
            };

            Dictionary<int, int> gainCount = new Dictionary<int, int>();
            Dictionary<ItemGrade, int> gradeCount = new Dictionary<ItemGrade, int>();
            for (int i = 0; i < count; ++i)
            {
                if (boxInfo.UseAvgGrowth)
                {
                    if (equipments == null || heroes == null)
                    {
                        (equipments, heroes) = await TaskUtility.WaitAll2(
                        _equipmentCache.GetAll(oidAccount),
                        _heroCache.GetAll(oidAccount));
                    }
                    selectId = GetRandomItemId(equipments, heroes, probabilityInfo);
                }
                else
                {
                    selectId = ThreadLocalRandom.Choose(probabilityInfo);
                }
                //등급 결정
                ItemGrade selectGrade = ThreadLocalRandom.Choose(boxInfo.ItemGradeProbabilities);
                rewardInfo.AddItem(selectId, selectGrade);
                if (gainCount.ContainsKey(selectId) == false)
                {
                    gainCount.Add(selectId, 0);
                }
                gainCount[selectId] += 1;
                if (gradeCount.ContainsKey(selectGrade) == false)
                {
                    gradeCount.Add(selectGrade, 0);
                }
                gradeCount[selectGrade] += 1;
            }
            int totalCount = rewardInfo.Heroes.Sum(e => e.Count) + rewardInfo.Equipments.Sum(e => e.Count);
            rewardInfo.Heroes.Sort((a, b) =>
            {
                if (a.Id < b.Id)
                    return -1;
                return 1;
            });
            for (int i = 0; i < rewardInfo.Heroes.Count; ++i)
            {
                _logger.Info($"[GainHeroes][{rewardInfo.Heroes[i].Id}][{rewardInfo.Heroes[i].Grade}][{rewardInfo.Heroes[i].Count}][{(double)rewardInfo.Heroes[i].Count / totalCount}]");
            }
            rewardInfo.Equipments.Sort((a, b) =>
            {
                if (a.Id < b.Id)
                    return -1;
                return 1;
            });
            for (int i = 0; i < rewardInfo.Equipments.Count; ++i)
            {
                _logger.Info($"[GainEquipments][{rewardInfo.Equipments[i].Id}][{rewardInfo.Equipments[i].Grade}][{rewardInfo.Equipments[i].Count}][{(double)rewardInfo.Equipments[i].Count / totalCount}]");
            }
            totalCount = gainCount.Sum(e => e.Value);
            foreach (var pair in gainCount.OrderBy(e => e.Key))
            {
                _logger.Info($"[GainCount][{pair.Key}][{pair.Value}][{(double)pair.Value / totalCount}]");
            }
            totalCount = gradeCount.Sum(e => e.Value);
            foreach (var pair in gradeCount.OrderBy(e => e.Key))
            {
                _logger.Info($"[GradeCount][{pair.Key}][{pair.Value}][{(double)pair.Value / totalCount}]");
            }
            LogAndProgress logAndProgress = new LogAndProgress(account);
            (RewardModel rewardModel, RewardInfo mail) = await GiveReward(oidAccount, rewardInfo, ChangeReason.OpenBox, logAndProgress);
            responseModel.RewardModel = rewardModel;
            await logAndProgress.Execute();
            return responseModel;
        }

        //여기서의 아이템은 장비와 캐릭터를 말한다.
        //probabilities가 null이면 그냥 1/n확률
        public Dictionary<int, int> GetItemWeight(IReadOnlyList<Equipment> equipments, IReadOnlyList<Hero> heroes, IReadOnlyDictionary<int, double> probabilities = null)
        {
            Dictionary<int/*equipmentId*/, int/*weight*/> equipmentsWeight = new Dictionary<int, int>();
            //모든 아이템의 아이템별 가치는 Common 기준으로 계산한다.Common : 1, Legendary : 243

            var equipmentInfos = StaticInfo.Instance.EquipmentInfo.GetInfos();

            //아이템을 소지하지 않고 있다면 최대 가중치를 적용한다.
            var maxWeight = StaticInfo.Instance.GradeByEquipmentWeight[ItemGrade.Legendary];
            foreach (var pair in equipmentInfos)
            {
                if (probabilities == null || (probabilities.ContainsKey(pair.Value.Id) && probabilities[pair.Value.Id] != 0))
                {
                    equipmentsWeight.Add(pair.Value.Id, maxWeight);
                }
            }
            //캐릭터 정보도 넣어준다. 캐릭터와 장비는 아이디가 겹치지 않는다.
            if (heroes != null)
            {
                foreach (var pair in StaticInfo.Instance.CharacterInfo.GetInfos())
                {
                    if (probabilities == null || (probabilities.ContainsKey(pair.Value.Key) && probabilities[pair.Value.Key] != 0))
                    {
                        equipmentsWeight.Add(pair.Value.Key, maxWeight);
                    }
                }
            }
            //소지하고 있는 아이템은 그만큼의 가중치를 뺀다
            for (int i = 0; equipments != null && i < equipments.Count; ++i)
            {
                int equipmentId = equipments[i].Id;
                ItemGrade grade = EnumInfo<ItemGrade>.ConvertUshort(equipments[i].Grade);
                if (grade == ItemGrade.Undefined)
                {
                    continue;
                }

                int weight = StaticInfo.Instance.GradeByEquipmentWeight[grade];
                if (equipmentsWeight.ContainsKey(equipmentId) == false)
                {
                    continue;
                }
                weight = Math.Max(0, equipmentsWeight[equipmentId] - weight);
                equipmentsWeight[equipmentId] = weight;
            }
            for (int i = 0; heroes != null && i < heroes.Count; ++i)
            {
                var hero = heroes[i];
                ItemGrade grade = StaticInfo.Instance.CharacterInfo[hero.Id][hero.HeroLevel].Grade;
                if (grade == ItemGrade.Undefined)
                {
                    continue;
                }

                int heroId = heroes[i].Id;
                int weight = StaticInfo.Instance.GradeByEquipmentWeight[grade] * (hero.SoulCount + 1);
                if (equipmentsWeight.ContainsKey(heroId) == false)
                {
                    continue;
                }
                weight = Math.Max(0, equipmentsWeight[heroId] - weight);
                equipmentsWeight[heroId] = weight;
            }
            return equipmentsWeight;
        }
        public int GetRandomItemId(IReadOnlyList<Equipment> equipments, IReadOnlyList<Hero> heroes, IReadOnlyDictionary<int, double> probabilities = null)
        {
            Dictionary<int, int> itemWeights = GetItemWeight(equipments, heroes, probabilities);
            int totalGainWeight = itemWeights.Sum(e => e.Value);

            double avgGrowthRate = StaticInfo.Instance.ServiceLogicInfo.Get().AvgGrowthRate;
            double defaultDropProbability = 1.0 / itemWeights.Count;
            Dictionary<int/*id*/, double/*probability*/> itemProbability = new Dictionary<int, double>();
            foreach (var weight in itemWeights)
            {
                double dropProbability = probabilities == null ? defaultDropProbability : (probabilities.ContainsKey(weight.Key) ? probabilities[weight.Key] : 0);
                double weightRate = (double)weight.Value / totalGainWeight;
                var actualRate = weightRate * avgGrowthRate + dropProbability * (1 - avgGrowthRate);
                itemProbability.Add(weight.Key, actualRate);
            }

            return ThreadLocalRandom.Choose(itemProbability);
        }

        public async Task<ReceiveMissionRewardResponse> ReceiveMissionReward(ulong oidAccount, int missionId)
        {
            var response = new ReceiveMissionRewardResponse();
            if (StaticInfo.Instance.MissionInfos.TryGetValue(missionId, out var info) == false)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = $"MissionInfo not found : {missionId}";
                return response;
            }

            (Account account, CompletedMission mission) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _completedMissionCache.Get(oidAccount, CompletedMission.MakeSubKeysWithName(missionId))
            );

            if (account == null)
            {
                response.ErrorCode = ErrorCode.DbError;
                response.ErrorText = $"Account not found : {oidAccount}";
                return response;
            }

            if (mission == null)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = $"Mission not completed : {missionId}";
                return response;
            }

            if (mission.IsRewarded == true)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = $"Mission already rewarded : {missionId}";
                return response;
            }

            var transaction = DaoCache.Instance.Transaction();
            var changed = new CompletedMission(mission);
            changed.IsRewarded = true;
            response.CompletedMission = changed;
            await _completedMissionCache.Change(mission, changed, transaction);

            if (info.RewardInfo != null)
            {
                RewardInfo mail = null;
                (response.RewardModel, mail) = await GiveReward(oidAccount, info.RewardInfo,
                     ChangeReason.MissionReward, new LogAndProgress(account), transaction);
            }

            if (await transaction.Execute() == false)
            {
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }

            return response;
        }

        public async Task SendMailExcessReward(ulong oidAccount, RewardInfo excessReward)
        {
            _mailService.SendMailExcessReward(oidAccount, excessReward);
        }
    }
}