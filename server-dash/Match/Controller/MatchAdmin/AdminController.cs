using Dash.Model;
using Dash.Protocol;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using server_dash.Match.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using server_dash.Net.Sessions;
using System.Linq;

namespace server_dash.Match.Controller.MatchAdmin
{
    [Produces("application/json")]
    [Route("~/[area]/[action]")]
    public class AdminController : MatchAdminAPIController
    {
        private readonly ServerCoordinator _serverCoordinator;
        private readonly SessionService _sessionService;
        public AdminController(ServerCoordinator battleServerCoordinator, SessionService sessionService)
        {
            _serverCoordinator = battleServerCoordinator;
            _sessionService = sessionService;
        }

        [HttpPost]
        public async Task<ActionResult<GetServersEnabledResponse>> GetServersEnabled()
        {
            return Ok(new GetServersEnabledResponse { ServersEnabled = _serverCoordinator.GetServersEnabled() });
        }

        [HttpPost]
        public async Task<ActionResult<SetServerEnableResponse>> SetServerEnable([Required][FromForm] string uuid, [Required][FromForm] bool enable)
        {
            ServerSession server = _serverCoordinator.SetServerEnable(uuid, enable);

            return Ok(new SetServerEnableResponse { UUID = server?.UUID });
        }

        [HttpPost]
        public async Task<ActionResult<HttpResponseModel>> KickAll()
        {
            _sessionService.BroadCastKickAll();
            return Ok(new HttpResponseModel(ErrorCode.Success));
        }
    }
}