using Dash.Model.Service;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using server_dash.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace server_dash.Lobby.Controller.Etc
{
    [Route(DefaultAreaRoute)]
    [Produces("application/json")]
    public class HealthController : EtcAPIController
    {
        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new
            {
                //Service = ServiceAreaType.Lobby,
                ASPNETCORE_ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Version = BuildVersion.Version,
                RevServer = BuildVersion.RevServer,
                RevDash = BuildVersion.RevDash,
                RevData = BuildVersion.RevData,
                HostName = ServerIPManager.Instance.HostName,
                IP = ServerIPManager.Instance.IP,
            });
        }

        [FilterEnv(Exclude = new Env[] { Env.LIVE })]
        [HttpPost]
        public IActionResult GCCollect() // TODO: Live에서 동작하지 않게
        {
            GC.Collect();
            Console.WriteLine(("[HealthController] GCCollect done"));
            return Ok();
        }

        [FilterEnv(Exclude = new Env[] { Env.LIVE })]
        [HttpPost]
        public IActionResult GCAlloc([Required][FromForm] int size, [Required][FromForm] int repeatCount) // TODO: Live에서 동작하지 않게
        {
            List<byte[]> list = new List<byte[]>();
            for (int i = 0; i < repeatCount; ++i)
            {
                var bytes = new byte[size];
                list.Add(bytes);
            }
            Console.WriteLine(("[HealthController] GCAlloc done"));
            return Ok();
        }
    }
}