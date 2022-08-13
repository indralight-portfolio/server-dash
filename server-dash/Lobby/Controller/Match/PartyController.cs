using Dash.Model;
using Dash.Protocol;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using server_dash.Net.Sessions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Match
{
    public class PartyController : MatchAPIController
    {
        private readonly MatchService _service;

        public PartyController(MatchService matchService)
        {
            _service = matchService;
        }

        [HttpPost]
        public async Task<ActionResult<GetSocialServerResponse>> GetSocialServer(ulong oidAccount)
        {
            return Ok(await _service.GetSocialServer(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<GetSocialServerResponse>> GetPartyConnectInfo(ulong oidAccount,
            [Required][FromForm]string partyCode)
        {
            return Ok(await _service.GetPartyConnectInfo(oidAccount, partyCode));
        }
    }
}
