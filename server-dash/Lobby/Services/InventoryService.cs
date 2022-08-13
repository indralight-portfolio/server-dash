using Common.Utility;
using Dash.Model.Rdb;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticData.Entity;
using Dash.StaticInfo;
using Dash.Types;
using System.Threading.Tasks;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Lobby.Services
{
    public class InventoryService : BaseService
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

        public InventoryService(DaoCache daoCache)
        {
            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _authCache = daoCache.GetSingle<Auth>();
            _accountCache = daoCache.GetSingle<Account>();
            _heroCache = daoCache.GetMultiple<Hero>();
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _equipmentSlotCache = daoCache.GetMultiple<EquipmentSlot>();
            _consumeCache = daoCache.GetMultiple<Consume>();
        }

        public async Task<UpdateUsingHeroResponse> UpdateUsingHero(ulong oidAccount, int heroId)
        {
            UpdateUsingHeroResponse response = new UpdateUsingHeroResponse();
            response.HeroId = heroId;

            (Account account, Hero hero) = await TaskUtility.WaitAll2(_accountCache.Get(oidAccount),
                _heroCache.Get(oidAccount, Hero.MakeSubKeysWithName(heroId)));

            if (account == null || hero == null)
            {
                response.SetResult(ErrorCode.InternalError, $"Account or Hero not found, Account null : {account == null}, Hero null : {hero == null}");
                return response;
            }

            Account newAccount = new Account(account);
            newAccount.UsingHeroId = heroId;
            await _accountCache.Change(account, newAccount);

            return response;
        }

        public async Task<UpdateEquipmentSlotResponse> UpdateEquipmentSlot(ulong oidAccount, uint equipmentSerial, int slotIndex)
        {
            UpdateEquipmentSlotResponse response = new UpdateEquipmentSlotResponse();
            Equipment equipment = null;
            EquipmentSlot slot = null;
            if (equipmentSerial == 0)
            {
                slot = await _equipmentSlotCache.Get(oidAccount, EquipmentSlot.MakeSubKeysWithName(slotIndex));

            }
            else
            {
                (equipment, slot) = await TaskUtility.WaitAll2(_equipmentCache.Get(oidAccount, Equipment.MakeSubKeysWithName(equipmentSerial)),
                    _equipmentSlotCache.Get(oidAccount, EquipmentSlot.MakeSubKeysWithName(slotIndex)));
            }

            if (slot == null)
            {
                response.SetResult(ErrorCode.WrongRequest, $"No equipment slot found, Index : {slotIndex}");
                return response;
            }

            if (equipmentSerial != 0)
            {
                if (equipment == null)
                {
                    response.SetResult(ErrorCode.WrongRequest, $"No equipment found, Serial : {equipmentSerial}");
                    return response;
                }
                var equipmentInfo = StaticInfo.Instance.EquipmentInfo[equipment.Id];
                if (equipmentInfo == null)
                {
                    response.SetResult(ErrorCode.WrongRequest, $"Invalid Id : {equipment.Id}");
                    return response;
                }
                //해당 아이템이 슬롯에 착용할수 있는지 검증
                var slots = EquipmentTypeHelper.GetEquipmentSlots(equipmentInfo.Type);
                var slotType = EnumInfo<EquipmentSlotType>.ConvertInt(slotIndex);
                if (slots.Contains(slotType) == false)
                {
                    response.SetResult(ErrorCode.WrongRequest, $"Invalid SlotIndex.");
                    return response;
                }
            }
            
            EquipmentSlot newSlot = new EquipmentSlot(slot);
            newSlot.EquipmentSerial = equipmentSerial;
            await _equipmentSlotCache.Change(slot, newSlot);
            response.EquipmentSerial = equipmentSerial;
            response.SlotIndex = slotIndex;
            return response;
        }

        public async Task<UpdateEmblemResponse> UpdateEmblem(ulong oidAccount, int heroId, int emblemId)
        {
            UpdateEmblemResponse response = new UpdateEmblemResponse();
            response.HeroId = heroId;
            response.EmblemId = emblemId;

            Hero hero = await _heroCache.Get(oidAccount, Hero.MakeSubKeysWithName(heroId));
            if (hero == null)
            {
                response.SetResult(ErrorCode.WrongRequest, $"Hero {heroId} not found.");
                return response;
            }
            CharacterInfo heroInfo = StaticInfo.Instance.CharacterInfo[heroId];
            // 0은 장착 해제로 취급.
            if (emblemId != 0 && heroInfo.LevelDatas[hero.HeroLevel].EmblemIds.Contains(emblemId) == false)
            {
                response.SetResult(ErrorCode.WrongRequest, $"Emblem {emblemId} is not usable.");
                return response;
            }

            Hero newHero = new Hero(hero);
            newHero.EmblemId = (ushort)emblemId;
            await _heroCache.Change(hero, newHero);

            return response;
        }
    }
}