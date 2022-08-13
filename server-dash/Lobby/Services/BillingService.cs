using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Common.NetCore.Billing;
using Common.Utility;
using Dash;
using Dash.Model.Rdb;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using Dash.StaticData.Shop;
using Dash.StaticInfo;
using Dash.Types;
using Newtonsoft.Json.Linq;
using server_dash.Internal.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static Dash.StaticData.Shop.ProductInfo;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Lobby.Services
{
    public class BillingService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private const string KEY_STORE = "Store";
        private const string KEY_TRANSACTION_ID = "TransactionID";
        private const string KEY_PAYLOAD = "Payload";

        private const string KEY_PRODUCT_ID = "ProductID";
        private const string KEY_OIDACCOUNT = "OidAccount";
        private const string KEY_DATETIME = "DateTime";
        private const string KEY_PAYLOAD_JSON = "PayloadJson";
        private const string KEY_ALREADY_USED = "AlreadyUsed";

        private const string ReceiptTableName = "Receipt";

        private LobbyServerConfig _lobbyServerConfig;
        private IAmazonDynamoDB _dynamoDBClient;
        private HttpClient httpClient;
        private DaoCache _daoCache;
        private IMemCache _memCache;
        private ISingleDBCache<Account> _accountCache;
        private IMultipleDBCache<ChapterAchievement> _chapterAcievementCache;
        private IMultipleDBCache<ShopReceipt> _shopReceiptCache;
        private IMultipleDBCache<ShopHistory> _shopHistoryCache;
        private string ddbTableSuffix;

        private const string googlePlayPackageName = "";
        private const string googlePlayPublicKey = "";
        private GoogleSignatureVerifier googleSignatureVerifier;

        private const string appStoreBundleId = "";
        private string appStoreVerifyReceiptUrl;
        private string appStorePassword;

        public BillingService(LobbyServerConfig lobbyServerConfig, IAmazonDynamoDB dynamoDBClient, DaoCache daoCache)
        {
            _lobbyServerConfig = lobbyServerConfig;
            _dynamoDBClient = dynamoDBClient;
            httpClient = new HttpClient();

            ddbTableSuffix = lobbyServerConfig.Billing.DDBTableSuffix;

            googleSignatureVerifier = new GoogleSignatureVerifier(googlePlayPublicKey);

            appStoreVerifyReceiptUrl = lobbyServerConfig.Billing.AppStoreVerifyReceiptUrl;
            appStorePassword = null;
            _daoCache = daoCache;
            _accountCache = daoCache.GetSingle<Account>();
            _chapterAcievementCache = daoCache.GetMultiple<ChapterAchievement>();
            _shopReceiptCache = daoCache.GetMultiple<ShopReceipt>();
            _shopHistoryCache = daoCache.GetMultiple<ShopHistory>();
            _memCache = daoCache.GetMemCache();
        }
        public async Task<PurchaseResponse> Purchase(ulong oidAccount, int productId)
        {
            PurchaseResponse response = new PurchaseResponse();
            response.AlreadyUsed = false;
            if (StaticInfo.Instance.ProductInfo.TryGet(productId, out ProductInfo productInfo) == false)
            {
                response.SetResult(ErrorCode.WrongRequest, $"Not Exist ProductId : {productId}");
                return response;
            }
            if (productInfo.Payment != PaymentType.Jewel && productInfo.Payment != PaymentType.Gold)
            {
                response.SetResult(ErrorCode.WrongRequest, $"Invalid Payment. Id : {productId}, Payment: {productInfo.Payment}");
                return response;
            }
            if (productInfo.Price.GoldPrice <= 0 && productInfo.Price.JewelPrice <= 0)
            {
                response.SetResult(ErrorCode.InternalError, $"Invalid Price. Id : {productId}, GoldPrice : {productInfo.Price.GoldPrice}, JewelPrice : {productInfo.Price.JewelPrice}");
                return response;
            }
            string boxProductIdString = null;
            if (productInfo.ShopCategory == ShopCategoryType.Box)
            {
                boxProductIdString = await _memCache.StringGet($"boxsale:{oidAccount}");
            }
            (Account account, List<ShopHistory> shopHistories, List<ChapterAchievement> chapterAchievements) = await TaskUtility.WaitAll3(
                _accountCache.Get(oidAccount), _shopHistoryCache.GetAll(oidAccount), _chapterAcievementCache.GetAll(oidAccount));
            ProductInfo.PriceInfo priceInfo = null;
            if(string.IsNullOrEmpty(boxProductIdString) == false && int.TryParse(boxProductIdString, out int boxSaleProductId) && productInfo.Id == boxSaleProductId)
            {
                priceInfo = productInfo.SalePrice;
            }
            if(priceInfo == null)
            {
                priceInfo = productInfo.Price;
            }
            if (account.Gold < priceInfo.GoldPrice)
            {
                response.SetResult(ErrorCode.NotEnoughGold);
                return response;
            }
            if(account.Jewel < priceInfo.JewelPrice)
            {
                response.SetResult(ErrorCode.NotEnoughJewel);
                return response;
            }
            if (ShopHelper.CheckCondition(account, chapterAchievements.GetLastChapter(), shopHistories, productInfo, DateTime.UtcNow) == false)
            {
                response.SetResult(ErrorCode.WrongRequest);
                return response;
            }
            ShopHistory shopHistory = shopHistories?.Find(e => e.ProductId == productInfo.Id) ?? null;
            TransactionTask transaction = _daoCache.Transaction();
            ChangedColumns changedColumns = new ChangedColumns();
            LogAndProgress logAndProgress = new LogAndProgress(account);

            switch (productInfo.Payment)
            {
                case PaymentType.Gold:
                    changedColumns.Add(AccountColumns.Gold, account.Gold - priceInfo.GoldPrice);
                    break;
                case PaymentType.Jewel:
                    changedColumns.Add(AccountColumns.Jewel, account.Jewel - priceInfo.JewelPrice);
                    break;
            }
            ShopReceipt newShopReceipt = new ShopReceipt
            {
                OidAccount = oidAccount,
                Id = 0,
                ProductId = productId,
            };
            
            _shopReceiptCache.Set(newShopReceipt, transaction);
            if(shopHistory == null)
            {
                shopHistory = new ShopHistory()
                {
                    OidAccount = oidAccount,
                    ProductId = productId,
                    Count = 1,
                    UpdateTime = DateTime.UtcNow,
                };
                _shopHistoryCache.Set(shopHistory, transaction);
            }
            else
            {
                ShopHistory changedHistory = new ShopHistory(shopHistory);
                if(Dash.ShopHelper.CanResetCount(productInfo, changedHistory.UpdateTime, DateTime.UtcNow))
                {
                    changedHistory.Count = 1;
                }
                else
                {
                    changedHistory.Count++;
                }
                changedHistory.UpdateTime = DateTime.UtcNow;
                _shopHistoryCache.Change(shopHistory, changedHistory, transaction);
            }
            if (changedColumns.Count != 0)
            {
                await _accountCache.Change(account, changedColumns.ToDBChanged(), transaction);
                Statistics.GameLogger.AccountChange(account, ChangeReason.ProductBuy, changedColumns, logAndProgress);
                ProgressService.AccountChange(account, changedColumns, logAndProgress);
                response.NonCashPayed = changedColumns;
            }
            Statistics.GameLogger.OnShop_Buy(oidAccount, account.Level, productId, productInfo.Payment, GetPrice(productInfo.Payment, priceInfo), logAndProgress);
            if (await transaction.Execute() == false)
            {
                response.SetResult(ErrorCode.DbError);
                return response;
            }
            await logAndProgress.Execute();
            if (productInfo.ShopCategory == ShopCategoryType.Box)
            {
                int boxSaleExpireSecond = StaticInfo.Instance.ServiceLogicInfo.Get().BoxSaleExpireSecond;
                await _memCache.StringSet($"boxsale:{oidAccount}", productId.ToString(), boxSaleExpireSecond + 5);//타이밍 이슈를 막기위해 5초정도의 여유를 둔다.
                response.BoxSaleExpireTime = DateTime.UtcNow.AddSeconds(boxSaleExpireSecond);
            }
            response.ProductId = productId;
            return response;
            
        }

        public async Task<PurchaseResponse> PurchaseCash(ulong oidAccount, int productId, string receiptString)
        {
            JObject receipt = JObject.Parse(receiptString);
            if (checkWellFormedReceipt(receipt) == false)
            {
                _logger.Error("receipt is not well formed.");
                return new PurchaseResponse { ErrorCode = ErrorCode.InternalError, ErrorText = "receipt is not well formed." };
            }
            receipt[KEY_OIDACCOUNT] = oidAccount;

            string store = receipt[KEY_STORE].Value<string>();
            if (store.Equals(StoreType.Fake.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                // Fake 영수증 처리
                receipt[KEY_PRODUCT_ID] = productId;
                receipt[KEY_DATETIME] = DateTime.UtcNow.ToString_ISO();
            }
            else if (store.Equals(StoreType.GooglePlay.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                // 구글 영수증 처리
                if (processRceiptGooglePlay(receipt) == false)
                {
                    _logger.Error("verifyReeiptGooglePlay failed.");
                    return new PurchaseResponse { ErrorCode = ErrorCode.InternalError, ErrorText = "verifyReeiptGooglePlay failed." };
                }
            }
            else if (store.Equals(StoreType.AppleAppStore.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                // 애플 영수증 처리
                if (await processRceiptAppStore(receipt) == false)
                {
                    _logger.Error("verifyReeiptAppStore failed.");
                    return new PurchaseResponse { ErrorCode = ErrorCode.InternalError, ErrorText = "verifyReeiptAppStore failed." };
                }
            }
            else
            {
                _logger.Error("invalid store.");
                return new PurchaseResponse { ErrorCode = ErrorCode.InternalError, ErrorText = "invalid store." };
            }

            // 사용한 영수증인지 체크: dynamoDB 에 존재하는지?
            string transactionId = receipt[KEY_TRANSACTION_ID].Value<string>();
            if (await existsDDBAsync(transactionId) == true)
            {
                // 존재하다면 영수증에 KEY_ALREADY_USED = true 추가
                receipt[KEY_ALREADY_USED] = true;
            }
            else
            {
                // 영수증을 저장
                await saveDDBAsync(receipt);                
            }
            bool alreadyUsed = receipt.ContainsKey(KEY_ALREADY_USED);
            if(alreadyUsed == false)
            {
                ProductInfo productInfo = StaticInfo.Instance.ProductInfo[productId];
                ShopReceipt newShopReceipt = new ShopReceipt
                {
                    OidAccount = oidAccount,
                    Id = 0,
                    ProductId = productId,
                };
                (Account account, bool isReceipt, ShopHistory shopHistory) = await TaskUtility.WaitAll3(
                    _accountCache.Get(oidAccount),
                    _shopReceiptCache.Set(newShopReceipt),
                    _shopHistoryCache.Get(oidAccount, ShopHistory.MakeSubKeysWithName(productId)));
                if (shopHistory == null)
                {
                    shopHistory = new ShopHistory()
                    {
                        OidAccount = oidAccount,
                        ProductId = productId,
                        Count = 1,
                        UpdateTime = DateTime.UtcNow,
                    };
                    await _shopHistoryCache.Set(shopHistory);
                }
                else
                {
                    ShopHistory changedHistory = new ShopHistory(shopHistory);
                    if (Dash.ShopHelper.CanResetCount(productInfo, changedHistory.UpdateTime, DateTime.UtcNow))
                    {
                        changedHistory.Count = 1;
                    }
                    else
                    {
                        changedHistory.Count++;
                    }
                    changedHistory.UpdateTime = DateTime.UtcNow;
                    await _shopHistoryCache.Change(shopHistory, changedHistory);
                }
                LogAndProgress logAndProgress = new LogAndProgress(account);
                PriceInfo priceInfo = productInfo.Price;
                if (productInfo.Price.CashPrice > (productInfo.SalePrice?.CashPrice ?? int.MaxValue))
                {
                    priceInfo = productInfo.SalePrice;
                }
                Statistics.GameLogger.OnShop_Buy(oidAccount, account.Level, productId, productInfo.Payment, GetPrice(productInfo.Payment, priceInfo), logAndProgress);
            }

            int porductId = receipt[KEY_PRODUCT_ID].Value<int>();

            return new PurchaseResponse { ProductId = productId, AlreadyUsed = alreadyUsed };
        }

        public async Task<ShopReceipt> GetShopReceipt(ulong oidAccount)
        {
            List< ShopReceipt> shopReceipts = await _shopReceiptCache.GetAll(oidAccount.ToString());
            if(shopReceipts.Count == 0)
            {
                return null;
            }
            return shopReceipts[shopReceipts.Count - 1];
        }

        public async Task<bool> CheckCashPurchase(ulong oidAccount, int productId)
        {
            if(StaticInfo.Instance.ProductInfo.TryGet(productId, out ProductInfo productInfo) == false)
            {
                return false;
            }
            (Account account, List<ShopHistory> shopHistories, List<ChapterAchievement> chapterAchievements) = await TaskUtility.WaitAll3(
                _accountCache.Get(oidAccount), _shopHistoryCache.GetAll(oidAccount), _chapterAcievementCache.GetAll(oidAccount));

            return ShopHelper.CheckCondition(account, chapterAchievements.GetLastChapter(), shopHistories, productInfo, DateTime.UtcNow);
        }

        private bool checkWellFormedReceipt(JObject receipt)
        {

            return (receipt.ContainsKey(KEY_STORE) && receipt.ContainsKey(KEY_TRANSACTION_ID) && receipt.ContainsKey(KEY_PAYLOAD));
        }

        private bool processRceiptGooglePlay(JObject receipt)
        {
            string payloadStr = receipt[KEY_PAYLOAD].Value<string>();
            JObject payload = JObject.Parse(payloadStr);
            //_logger.Info($"payload : {payload}");

            if ((payload.ContainsKey("json") && payload.ContainsKey("signature")) == false)
            {
                _logger.Error("receipt for GooglePlay is not well formed.");
                return false;
            }

            string json = payload["json"].Value<string>();
            string signature = payload["signature"].Value<string>();
            try
            {
                if (googleSignatureVerifier.Verify(json, signature) == false)
                {
                    _logger.Error("receipt for GooglePlay verify failed.");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("receipt for GooglePlay verify failed. " + e);
                return false;
            }

            JObject payloadJson = JObject.Parse(json);
            string packageName = payloadJson["packageName"].Value<string>();
            if (packageName.Equals(googlePlayPackageName) == false)
            {
                _logger.Fatal($"packageName mismatched. {packageName}");
                return false;
            }
            string productId = payloadJson["productId"].Value<string>();
            string orderId = payloadJson["orderId"].Value<string>();
            DateTime purchaseTime = DateTimeExtension.FromUnixTimestamp(payloadJson["purchaseTime"].Value<long>());

            receipt[KEY_TRANSACTION_ID] = orderId;
            receipt[KEY_PRODUCT_ID] = productId;
            receipt[KEY_DATETIME] = purchaseTime.ToString_ISO();
            receipt[KEY_PAYLOAD_JSON] = payloadJson;

            return true;
        }

        private async Task<bool> processRceiptAppStore(JObject receipt)
        {
            string payloadStr = receipt[KEY_PAYLOAD].Value<string>();

            var data = new JObject(new JProperty("receipt-data", payloadStr));
            if (string.IsNullOrEmpty(appStorePassword) == false)
            {
                data.Add("password", appStorePassword);
            }
            StringContent content = new StringContent(data.ToString());
            var httpResponse = await httpClient.PostAsync(appStoreVerifyReceiptUrl, content);
            if (httpResponse.IsSuccessStatusCode == false)
            {
                _logger.Error("receipt for AppleAppStore verify failed. HttpStatusCode:" + httpResponse.StatusCode);
                return false;
            }
            var rawResponse = await httpResponse.Content.ReadAsStringAsync();
            JObject payloadJson = JObject.Parse(rawResponse);
            //_logger.Info($"payloadJson : {payloadJson}");

            int status = payloadJson["status"].Value<int>();
            if (status != 0)
            {
                _logger.Error("receipt for AppStore verify failed. status:" + status);
                return false;
            }

            JObject appleReceipt = (JObject)payloadJson["receipt"];
            string bundleId = appleReceipt["bundle_id"].Value<string>();
            if (bundleId.Equals(appStoreBundleId) == false)
            {
                _logger.Error($"bundleId mismatched. {bundleId}");
                return false;
            }

            var product = ((JArray)appleReceipt["in_app"])[0];
            string productId = product["product_id"].Value<string>();
            string orderId = product["transaction_id"].Value<string>();
            DateTime purchaseTime = DateTimeExtension.FromUnixTimestamp(long.Parse(product["purchase_date_ms"].Value<string>()));

            receipt[KEY_TRANSACTION_ID] = orderId;
            receipt[KEY_PRODUCT_ID] = productId;
            receipt[KEY_DATETIME] = purchaseTime.ToString_ISO();
            receipt[KEY_PAYLOAD_JSON] = payloadJson;

            return true;
        }

        private async Task<bool> existsDDBAsync(string transactionId)
        {
            var table = Table.LoadTable(_dynamoDBClient, $"{ReceiptTableName}_{ddbTableSuffix}");
            var item = await table.GetItemAsync(transactionId);
            return item != null;
        }

        private async Task saveDDBAsync(JObject receipt)
        {
            var table = Table.LoadTable(_dynamoDBClient, $"{ReceiptTableName}_{ddbTableSuffix}");
            var item = Document.FromJson(receipt.ToString());
            await table.PutItemAsync(item);
        }

        private int getMyMoney(Account account, PaymentType paymentType)
        {
            switch (paymentType)
            {
                case PaymentType.Gold:
                    return account.Gold;
                case PaymentType.Jewel:
                    return account.Jewel;
            }
            return 0;
        }
        private int GetPrice(PaymentType payment, PriceInfo priceInfo)
        {
            switch (payment)
            {
                case PaymentType.Cash:
                    return priceInfo.CashPrice;
                case PaymentType.Gold:
                    return priceInfo.GoldPrice;
                case PaymentType.Jewel:
                    return priceInfo.JewelPrice;
                default:
                    throw new System.Exception("Invalid request. method attribute!");
            }
        }
    }
}