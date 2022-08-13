using Dash;
using Dash.Model.Rdb;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticData;
using Dash.StaticInfo;
using Dash.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server_dash.Lobby.Services
{
    public class ShopService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private DaoCache _daoCache;
        private ISingleDBCache<Account> _accountCache;
        private IMultipleDBCache<Equipment> _equipmentCache;
        private IMultipleDBCache<Hero> _heroCache;
        private IMultipleDBCache<ChapterAchievement> _chapterAcievementCache;
        private IMultipleDBCache<ShopReceipt> _shopReceiptCache;
        private IMultipleDBCache<ShopHistory> _shopHistoryCache;
        private IMultipleDBCache<Consume> _consumeCache;

        private RewardService _rewardService;

        public ShopService(DaoCache daoCache, RewardService rewardService)
        {
            _daoCache = daoCache;
            _accountCache = daoCache.GetSingle<Account>();
            _equipmentCache = daoCache.GetMultiple<Equipment>();
            _heroCache = daoCache.GetMultiple<Hero>();
            _chapterAcievementCache = daoCache.GetMultiple<ChapterAchievement>();
            _shopReceiptCache = daoCache.GetMultiple<ShopReceipt>();
            _shopHistoryCache = daoCache.GetMultiple<ShopHistory>();
            _consumeCache = daoCache.GetMultiple<Consume>();
            _rewardService = rewardService;
        }
        public async Task<PurchaseResponse> GiveShopProduct(ulong oidAccount, ShopReceipt shopReceipt, PurchaseResponse response)
        {
            Account account = await _accountCache.Get(oidAccount);
            if (shopReceipt == null)
            {
                _logger.Error($"[ShopService][GiveShopProduct] shopReceipt is null. {oidAccount.LogOid()}");
                response.SetResult(ErrorCode.GiveShopProductFailed, "shopReceipt is null");
                return response;
            }
            var info = StaticInfo.Instance.ProductInfo[shopReceipt.ProductId];
            LogAndProgress logAndProgress = new LogAndProgress(account);
            var trans = DaoCache.Instance.Transaction();
            RewardInfo excessReward = null;
            if (info.ShopCategory == ShopCategoryType.Box && info.BoxId != 0)
            {
                response.OpenBoxModel = await _rewardService.OpenBox(account, info.BoxId, info.BoxCount, logAndProgress, trans);
            }
            else if(info.ShopCategory != ShopCategoryType.Box)
            {
                List<Dash.StaticData.RewardInfo> rewardInfos = new List<Dash.StaticData.RewardInfo>();
                if (info.BoxId != 0)
                {
                    rewardInfos.Add(await _rewardService.OpenBox(account, info.BoxId, info.BoxCount));
                }
                if(info.ConsumeBoxId != 0)
                {
                    rewardInfos.Add(await _rewardService.OpenBox(account, info.ConsumeBoxId, info.ConsumeBoxCount));
                }
                if (info.RewardInfo != null)
                {
                    rewardInfos.Add(info.RewardInfo);
                }
                var combineRewardInfo = Dash.StaticData.RewardInfoHelper.CombineRewardInfo(rewardInfos);
                (response.RewardModel, excessReward) = await _rewardService.GiveReward(account, combineRewardInfo, ChangeReason.ShopProduct, logAndProgress, trans);
            }
            
            _shopReceiptCache.Del(shopReceipt, trans);
            if (await trans.Execute())
            {
                response.SetResult(ErrorCode.Success);
                await logAndProgress.Execute();
                response.IsSendMail = excessReward != null && excessReward.IsEmpty() == false;
                if (response.IsSendMail)
                {
                    //우편으로 보내줌
                    await _rewardService.SendMailExcessReward(account.OidAccount, excessReward);
                }
            }
            else
            {
                response.OpenBoxModel = null;
                response.RewardModel = null;
                response.ErrorCode = ErrorCode.GiveShopProductFailed;
            }
            return response;
        }
        public async Task<ShopListResponse> ShopList(ulong oidAccount)
        {
            ShopListResponse response = new ShopListResponse();
            (Account account, List<ChapterAchievement> chapterAchievements, List<ShopHistory> shopHistories) = await Common.Utility.TaskUtility.WaitAll3(
            _accountCache.Get(oidAccount),
            _chapterAcievementCache.GetAll(oidAccount),
            _shopHistoryCache.GetAll(oidAccount));
            
            (List<int> productIds, DateTime nextRefreshTime) = Dash.ShopHelper.GetList(account, chapterAchievements.GetLastChapter(), shopHistories);
            response.ProductIds = productIds;
            response.NextRefreshTime = nextRefreshTime;
            return response;
        }
    }
}
