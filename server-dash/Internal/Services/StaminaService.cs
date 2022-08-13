using Common.Utility;
using Dash;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using Dash.StaticData;
using Dash.StaticInfo;
using Dash.Types;
using server_dash.Statistics;
using System;
using System.Threading.Tasks;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Internal.Services
{
    public static class StaminaService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private static ISingleDBCache<Account> _accountCache;
        private static ISingleDBCache<StaticAccount> _staticAccountCache;
        private static IMultipleDBCache<TimeReward> _timeRewardCache;

        public static void Init(DaoCache daoCache)
        {
            _accountCache = daoCache.GetSingle<Account>();
            _staticAccountCache = daoCache.GetSingle<StaticAccount>();
            _timeRewardCache = daoCache.GetMultiple<TimeReward>();
        }

        public static async Task<AddStaminaModel> AddStaminaByTime(ulong oidAccount)
        {
            var model = new AddStaminaModel();
            model.ErrorCode = ErrorCode.Success;

            (Account account, TimeReward timeReward) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _timeRewardCache.Get(oidAccount, TimeReward.MakeSubKeysWithName(TimeRewardType.RestoreStamina.ToString()))
            );
            if (account == null || timeReward == null)
            {
                _logger.Error($"[RestoreStamina]{oidAccount.LogOid()} db error.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }

            if (account.Stamina >= RewardService.MaxStamina)
            {
                TimeReward newTimeReward = new TimeReward(timeReward);
                newTimeReward.RewardTime = DateTime.UtcNow;
                await _timeRewardCache.Change(timeReward, newTimeReward);
            }

            DateTime now = DateTime.UtcNow;
            DateTime lastReceiveRewardTime = timeReward.RewardTime;
            // 5초정도 여유시간 줌.
            int elapsedSeconds = (int)(now - lastReceiveRewardTime).TotalSeconds + 5;
            if (elapsedSeconds <= RewardService.NeedSecondsForRestoreStamina)
            {
                _logger.Debug($"[RestoreStamina]{oidAccount.LogOid()} invalid elapsed time, {elapsedSeconds}");
                model.ErrorCode = ErrorCode.LackOfCost;
                return model;
            }

            int restoredStamina = elapsedSeconds /
                                  RewardService.NeedSecondsForRestoreStamina;
            lastReceiveRewardTime = lastReceiveRewardTime.AddSeconds(restoredStamina * RewardService.NeedSecondsForRestoreStamina);

            model.TimeReward = new TimeReward(timeReward);
            model.TimeReward.RewardTime = lastReceiveRewardTime;

            int newStamina = Math.Clamp(account.Stamina + restoredStamina, 0, RewardService.MaxStamina);
            restoredStamina = newStamina - account.Stamina;
            if (restoredStamina == 0)
            {
                await _timeRewardCache.Change(timeReward, model.TimeReward);
                return model;
            }

            model.ChangedColumns = new ChangedColumns();
            model.ChangedColumns.Add(AccountColumns.Stamina, newStamina);

            LogAndProgress logAndProgress = new LogAndProgress(account);
            GameLogger.AccountChange(account, ChangeReason.RestoreStamina, model.ChangedColumns, logAndProgress);
            ProgressService.AccountChange(account, model.ChangedColumns, logAndProgress);

            var trans = DaoCache.Instance.Transaction();
            await TaskUtility.WaitAll2(
                _accountCache.Change(account, model.ChangedColumns.ToDBChanged(), trans),
                _timeRewardCache.Change(timeReward, model.TimeReward, trans)
                );
            await trans.Execute();

            _ = logAndProgress.Execute();

            return model;
        }

        public static async Task<AddStaminaModel> AddStaminaByJewel(ulong oidAccount)
        {
            var model = new AddStaminaModel();
            model.ErrorCode = ErrorCode.Success;

            Account account = await _accountCache.Get(oidAccount);
            if (account == null)
            {
                _logger.Error($"[AddStaminaByJewel]{oidAccount.LogOid()} db error.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }

            int cost = RewardService.JewelForStamina;
            if (account.Jewel < cost)
            {
                model.ErrorCode = ErrorCode.NotEnoughJewel;
                return model;
            }

            int newJewel = account.Jewel - cost;
            int newStamina = account.Stamina + RewardService.StaminaByJewel;
            model.ChangedColumns = new ChangedColumns();
            model.ChangedColumns.Add(AccountColumns.Jewel, newJewel);
            model.ChangedColumns.Add(AccountColumns.Stamina, newStamina);

            LogAndProgress logAndProgress = new LogAndProgress(account);
            GameLogger.AccountChange(account, ChangeReason.RewardByJewel, model.ChangedColumns, logAndProgress);
            ProgressService.AccountChange(account, model.ChangedColumns, logAndProgress);

            await _accountCache.Change(account, model.ChangedColumns.ToDBChanged());

            _ = logAndProgress.Execute();

            return model;
        }

        public static async Task<AddStaminaModel> AddStaminaByAds(ulong oidAccount)
        {
            var model = new AddStaminaModel();
            model.ErrorCode = ErrorCode.Success;

            (Account account, StaticAccount staticAccount, TimeReward timeReward) = await TaskUtility.WaitAll3(
                _accountCache.Get(oidAccount),
                _staticAccountCache.Get(oidAccount),
                _timeRewardCache.Get(oidAccount, TimeReward.MakeSubKeysWithName(TimeRewardType.AddStaminaByAds.ToString()))
            );
            if (account == null || staticAccount == null)
            {
                _logger.Error($"[AddStaminaByAds]{oidAccount.LogOid()} db error.");
                model.ErrorCode = ErrorCode.DbError;
                return model;
            }

            ServiceLogicInfo serviceLogicInfo = StaticInfo.Instance.ServiceLogicInfo.Get();
            var localTimeComparer = new LocalTimeComparer(staticAccount.TimeOffset, serviceLogicInfo.DailyResetHour);

            if (timeReward == null)
            {
                model.TimeReward = new TimeReward { OidAccount = oidAccount, RewardType = TimeRewardType.AddStaminaByAds.ToString(), RewardTime = DateTime.UtcNow };
            }
            else
            {
                if (localTimeComparer.IsAfterResetTime(timeReward.RewardTime) == false && timeReward.Count >= 5)
                {
                    model.ErrorCode = ErrorCode.OverLimit;
                    return model;
                }

                model.TimeReward = new TimeReward(timeReward);
                model.TimeReward.RewardTime = DateTime.UtcNow;
                if (localTimeComparer.IsAfterResetTime(timeReward.RewardTime) == true)
                    model.TimeReward.Count = 0;
            }
            model.TimeReward.Count++;

            int newStamina = account.Stamina + RewardService.StaminaByAds;
            model.ChangedColumns = new ChangedColumns();
            model.ChangedColumns.Add(AccountColumns.Stamina, newStamina);

            LogAndProgress logAndProgress = new LogAndProgress(account);
            GameLogger.AccountChange(account, ChangeReason.RewardByAds, model.ChangedColumns, logAndProgress);
            ProgressService.AccountChange(account, model.ChangedColumns, logAndProgress);

            var trans = DaoCache.Instance.Transaction();
            await TaskUtility.WaitAll2(
                _accountCache.Change(account, model.ChangedColumns.ToDBChanged(), trans),
                timeReward == null ? _timeRewardCache.Set(model.TimeReward, trans) : _timeRewardCache.Change(timeReward, model.TimeReward, trans)
                );
            await trans.Execute();

            _ = logAndProgress.Execute();

            return model;
        }

        public static async Task ConsumeStamina(ulong oidAccount, int amount, LogAndProgress logAndProgress, TransactionTask trans, Account account = null)
        {
            TimeReward timeReward = null;
            if (account == null)
            {
                (account, timeReward) = await TaskUtility.WaitAll2(
                    _accountCache.Get(oidAccount),
                    _timeRewardCache.Get(oidAccount,
                        TimeReward.MakeSubKeysWithName(TimeRewardType.RestoreStamina.ToString())));
            }
            else
            {
                timeReward = await _timeRewardCache.Get(oidAccount,
                    TimeReward.MakeSubKeysWithName(TimeRewardType.RestoreStamina.ToString()));
            }
            // 없어도 될 듯하다.
            //if (account.Stamina >= RewardService.MaxStamina)
            //{
            //    TimeReward newTimeReward = new TimeReward(timeReward);
            //    newTimeReward.RewardTime = DateTime.UtcNow;
            //    await _timeRewardCache.Change(timeReward, newTimeReward);
            //}

            if (account.Stamina < amount)
            {
                throw new Exception($"{oidAccount.LogOid()} Insufficient Stamina!");
            }

            int newStamina = account.Stamina - amount;

            var changedColumns = new ChangedColumns();
            changedColumns.Add(AccountColumns.Stamina, newStamina);
            GameLogger.AccountChange(account, ChangeReason.GamePlayCost, changedColumns, logAndProgress);
            ProgressService.AccountChange(account, changedColumns, logAndProgress);

            await _accountCache.Change(account, changedColumns.ToDBChanged(), trans);
        }
    }
}