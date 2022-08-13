using Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace server_dash
{
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json", "application/x-msgpack")]
    [ApiController]
    public abstract class BaseAPIController : ControllerBase
    {
        public const string DefaultRoute = "~/[controller]/[action]";
        public const string DefaultAreaRoute = "~/[area]/[controller]/[action]";
    }
}