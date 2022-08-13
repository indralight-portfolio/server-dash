using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Match
{
    public class MultiplayController : MatchAPIController
    {
        private readonly MatchService _service;

        public MultiplayController(MatchService matchService)
        {
            _service = matchService;
        }

        [HttpPost]
        public async Task<ActionResult<EnqueueMatchResponse>> EnqueueMatch(ulong oidAccount,
            [Required][FromForm]int chapterId, [Required][FromForm]string nickname)
        {
            return Ok(await _service.EnqueueMatch(oidAccount, chapterId, nickname));
        }

        [HttpPost]
        public async Task<ActionResult<DequeueMatchResponse>> DequeueMatch(ulong oidAccount)
        {
            return Ok(await _service.DequeueMatch(oidAccount));
        }
        [HttpPost]
        public async Task<ActionResult<LastEnqueuePlayerInfoResponse>> LastEnqueuePlayerInfo(ulong oidAccount)
        {
            return Ok(await _service.GetLastEnqueuePlayerInfo());
        }
        [HttpPost]
        public async Task<ActionResult<CheckMatchResponse>> CheckMatch(ulong oidAccount)
        {
            return Ok(await _service.CheckMatch(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<CheckArenaResponse>> CheckArena(ulong oidAccount)
        {
            return Ok(await _service.CheckArena(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<ForgiveArenaResponse>> ForgiveArena(ulong oidAccount)
        {
            return Ok(await _service.ForgiveArena(oidAccount));
        }
    }
}