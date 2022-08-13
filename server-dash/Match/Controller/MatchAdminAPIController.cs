using Microsoft.AspNetCore.Mvc;

namespace server_dash.Match.Controller
{
    [ApiExplorerSettings(GroupName = nameof(WebAPI.MatchAdmin))]
    [Area(nameof(WebAPI.MatchAdmin))]
    [Route(DefaultAreaRoute)]
    public abstract class MatchAdminAPIController : BaseAPIController
    {
    }
}