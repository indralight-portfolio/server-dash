using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Utility;
using Dash;
using Dash.Model.Rdb;
using Dash.StaticInfo;
using Dash.Types;
using MySql.Data.MySqlClient;
using NLog;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using Dash.Model.Service;
using Dash.StaticData;

namespace server_dash.Lobby.Services
{
    public class AccountCreateResult
    {
        public string DeviceId;
        public ulong OidAccount;
    }

    public static class AccountCreate
    {
        private static readonly ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private static DaoCache _daoCache;        
        private static ISingleDBCache<IssueSerial> _issueSerialCache;
        private static ISingleDBCache<StaticAccount> _staticAccountCache;
        private static ISingleDBCache<Footprint> _footprintCache;
        private static IMultipleDBCache<Auth> _authCache;
        private static IMultipleDBCache<EquipmentSlot> _equipmentSlotCache;
        private static IMultipleDBCache<Hero> _heroCache;
        private static IMultipleDBCache<Talent> _talentCache;
        private static IMultipleDBCache<TimeReward> _timeRewardCache;

        private static CheatConfig _cheatConfig;

        private static int DefaultHero = 3;
        private static int InitGold = 0;
        private static int InitJewel = 0;

        private static ServiceLogicInfo _serviceLogicInfo;

        // @. For Cheat
        #region Cheat
        private static IMultipleDBCache<Equipment> _equipmentCache;
        private static IMultipleDBCache<Consume> _consumeCache;
        #endregion

        public static void Init(DaoCache daoCache)
        {
            _cheatConfig = ConfigManager.Get<CheatConfig>(Config.Cheat);

            _daoCache = daoCache;
            _issueSerialCache = daoCache.GetSingle<IssueSerial>();
            _staticAccountCache = daoCache.GetSingle<StaticAccount>();
            _footprintCache = daoCache.GetSingle<Footprint>();
            _authCache = daoCache.GetMultiple<Auth>();
            _equipmentSlotCache = daoCache.GetMultiple<EquipmentSlot>();
            _heroCache = daoCache.GetMultiple<Hero>();
            _talentCache = daoCache.GetMultiple<Talent>();
            _timeRewardCache = daoCache.GetMultiple<TimeReward>();

            _serviceLogicInfo = StaticInfo.Instance.ServiceLogicInfo.Get();

            // @. For Cheat
            #region Cheat
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _consumeCache = daoCache.GetMultiple<Consume>();
            if (_cheatConfig.GiveTestJewelWhenCreate == true)
            {
                InitJewel = 1000;
            }
            #endregion
        }

        public static async Task<Auth> ExecuteAsync(AuthReq authReq)
        {
            try
            {
                // 1. Account, Auth
                if (string.IsNullOrEmpty(authReq.AuthId) == true)
                {
                    _logger.Error($"[AccountCreate] DeviceId is null");
                    return null;
                }
                Account account = new Account();
                using (MySqlConnection connection = _daoCache.GameDao.Connector.GetConnection())
                using (MySqlCommand command = new MySqlCommand("INSERT INTO Account (UsingHeroId, Gold, Jewel, Stamina, Exp) VALUES (@UsingHeroId, @Gold, @Jewel, @Stamina, @Exp)", connection))
                {
                    command.Parameters.AddWithValue("@UsingHeroId", 3); // 다인이 시작캐
                    command.Parameters.AddWithValue("@Gold", InitGold);
                    command.Parameters.AddWithValue("@Jewel", InitJewel);
                    command.Parameters.AddWithValue("@Stamina", _serviceLogicInfo.MaxStamina);
                    command.Parameters.AddWithValue("@Exp", 0);
                    await command.Connection.OpenAsync().ConfigureAwait(false);
                    await command.PrepareAsync().ConfigureAwait(false);
                    await command.ExecuteReaderAsync().ConfigureAwait(false);

                    account.OidAccount = (ulong)command.LastInsertedId;
                    account.Level = 1;

                    _logger.Info($"{account.OidAccount.LogOid()} Account create, DeviceId : {authReq.AuthId}");
                }

                TransactionTask trans = DaoCache.Instance.Transaction();

                // 1. Auth
                Auth auth = new Auth(authReq);
                auth.OidAccount = account.OidAccount;
                await _authCache.Set(auth, trans);
                
                // 2. Footprint
                Footprint footprint = new Footprint()
                {
                    OidAccount = account.OidAccount,
                    Created = DateTime.UtcNow,
                    LatestLogon = DateTime.UtcNow,
                };
                await _footprintCache.Set(footprint, trans);

                // 3. StaticAccount
                StaticAccount staticAccount = new StaticAccount
                {
                    OidAccount = account.OidAccount,
                    TimeOffset = authReq.TimeOffset,
                    Country = authReq.Country,
                    Nickname = string.Empty,
                };
                await _staticAccountCache.Set(staticAccount, trans); ;

                // 3. ItemSerial
                IssueSerial issueSerial = new IssueSerial()
                {
                    OidAccount = account.OidAccount,
                    Equipment = 0,
                };
                await _issueSerialCache.Set(issueSerial, trans);

                // 4. EquipmentSlot
                await _equipmentSlotCache.SetAll(Enumerable.Range(0, EnumInfo<EquipmentSlotType>.Count).Select(i => new EquipmentSlot()
                {
                    OidAccount = account.OidAccount,
                    SlotIndex = (ushort)i,
                    EquipmentSerial = 0,
                }).ToList(), trans);

                // 5. Hero
                List<int> heroIds;
                if (_cheatConfig.GiveAllHeroWhenCreate == true)
                {
                    heroIds = StaticInfo.Instance.CharacterInfo.GetList().Select(i => i.Id).ToList();
                }
                else
                {
                    // 다인만 지급.
                    heroIds = new List<int>() { DefaultHero };
                }
                List<Hero> heroModels = new List<Hero>();
                for(int i = 0; i < heroIds.Count; ++i)
                {
                    var characterInfo = StaticInfo.Instance.CharacterInfo[heroIds[i]];
                    var model = new Hero()
                    {
                        OidAccount = account.OidAccount,
                        Id = characterInfo.Id,
                        HeroLevel = 1,
                        SoulCount = 0,
                        EmblemId = 0,
                    };
                    if(characterInfo.FirstEmblemGrade == ItemGrade.Common)
                    {
                        model.EmblemId = (ushort)characterInfo.FirstEmblemId;
                    }
                    heroModels.Add(model);
                }
                await _heroCache.SetAll(heroModels, trans);

                // 6. Talent
                await _talentCache.SetAll(StaticInfo.Instance.TalentInfo.GetList().Select(i => new Talent()
                {
                    OidAccount = account.OidAccount,
                    Id = (int)i.Type,
                    Level = i.Type == TalentType.Glory ? 1 : 0
                }).ToList(), trans);

                // 7. TimeReward
                List<TimeReward> timeRewards = new List<TimeReward>();
                timeRewards.Add(new TimeReward()
                {
                    OidAccount = account.OidAccount,
                    RewardType = TimeRewardType.RestoreStamina.ToString(),
                    Count = 0,
                    RewardTime = DateTime.UtcNow
                });
                await _timeRewardCache.SetAll(timeRewards, trans);

                // @. For Cheat
                #region Cheat
                if (_cheatConfig.GiveTestItemWhenCreate == true)
                {
                    uint serial = 0;
                    List<Equipment> equipments = new List<Equipment>();
                    foreach (var id in new int[] { 101001, 102001, 103001, 104001 })
                    {
                        foreach (var grade in EnumInfo<ItemGrade>.GetValues())
                        {
                            if (grade <= ItemGrade.Undefined) continue;
                            for (int i = 0; i < 3; ++i)
                            {
                                equipments.Add(
                                    new Equipment { OidAccount = account.OidAccount, Serial = ++serial, Id = id, Grade = (ushort)grade, Level = Dash.Constants.EquipmentMinLevel }
                                );
                            }
                        }
                    }
                    IssueSerial newIssueSerial = new IssueSerial(issueSerial);
                    newIssueSerial.Equipment = serial;

                    List<Consume> consumes = new List<Consume>();
                    foreach (var id in new int[] { 20001, 20002, 20003, 20004 })
                    {
                        consumes.Add(
                            new Consume { OidAccount = account.OidAccount, Id = id, Count = 250 }
                        );
                    }

                    await _issueSerialCache.Change(issueSerial, newIssueSerial, trans);
                    await _equipmentCache.SetAll(equipments, trans);
                    await _consumeCache.SetAll(consumes, trans);
                }
                #endregion

                bool transactionSucceed = await trans.Execute();
                if (transactionSucceed == false)
                {
                    _logger.Error($"{account.OidAccount.LogOid()} Transaction failed!");
                }
                _logger.Info($"{account.OidAccount.LogOid()} Account create done.");

                Statistics.GameLogger.OnCreate(account.OidAccount, account.Level, staticAccount.Country);

                return auth;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Exception occured during account create.");
                throw;
            }
        }
    }
}