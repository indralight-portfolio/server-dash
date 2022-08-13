using System.Threading.Tasks;
using System.Collections.Generic;
using NLog;
using Dash.Model.Rdb;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using System.Linq;
using Dash;

namespace server_dash.Internal.Services
{
    public static class InventoryService
    {
        private static readonly ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private static ISingleDBCache<Account> _accountCache;
        private static IMultipleDBCache<Hero> _characterCache;
        private static IMultipleDBCache<Equipment> _equipmentCache;
        private static IMultipleDBCache<EquipmentSlot> _equipmentSlotCache;

        public static void Init(DaoCache daoCache)
        {
            _accountCache = daoCache.GetSingle<Account>();
            _characterCache = daoCache.GetMultiple<Hero>();
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _equipmentSlotCache = daoCache.GetMultiple<EquipmentSlot>();
        }

        public static async Task<Hero> GetUsingHero(ulong oidAccount, Account account = null)
        {
            if (account == null)
            {
                account = await _accountCache.Get(oidAccount);
            }
            if (account == null)
            {
                _logger.Error($"{oidAccount.LogOid()} no Account in DB!");
                return null;
            }

            Hero character = await _characterCache.Get(oidAccount, Hero.MakeSubKeysWithName(account.UsingHeroId));
            if (character == null)
            {
                _logger.Error($"{oidAccount.LogOid()} character not found, Id : {account.UsingHeroId}");
                return null;
            }

            return character;
        }

        public static async Task<List<Equipment>> GetEquipmentsInSlot(ulong oidAccount, List<EquipmentSlot> slots = null)
        {
            if (slots == null)
            {
                slots = await _equipmentSlotCache.GetAll(oidAccount);
            }
            if (slots == null)
            {
                _logger.Error($"{oidAccount.LogOid()} no equipment slots!");
                return null;
            }
            var subKeys = slots.FindAll(e => e.EquipmentSerial != 0).Select(e => e.EquipmentSerial.ToString()).ToArray();
            List<Equipment> equipments = null;
            if (subKeys != null && subKeys.Length != 0)
            {
                equipments = await _equipmentCache.GetList(oidAccount, subKeys);
            }
            return equipments ?? new List<Equipment>();
        }

        public static async Task<bool> UpdateConsume(ulong oidAccount, IMultipleDBCache<Consume> consumeCache, Consume origin, Consume changed, TransactionTask trans = null)
        {
            if ((origin == null || origin.Count == 0) && changed != null && changed.Count != 0)
            {
                //set
                return await consumeCache.Set(changed, trans);
            }
            else if (origin != null && origin.Count != 0 && changed != null && changed.Count != 0)
            {
                return await consumeCache.Change(origin, changed, trans);
            }
            else if (origin != null && origin.Count != 0 && (changed == null || changed.Count == 0))
            {
                return await consumeCache.Del(origin, trans);
            }
            //로그 필요
            _logger.Error($"[UpdateConsume]{oidAccount.LogOid()} nothing happen.");
            return false;
        }
    }
}