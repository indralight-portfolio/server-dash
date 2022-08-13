using Dash.Model.Rdb;
using Dash.StaticData.Shop;
using Dash.Types;
using System;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    public class ConditionModel
    {
        public DateTime? BeforeCreate { get; set; }
        public DateTime? AfterCreate { get; set; }
        public bool Newbie { get; set; }
        public bool NotNewbie { get; set; }

        public bool Returnee { get; set; }
        public Range AccountLevel { get; set; }

        public ConditionModel CheckEmpty()
        {
            if (BeforeCreate == null && AfterCreate == null && Newbie == false && NotNewbie == false && Returnee == false)
                return null;
            else
                return this;
        }

        public bool Check(Account account, ServerTime serverTime)
        {
            if (AccountLevel.Min > 0 && account.Level < AccountLevel.Min)
                return false;
            else if (AccountLevel.Max > 0 && account.Level > AccountLevel.Max)
                return false;
            else if (BeforeCreate != null && account.Created > BeforeCreate)
                return false;
            else if (AfterCreate != null && account.Created < AfterCreate)
                return false;
            else if (Newbie == true && account.IsNewbie(serverTime) == false)
                return false;
            else if (NotNewbie == true && account.IsNewbie(serverTime) == true)
                return false;
            else if (Returnee == true && account.IsReturnee(serverTime) == false)
                return false;

            return true;
        }
    }

    public class MailConditionModel : ConditionModel
    {
        public MarketType MarketType { get; set; }

        public new MailConditionModel CheckEmpty()
        {
            if (MarketType == MarketType.Undefined)
                return base.CheckEmpty() as MailConditionModel;
            else
                return this;
        }

        public bool Check(Account account, AccountExtra accountExtra, ServerTime serverTime)
        {
            if (Check(account, serverTime) == false)
                return false;
            else if (MarketType != MarketType.Undefined && MarketType != accountExtra.MarketType)
                return false;

            return true;
        }
    }

    public class ProductConditionModel : ConditionModel
    {
        public int PreEpisodeId { get; }
        public int PreProductId { get; }

        public new ProductConditionModel CheckEmpty()
        {
            if (PreEpisodeId == 0 && PreProductId == 0)
                return base.CheckEmpty() as ProductConditionModel;
            else
                return this;
        }

        public bool Check(ProductCheckContext context, ProductInfo productInfo, ServerTime serverTime)
        {
            if (Check(context.Account, serverTime) == false)
                return false;
            else if (PreEpisodeId > 0)
            {
                var preEpisodeClear = context.EpisodeClears?.Find(e => e.EpisodeId == PreEpisodeId);
                if (preEpisodeClear == null)
                    return false;
            }
            else if (PreProductId > 0)
            {
                var prePurchaseHistory = context.ShopHistories?.Find(e => e.ProductId == PreProductId);
                if (prePurchaseHistory == null || prePurchaseHistory.UpdateTime < productInfo.Period.Start)
                    return false;
            }
            return true;
        }
    }

    public class EventConditionModel : ConditionModel
    {
        public int PreEpisodeId { get; }

        public new EventConditionModel CheckEmpty()
        {
            if (PreEpisodeId == 0)
                return base.CheckEmpty() as EventConditionModel;
            else
                return this;
        }

        public bool Check(Account account, List<EpisodeClear> episodeClears, ServerTime serverTime)
        {
            if (Check(account, serverTime) == false)
                return false;
            else if (PreEpisodeId > 0)
            {
                var preEpisodeClear = episodeClears?.Find(e => e.EpisodeId == PreEpisodeId);
                if (preEpisodeClear == null)
                    return false;
            }

            return true;
        }
    }
}