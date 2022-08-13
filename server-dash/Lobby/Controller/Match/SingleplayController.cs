using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Match
{
    public class SingleplayController : MatchAPIController
    {
        private readonly MatchService _service;

        public SingleplayController(MatchService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<GetBattleServerResponse>> GetBattleServer(ulong oidAccount)
        {
            return Ok(await _service.GetBattleServer());
        }
    }
}