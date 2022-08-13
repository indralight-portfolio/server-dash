using Common.Model;
using Common.NetCore.Social;
using Dash.Model;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.StaticInfo;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Lobby
{
    public class AccountController : LobbyAPIController
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private AccountService _service;
        public AccountController(AccountService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route(DefaultAreaRoute)]
        [Consumes("application/json")]
        public async Task<ActionResult<HttpResponseModel>> VerifySocial([FromBody] VerifySocialRequest request)
        {
            var response = new HttpResponseModel();
            AuthReq authReq = request.AuthReq;
            if (authReq == null)
            {
                _logger.Error($"authReq is null");
                response.SetResult(ErrorCode.InvalidParameter, "authReq is null");
                return Ok(response);
            }

            if (authReq.AuthType == AuthType.PlayGames)
            {
                string authCode = request.authCode;
                string idToken = request.idToken;
                if (authCode == null || idToken == null)
                {
                    _logger.Error($"authCode or idToken is null");
                    response.SetResult(ErrorCode.InvalidParameter, "authCode or idToken is null");
                    return Ok(response);
                }

                if (await PlayGamesVerifier.Verify(idToken) == false)
                {
                    _logger.Error($"VerifyPlayGames: false, [authReq: {authReq}]");
                    _logger.Error($"idToken: {idToken}");
                    response.SetResult(ErrorCode.InternalError);
                    return Ok(response);
                }
            }
            else if (authReq.AuthType == AuthType.GameCenter)
            {
                GameCenterAuth gameCenterAuth = request.GameCenterAuth;
                if (gameCenterAuth == null)
                {
                    _logger.Error($"gameCenterAuth is null");
                    response.SetResult(ErrorCode.InvalidParameter, "gameCenterAuth is null");
                    return Ok(response);
                }

                //_logger.Info($"gameCenterAuth: {gameCenterAuth.ToString()}");
                if (GameCenterVerifier.Verify(gameCenterAuth) == false)
                {
                    _logger.Error($"VerifyGameCenter: false, [authReq: {authReq}]");
                    _logger.Error($"gameCenterAuth: {gameCenterAuth.ToString()}");
                    response.SetResult(ErrorCode.InternalError);
                    return Ok(response);
                }
            }
            else if (authReq.AuthType != AuthType.UnitySocial)
            {
                response.SetResult(ErrorCode.InternalError);
                return Ok(response);
            }

            // verify 성공 정보를 redis 에 남기자
            await _service.SetVerifySocial(authReq);
            return Ok(response);
        }

        [HttpPost]
        [Route(DefaultAreaRoute)]
        [Consumes("application/json")]
        public async Task<ActionResult<CheckAuthResponse>> CheckAuth([FromBody] CheckAuthRequest request)
        {
            AuthReq authReq = request.AuthReq;
            AuthReq authReq_o = request.AuthReq_o;

            authReq.Country = StaticInfo.Instance.GeoLite2Info.getCountryIso(HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(), authReq.Country);

            CheckAuthResponse response = await _service.CheckAuth(authReq, authReq_o);
            return Ok(response);
        }

        [HttpPost]
        [Route(DefaultAreaRoute)]
        [Consumes("application/json")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] AuthReq authReq)
        {
            LoginResponse response = await _service.Login(authReq);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<PlayerInfoResponse>> PlayerInfo(ulong oidAccount)
        {
            return Ok(await _service.GetPlayerInfo(oidAccount));
        }

        [HttpPost]
        [Route("{targetOidAccount}")]
        public async Task<ActionResult<OtherPlayerInfoResponse>> OtherPlayerInfo(ulong oidAccount, [Required] ulong targetOidAccount)
        {
            return Ok(await _service.GetOtherPlayerInfo(targetOidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<SetNicknameResponse>> SetNickname(ulong oidAccount,
            [Required][FromForm] string nickname)
        {
            return Ok(await _service.SetNickname(oidAccount, nickname));
        }

        [HttpPost]
        public async Task<ActionResult<UpgradeTalentResponse>> UpgradeTalent(ulong oidAccount,
            [Required][FromForm] bool useJewel)
        {
            return Ok(await _service.UpgradeTalent(oidAccount, useJewel));
        }

        [HttpPost]
        public async Task<ActionResult<CompletedMissionsResponse>> GetCompletedMissions(ulong oidAccount)
        {
            return Ok(await _service.GetCompletedMissions(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<MissionCompleteResponse>> MissionComplete(ulong oidAccount,
            [Required][FromForm] int missionId)
        {
            return Ok(await _service.MissionComplete(oidAccount, missionId));
        }
    }
}