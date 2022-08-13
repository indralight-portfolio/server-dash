using Dash.Model.Rdb;
using Dash.StaticData.Shop;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    public class ProductCheckContext
    {
        public Account Account;
        public List<EpisodeClear> EpisodeClears;
        public List<ShopHistory> ShopHistories;

        public ProductCheckContext(Account account,
                List<EpisodeClear> episodeClears, List<ShopHistory> shopHistories)
        {
            Account = account;
            EpisodeClears = episodeClears;
            ShopHistories = shopHistories;
        }

        public static ProductCheckContext Get(Account account,
                List<EpisodeClear> episodeClears, List<ShopHistory> shopHistories) => new ProductCheckContext(account, episodeClears, shopHistories);
    }
}