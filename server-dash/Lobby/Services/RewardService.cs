using Common.Utility;
using Dash.Model.Rdb;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticData;
using Dash.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server_dash.Lobby.Services
{
    public class RewardService : Internal.Services.RewardService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private ISingleDBCache<Footprint> _footprintCache;
        private ISingleDBCache<Coupon> _couponCache;
        private IMultipleDBCache<CouponUse> _couponUseCache;

        public RewardService(DaoCache daoCache, Internal.Services.MailService mailService) : base(daoCache, mailService)
        {
            _footprintCache = daoCache.GetSingle<Footprint>();
            _couponCache = daoCache.GetSingle<Coupon>();
            _couponUseCache = daoCache.GetMultiple<CouponUse>();
        }

        public async Task<CouponUseResponse> ExchangeCoupon(ulong oidAccount, string code)
        {
            CouponUseResponse response = new CouponUseResponse();

            (Account account, Footprint footprint) = await TaskUtility.WaitAll2(
                    _accountCache.Get(oidAccount),
                    _footprintCache.Get(oidAccount));

            if (account == null || footprint == null)
            {
                response.SetResult(ErrorCode.NoAccount);
                return response;
            }

            string query = $"SELECT * FROM {nameof(Coupon)} WHERE {nameof(Coupon.Code)} = @{nameof(Coupon.Code)} AND {nameof(Coupon.Start)} <= @{nameof(Coupon.Start)}";
            var param = new List<KeyValuePair<string, object>>{
                new KeyValuePair<string, object>($"@{nameof(Coupon.Code)}", code),
                new KeyValuePair<string, object>($"@{nameof(Coupon.Start)}", DateTime.UtcNow),
            };
            Coupon coupon = await _couponCache.GetQueryNoCache(query, param);
            if (coupon == null)
            {
                response.SetResult(ErrorCode.NoData);
                return response;
            }
            if (coupon.End < DateTime.UtcNow)
            {
                response.SetResult(ErrorCode.Expired);
                return response;
            }
            if (coupon.DataObject.TryGetReward(out RewardInfo rewardInfo) == false || rewardInfo == null)
            {
                response.SetResult(ErrorCode.NoData);
                return response;
            }
            if (coupon.ConditionObject?.Check(footprint) == false)
            {
                // 조건에 맞지 않으면 만료되었다고 표현하자.
                response.SetResult(ErrorCode.Expired);
                return response;
            }

            query = $"SELECT COUNT(*) FROM {nameof(CouponUse)} WHERE {nameof(CouponUse.CouponId)} = @{nameof(CouponUse.CouponId)}";
            param = new List<KeyValuePair<string, object>>{
                new KeyValuePair<string, object>($"@{nameof(CouponUse.CouponId)}", coupon.Id),
            };
            int usedCount = await _couponUseCache.CountQueryNocache(query, param);
            if (coupon.Count >= 0 && coupon.Count <= usedCount)
            {
                response.SetResult(ErrorCode.NotEnough);
                return response;
            }
            CouponUse couponUse = await _couponUseCache.Get(oidAccount, CouponUse.MakeSubKeysWithName(coupon.Id));
            if (couponUse != null)
            {
                response.SetResult(ErrorCode.AlreadyProcessed);
                return response;
            }

            response.RewardInfo = rewardInfo;

            LogAndProgress logAndProgress = new LogAndProgress(account);
            var trans = DaoCache.Instance.Transaction();
            RewardInfo excessReward;
            (response.RewardModel, excessReward) = await GiveReward(account, rewardInfo, ChangeReason.Coupon, logAndProgress, trans);

            couponUse = new CouponUse
            {
                OidAccount = oidAccount,
                CouponId = coupon.Id,
                UsedTime = DateTime.UtcNow,
            };
            await _couponUseCache.Set(couponUse, trans);

            if (await trans.Execute())
            {
                await logAndProgress.Execute();
                response.IsSendMail = excessReward != null && excessReward.IsEmpty() == false;
                if (response.IsSendMail)
                {
                    //우편으로 보내줌
                    await SendMailExcessReward(account.OidAccount, excessReward);
                }
            }
            else
            {
                response.RewardInfo = null;
                response.RewardModel = null;
                response.ErrorCode = ErrorCode.GiveRewardFailed;
            }
            return response;
        }
    }
}
