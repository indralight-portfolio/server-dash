using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Lobby
{
    public class MailController : LobbyAPIController
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private MailService _service;

        public MailController(MailService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ListMailResponse>> List(ulong oidAccount)
        {
            return Ok(await _service.List(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<ListMailResponse>> SendTemplateMail(ulong oidAccount)
        {
            await _service.SendTemplateMail(oidAccount);
            return Ok(await _service.List(oidAccount));
        }

        [HttpPost]
        public async Task<ActionResult<ReceiveMailRewardResponse>> ReceiveReward(ulong oidAccount,
            [Required][FromForm]ulong id)
        {
            return Ok(await _service.ReceiveReward(oidAccount, id));
        }

        [HttpPost]
        public async Task<ActionResult<ReadMailResponse>> Read(ulong oidAccount, 
            [Required][FromForm]ulong id)
        {
            return Ok(await _service.Read(oidAccount, id));
        }
    }
}
