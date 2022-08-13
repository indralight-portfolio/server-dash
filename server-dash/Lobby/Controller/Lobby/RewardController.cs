using Dash.Model.Service;
using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Lobby
{
    public class RewardController : LobbyAPIController
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private RewardService _service;

        public RewardController(RewardService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<TalentTimeRewardResponse>> ReceiveTalentTimeReward(ulong oidAccount)
        {
            ReceiveTalentTimeRewardModel model = await _service.ReceiveTalentTimeReward(oidAccount);
            return Ok(new TalentTimeRewardResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<OpenBoxWithKeyResponse>> OpenBoxWithKey(ulong oidAccount,
            [Required][FromForm]int boxId)
        {
            return Ok(await _service.OpenBoxWithKey(oidAccount, boxId));
        }

        [HttpPost]
        public async Task<ActionResult<OpenBoxWithTimeRewardResponse>> OpenBoxWithTimeReward(ulong oidAccount,
            [Required][FromForm]int productId)
        {
            return Ok(await _service.OpenBoxWithTimeReward(oidAccount, productId));
        }

        [HttpPost]
        public async Task<ActionResult<ReceiveChapterAchievementRewardResponse>> ReceiveChapterAchievementReward(ulong oidAccount,
            [Required][FromForm]ushort chapterId, [Required][FromForm]ushort targetStage)
        {
            ReceiveChapterAchievementRewardModel model = await _service.ReceiveChapterAchievementReward(oidAccount, chapterId, targetStage);
            return Ok(new ReceiveChapterAchievementRewardResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<AddStaminaResponse>> AddStaminaByTime(ulong oidAccount)
        {
            AddStaminaModel model = await Internal.Services.StaminaService.AddStaminaByTime(oidAccount);
            return Ok(new AddStaminaResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<AddStaminaResponse>> AddStaminaByJewel(ulong oidAccount)
        {
            AddStaminaModel model = await Internal.Services.StaminaService.AddStaminaByJewel(oidAccount);
            return Ok(new AddStaminaResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<AddStaminaResponse>> AddStaminaByAds(ulong oidAccount)
        {
            AddStaminaModel model = await Internal.Services.StaminaService.AddStaminaByAds(oidAccount);
            return Ok(new AddStaminaResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<OpenBoxResponse>> TestOpenBox(ulong oidAccount,
            [Required][FromForm]int boxId, [Required]int count)
        {
            OpenBoxModel model = await _service.TestOpenBox(oidAccount, boxId, count);
            return Ok(new OpenBoxResponse(model));
        }

        [HttpPost]
        public async Task<ActionResult<ReceiveMissionRewardResponse>> ReceiveMissionReward(ulong oidAccount,
            [Required][FromForm]int missionId)
        {
            return Ok(await _service.ReceiveMissionReward(oidAccount, missionId));
        }

        [HttpPost]
        public async Task<ActionResult<CouponUseResponse>> CouponUse(ulong oidAccount,
            [Required][FromForm] string code)
        {
            return Ok(await _service.ExchangeCoupon(oidAccount, code));
        }
    }
}
