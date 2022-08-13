using Dash.Model;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticInfo;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Lobby
{
    public class ShopController : LobbyAPIController
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private BillingService _billingService;
        private ShopService _shopService;
        public ShopController(BillingService billingService, ShopService shopService)
        {
            _billingService = billingService;
            _shopService = shopService;
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseResponse>> Purchase(ulong oidAccount,
            [Required][FromForm]int productId, [FromForm]string receipt)
        {
            var info = StaticInfo.Instance.ProductInfo[productId];
            PurchaseResponse response;
            if (info.Payment == PaymentType.Cash)
            {
                response = await _billingService.PurchaseCash(oidAccount, productId, receipt);
            }
            else
            {
                response = await _billingService.Purchase(oidAccount, productId);
            }
            if (response.ErrorCode != ErrorCode.Success || response.AlreadyUsed == true)
            {
                return Ok(response);
            }
            var shopReceipt = await _billingService.GetShopReceipt(oidAccount);
            response = await _shopService.GiveShopProduct(oidAccount, shopReceipt, response);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseResponse>> CheckShopReceipt(ulong oidAccount)
        {
            PurchaseResponse response = new PurchaseResponse();
            var shopReceipt = await _billingService.GetShopReceipt(oidAccount);
            if (shopReceipt == null)
            {
                return Ok(response);
            }
            response = await _shopService.GiveShopProduct(oidAccount, shopReceipt, response);
            return Ok(response);
        }

        const string BoxSale = "boxsale";
        [HttpPost]
        public async Task<ActionResult<HttpResponseModel>> BoxSaleClose(ulong oidAccount)
        {
            await DaoCache.Instance.GetMemCache().Del($"{BoxSale}:{oidAccount}");
            return Ok(new HttpResponseModel(ErrorCode.Success));
        }

        [HttpPost]
        public async Task<ActionResult<ShopListResponse>> List(ulong oidAccount)
        {
            return Ok(await _shopService.ShopList(oidAccount));
        }

        //캐시 상품은 구매하기전에 구매제한 체크를 해야한다.
        [HttpPost]
        public async Task<ActionResult<CheckCashPurchaseResponse>> CheckCashPurchase(ulong oidAccount,
            [Required][FromForm]int productId)
        {
            var info = StaticInfo.Instance.ProductInfo[productId];
            CheckCashPurchaseResponse response = new CheckCashPurchaseResponse();
            if (info.Payment != PaymentType.Cash)
            {
                response.ErrorCode = ErrorCode.WrongRequest;
                return Ok(response);
            }
            if (await _billingService.CheckCashPurchase(oidAccount, productId) == false)
            {
                response.ErrorCode = ErrorCode.UnmatchedCondition;
            }
            response.ProductId = productId;
            return Ok(response);
        }

    }
}