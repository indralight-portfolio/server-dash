using Microsoft.AspNetCore.Mvc;

namespace server_dash.Lobby.Controller
{
    [ApiExplorerSettings(GroupName = nameof(WebAPI.Match))]
    [Area(nameof(WebAPI.Match))]
    [Route(DefaultAreaRoute + "/{oidAccount}")]
    public abstract class MatchAPIController : BaseAPIController
    {
    }
}