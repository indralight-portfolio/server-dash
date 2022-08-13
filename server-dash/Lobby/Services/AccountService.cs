using Common.Utility;
using Dash;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticData;
using Dash.StaticData.Mission;
using Dash.StaticInfo;
using Dash.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Lobby.Services
{
    public class AccountService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private DaoCache _daoCache;
        private IMemCache _memCache;
        private IMultipleDBCache<Auth> _authCache;
        private ISingleDBCache<Account> _accountCache;
        private ISingleDBCache<StaticAccount> _staticAccountCache;
        private ISingleDBCache<Footprint> _footprintCache;

        private IMultipleDBCache<Equipment> _equipmentCache;
        private IMultipleDBCache<EquipmentSlot> _equipmentSlotCache;
        private IMultipleDBCache<Hero> _characterCache;
        private IMultipleDBCache<Consume> _consumeCache;
        private IMultipleDBCache<Talent> _talentCache;
        private IMultipleDBCache<TimeReward> _timeRewardCache;
        private IMultipleDBCache<ChapterAchievement> _chapterAcievementCache;
        private IMultipleDBCache<BoxKey> _boxKeyCache;
        private IMultipleDBCache<CompletedMission> _completedMissionCache;

        private CheatConfig _cheatConfig;
        private ServiceLogicInfo _serviceLogicInfo;

        private const string VerifyKeyPrefix = "vf";
        private const int VerifyKeyExpireSecconds = 600;
        private const int SessionKeyExpireSeconds = 28800;

        private MailService _mailService;

        public AccountService(DaoCache daoCache, MailService mailService)
        {
            AccountCreate.Init(daoCache);

            _cheatConfig = ConfigManager.Get<CheatConfig>(Config.Cheat);
            _serviceLogicInfo = StaticInfo.Instance.ServiceLogicInfo.Get();

            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _authCache = daoCache.GetMultiple<Auth>();
            _accountCache = daoCache.GetSingle<Account>();
            _staticAccountCache = daoCache.GetSingle<StaticAccount>();
            _footprintCache = daoCache.GetSingle<Footprint>();

            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _equipmentSlotCache = daoCache.GetMultiple<EquipmentSlot>();
            _characterCache = daoCache.GetMultiple<Hero>();
            _consumeCache = daoCache.GetMultiple<Consume>();
            _talentCache = daoCache.GetMultiple<Talent>();
            _timeRewardCache = daoCache.GetMultiple<TimeReward>();
            _chapterAcievementCache = daoCache.GetMultiple<ChapterAchievement>();
            _boxKeyCache = daoCache.GetMultiple<BoxKey>();
            _completedMissionCache = daoCache.GetMultiple<CompletedMission>();
            _mailService = mailService;
        }

        public async Task SetVerifySocial(AuthReq authReq)
        {
            string key = $"{VerifyKeyPrefix}:{authReq.AuthType.ToString()}:{authReq.AuthId}";
            await _memCache.StringSet(key, true.ToString(), VerifyKeyExpireSecconds);
        }

        public async Task<CheckAuthResponse> CheckAuth(AuthReq authReq, AuthReq authReq_o)
        {
            if (authReq == null)
            {
                return new CheckAuthResponse { ErrorCode = ErrorCode.InvalidParameter };
            }
            else if (authReq.IsSocial)
            {
                // 소셜 계정일 경우 Verify 되었는지 확인
                string key = $"{VerifyKeyPrefix}:{authReq.AuthType.ToString()}:{authReq.AuthId}";
                bool.TryParse(await _memCache.StringGet(key), out bool result);
                if (result == false)
                {
                    return new CheckAuthResponse { ErrorCode = ErrorCode.InternalError };
                }
            }

            Auth auth;
            if (authReq_o != null)
            {
                Auth auth_o = await _authCache.Get(authReq_o.AuthId, Auth.MakeSubKeysWithName((byte)authReq_o.AuthType));
                if (auth_o != null)
                {
                    // 이전 인증이 있고, 이전 인증에 계정이 존재한다면 Overwrite 처리
                    auth = await _authCache.Get(authReq.AuthId, Auth.MakeSubKeysWithName((byte)authReq.AuthType));
                    if (auth == null)
                    {
                        auth = new Auth(authReq);
                        auth.OidAccount = auth_o.OidAccount;
                        await _authCache.Set(auth);
                    }
                    else
                    {
                        Auth auth_n = new Auth(auth);
                        auth_n.OidAccount = auth_o.OidAccount;
                        await _authCache.Change(auth, auth_n);
                        auth = auth_n;
                    }

                    return new CheckAuthResponse
                    {
                        Result = CheckAuthResponse.ResultType.Overwrite_Exsisting_Account,
                        AuthReq = new AuthReq(auth),
                    };
                }
            }

            // 저장된 데이터가 존재하는지 체크
            auth = await _authCache.Get(authReq.AuthId, Auth.MakeSubKeysWithName((byte)authReq.AuthType));
            if (auth != null)
            {
                return new CheckAuthResponse
                {
                    Result = CheckAuthResponse.ResultType.Load_Exsisting_Account,
                    AuthReq = new AuthReq(auth),
                };
            }
            else
            {
                //신규 생성
                auth = await AccountCreate.ExecuteAsync(authReq);

                return new CheckAuthResponse
                {
                    Result = CheckAuthResponse.ResultType.Create_New_Account,
                    AuthReq = new AuthReq(auth),
                };
            }
        }

        public async Task<LoginResponse> Login(AuthReq authReq)
        {
            if (authReq == null)
            {
                return new LoginResponse { ErrorCode = ErrorCode.InvalidParameter };
            }
            else if (authReq.IsSocial)
            {
                // 소셜 계정일 경우 Verify 되었는지 확인
                string key = $"{VerifyKeyPrefix}:{authReq.AuthType.ToString()}:{authReq.AuthId}";
                bool.TryParse(await _memCache.StringGet(key), out bool result);
                if (result == false)
                {
                    _logger.Error($"SocialAccount not verified, [authReq: {authReq}]");
                    return new LoginResponse { ErrorCode = ErrorCode.InternalError };
                }
            }

            _logger.Info($"Start Login, [authReq:{authReq}]");
            Auth auth = null;
            try
            {
               auth = await _authCache.Get(authReq.AuthId, Auth.MakeSubKeysWithName((byte)authReq.AuthType));
            }
            catch (Exception e)
            {
                _logger.Fatal($"Get Auth failed : {authReq}");
                throw;
            }
            Common.Model.SessionKey sessionKey = null;

            if (auth == null)
            {
                if (_cheatConfig.AutoAccountCreate)
                {
                    //신규 생성
                    auth = await AccountCreate.ExecuteAsync(authReq);
                }
                else
                {
                    _logger.Error($"No Auth, [authReq:{authReq}]");
                    return new LoginResponse { ErrorCode = ErrorCode.NoAuth };
                }
            }

            (Account account, StaticAccount staticAccount) = await TaskUtility.WaitAll2(_accountCache.Get(auth.OidAccount), _staticAccountCache.Get(auth.OidAccount));
            if (account == null || staticAccount == null)
            {
                _logger.Error($"Account, StaticAccount not found, {auth.OidAccount.LogOid()}");
                return new LoginResponse { ErrorCode = ErrorCode.NoAccount };
            }

            _logger.Info($"Login, {auth.OidAccount.LogOid()}[nick:{staticAccount.Nickname}]");

            var changeColumns = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>(nameof(staticAccount.TimeOffset), authReq.TimeOffset),
            };
            _staticAccountCache.Change(staticAccount, changeColumns);

            sessionKey = await StoreSessionKey(auth.OidAccount);
            return new LoginResponse
            {
                OidAccount = auth.OidAccount,
                SessionKey = sessionKey,
            };
        }

        private async Task<Common.Model.SessionKey> StoreSessionKey(ulong oidAccount)
        {
            string key = UniqueIdenifier.GenerateSessionKey(DateTime.UtcNow);
            await _memCache.StringSet(SessionKey.RedisKeyPrefix + oidAccount, key, SessionKeyExpireSeconds);
            return new Common.Model.SessionKey()
            {
                Key = key,
                Expiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds(SessionKeyExpireSeconds))
            };
        }

        public async Task<PlayerInfoResponse> GetPlayerInfo(ulong oidAccount)
        {
            PlayerInfoResponse response = new PlayerInfoResponse();

            (Account account,
                    StaticAccount staticAccount,
                    Footprint footprint,
                    List<Hero> characters,
                    List<EquipmentSlot> equipmentSlots,
                    List<Equipment> equipments,
                    List<Consume> consumes,
                    List<Talent> talents,
                    List<TimeReward> timeRewards,
                    List<ChapterAchievement> chapterAchievements,
                    List<BoxKey> boxKeys) =
                await TaskUtility.WaitAll11(
                    _accountCache.Get(oidAccount),
                    _staticAccountCache.Get(oidAccount),
                    _footprintCache.Get(oidAccount),
                    _characterCache.GetAll(oidAccount),
                    _equipmentSlotCache.GetAll(oidAccount),
                    _equipmentCache.GetAll(oidAccount),
                    _consumeCache.GetAll(oidAccount),
                    _talentCache.GetAll(oidAccount),
                    _timeRewardCache.GetAll(oidAccount),
                    _chapterAcievementCache.GetAll(oidAccount),
                    _boxKeyCache.GetAll(oidAccount)
                    );

            if(account == null || footprint == null)
            {
                response.SetResult(ErrorCode.NoAccount);
                return response;
            }

            var newFootprint = footprint.Touch();
            _footprintCache.Change(footprint, newFootprint);

            response.Account = account;
            response.StaticAccount = staticAccount;
            response.Heros = characters ?? new List<Hero>();
            response.EquipmentSlots = equipmentSlots ?? new List<EquipmentSlot>();
            response.Equipments = equipments ?? new List<Equipment>();
            response.Consumes = consumes ?? new List<Consume>();
            response.Talents = talents ?? new List<Talent>();
            response.TimeRewards = timeRewards ?? new List<TimeReward>();
            response.ChapterAchievements = chapterAchievements ?? new List<ChapterAchievement>();
            response.BoxKeys = boxKeys ?? new List<BoxKey>();

            response.UndoneGameHash = await _memCache.StringGet("undone:" + oidAccount);

            await _mailService.SendTemplateMail(oidAccount);
            response.MailCount = (await _mailService.List(oidAccount)).MailList.Count;

            //account.Level
            Statistics.GameLogger.OnLogIn(oidAccount, account.Level, staticAccount.Country, footprint.Created);
            return response;
        }

        public async Task<OtherPlayerInfoResponse> GetOtherPlayerInfo(ulong oidAccount)
        {
            OtherPlayerInfoResponse response = new OtherPlayerInfoResponse();

            (Account account, StaticAccount staticAccount, List<EquipmentSlot> equpmentSlots, List<Talent> talents) = await TaskUtility.WaitAll4(
                _accountCache.Get(oidAccount),
                _staticAccountCache.Get(oidAccount),
                _equipmentSlotCache.GetAll(oidAccount),
                Internal.Services.TalentService.GetTalents(oidAccount));

            response.AccountLevel = account.Level;
            response.Nickname = staticAccount.Nickname;
            response.EquipmentSlots = equpmentSlots;
            response.UsingEquipments = await Internal.Services.InventoryService.GetEquipmentsInSlot(oidAccount, response.EquipmentSlots);
            response.UsingHero = await Internal.Services.InventoryService.GetUsingHero(oidAccount, account);
            response.Talents = talents;

            return response;
        }

        public async Task<SetNicknameResponse> SetNickname(ulong oidAccount, string nickname)
        {
            SetNicknameResponse response = new SetNicknameResponse();
            nickname = nickname.Trim();
            if (nickname.Length < _serviceLogicInfo.MinNicknameLength ||
                nickname.Length > _serviceLogicInfo.MaxNicknameLength)
            {
                response.SetResult(ErrorCode.InvalidParameter, $"Invalid nickname length! {nickname.Length}");
                return response;
            }

            StaticAccount staticAccount = await _staticAccountCache.Get(oidAccount);
            if (string.IsNullOrEmpty(staticAccount.Nickname) == false)
            {
                response.SetResult(ErrorCode.InvalidRequest, "Already nickname set!");
                return response;
            }

            // REF : https://www.regular-expressions.info/unicode.html#prop
            Regex p = new Regex("^[\\p{L}\\p{Z}\\p{P}\\p{M}\\p{N}\\p{S}]{2,16}$");
            System.Text.RegularExpressions.Match m = p.Match(nickname);
            if (m.Success == false)
            {
                response.SetResult(ErrorCode.InvalidRequest, "Regex match failed!");
                return response;
            }

            if (StaticInfo.Instance.BadWordInfo.Contains(nickname) == true)
            {
                response.SetResult(ErrorCode.NotUsableNickname, "Contains bad word!");
                return response;
            }

            StaticAccount newStaticAccount = new StaticAccount(staticAccount);
            newStaticAccount.Nickname = nickname;

            await _staticAccountCache.Change(staticAccount, newStaticAccount);
            response.Nickname = nickname;

            return response;
        }

        public async Task<UpgradeTalentResponse> UpgradeTalent(ulong oidAccount, bool useJewel = false)
        {
            UpgradeTalentResponse response = new UpgradeTalentResponse();

            (Account account, List<Talent> talents, TimeReward timeReward) = await TaskUtility.WaitAll3(
                _accountCache.Get(oidAccount),
                _talentCache.GetAll(oidAccount),
                _timeRewardCache.Get(oidAccount, TimeReward.MakeSubKeysWithName(TimeRewardType.Talent.ToString()))
            );

            if (account == null || talents == null)
            {
                response.SetResult(ErrorCode.InternalError, "Internal Error 1-1");
                return response;
            }

            int totalTalentLevel = talents.Sum(t => t.Level);

            TalentUpgradeInfo talentUpgradeInfo = StaticInfo.Instance.TalentUpgradeInfo[totalTalentLevel + 1];
            if (talentUpgradeInfo == null)
            {
                response.SetResult(ErrorCode.InternalError, "Internal Error 1-2");
                return response;
            }

            if (account.Level < talentUpgradeInfo.MinAccountLevel)
            {
                response.SetResult(ErrorCode.WrongRequest, $"Account level must not smaller than {talentUpgradeInfo.MinAccountLevel}");
                return response;
            }

            int needGold = talentUpgradeInfo.GoldCost;
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


            Talent beUpgradeTalent = Internal.Services.TalentService.SelectUpgradableTalent(talents);
            if (beUpgradeTalent == null)
            {
                response.SetResult(ErrorCode.WrongRequest, "No upgradable talent!");
                return response;
            }
            LogAndProgress logAndProgress = new LogAndProgress(account);
            response.ChangedColumns = new ChangedColumns();
            response.ChangedColumns.Add(AccountColumns.Gold, account.Gold - needGold);
            response.ChangedColumns.Add(AccountColumns.Jewel, account.Jewel - needJewel);

            Talent newTalent = new Talent(beUpgradeTalent);
            ++newTalent.Level;
            await _talentCache.Change(beUpgradeTalent, newTalent);
            await _accountCache.Change(account, response.ChangedColumns.ToDBChanged());
            if (newTalent.Level == 1 && newTalent.Id == (int)TalentType.AddTimeRewardGold && timeReward == null)
            {
                timeReward = new TimeReward()
                {
                    OidAccount = oidAccount,
                    RewardType = TimeRewardType.Talent.ToString(),
                    RewardTime = DateTime.UtcNow,
                };
                await _timeRewardCache.Set(timeReward);
                response.CreatedTalentTimeReward = timeReward;
            }
            Internal.Services.ProgressService.AccountChange(account, response.ChangedColumns, logAndProgress);
            Statistics.GameLogger.AccountChange(account, ChangeReason.UpgradeTalent, response.ChangedColumns, logAndProgress);
            await logAndProgress.Execute();

            response.Upgraded = newTalent;
            response.ProgressChanges = logAndProgress.GetProgressChanges();
            return response;
        }
        public async Task<CompletedMissionsResponse> GetCompletedMissions(ulong oidAccount)
        {
            CompletedMissionsResponse response = new CompletedMissionsResponse();
            response.CompletedMissions = await _completedMissionCache.GetAll(oidAccount) ?? new List<CompletedMission>();
            return response;
        }
        public async Task<MissionCompleteResponse> MissionComplete(ulong oidAccount, int missionId)
        {
            MissionCompleteResponse response = new MissionCompleteResponse();
            if (StaticInfo.Instance.MissionInfos.TryGetValue(missionId, out var info) == false)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                return response;
            }
            CompletedMission mission = await _completedMissionCache.Get(oidAccount, CompletedMission.MakeSubKeysWithName(missionId));
            if(mission != null)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                response.ErrorText = "AlreadyComplete Mission.";
                return response;
            }
            var transaction = DaoCache.Instance.Transaction();
            response.CompletedMission = new CompletedMission()
            {
                OidAccount = oidAccount,
                MissionId = missionId,
                IsRewarded = false,
            };
            if(info.Type == MissionType.LobbyTutorial && info is LobbyTutorialInfo tutorialInfo && tutorialInfo.TutorialType == LobbyTutorialType.BoxOpen)
            {
                var timeRewards = await _timeRewardCache.GetAll(oidAccount);
                var openBox = timeRewards.Find(e => e.RewardType == TimeRewardType.OpenBox.ToString());
                var openBoxAds = timeRewards.Find(e => e.RewardType == TimeRewardType.OpenBoxByAds.ToString());
                if (openBox == null)
                {
                    openBox = new TimeReward()
                    {
                        OidAccount = oidAccount,
                        Count = 0,
                        RewardTime = DateTime.UtcNow,
                        RewardType = TimeRewardType.OpenBox.ToString()
                    };
                    await _timeRewardCache.Set(openBox, transaction);
                    if (response.CreatedTimeRewards == null)
                    {
                        response.CreatedTimeRewards = new List<TimeReward>();
                    }
                    response.CreatedTimeRewards.Add(openBox);
                }
                if (openBoxAds == null)
                {
                    openBoxAds = new TimeReward()
                    {
                        OidAccount = oidAccount,
                        Count = 0,
                        RewardTime = DateTime.UtcNow,
                        RewardType = TimeRewardType.OpenBoxByAds.ToString()
                    };
                    await _timeRewardCache.Set(openBoxAds, transaction);
                    if (response.CreatedTimeRewards == null)
                    {
                        response.CreatedTimeRewards = new List<TimeReward>();
                    }
                    response.CreatedTimeRewards.Add(openBoxAds);
                }
            }
            await _completedMissionCache.Set(response.CompletedMission, transaction);
            if(await transaction.Execute() == false)
            {
                response.ErrorCode = ErrorCode.DbError;
                return response;
            }
            return response;
        }
    }
}