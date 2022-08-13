using Microsoft.AspNetCore.Mvc;

namespace server_dash.Lobby.Controller
{
    [ApiExplorerSettings(GroupName = nameof(WebAPI.Etc))]
    [Area(nameof(WebAPI.Etc))]
    [Route(DefaultAreaRoute + "/{oidAccount}")]
    public abstract class EtcAPIController : BaseAPIController
    {
    }
}