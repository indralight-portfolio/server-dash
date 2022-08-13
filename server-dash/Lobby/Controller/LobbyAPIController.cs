using Microsoft.AspNetCore.Mvc;

namespace server_dash.Lobby.Controller
{
    [ApiExplorerSettings(GroupName = nameof(WebAPI.Lobby))]
    [Area(nameof(WebAPI.Lobby))]
    [Route(DefaultAreaRoute + "/{oidAccount}")]
    public abstract class LobbyAPIController : BaseAPIController
    {
    }
}