using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace server_dash
{
    public sealed class ServerIPManager
    {
        private static volatile ServerIPManager _instance;
        private static readonly object SyncRoot = new object();

        public string IP { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;

        public static ServerIPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServerIPManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private ServerIPManager()
        {
        }

        public void Init(string path)
        {
            HostName = Dns.GetHostName();
            IP = Dns.GetHostAddresses(HostName)
                .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "";

            if (File.Exists(path))
            {
                using (StreamReader file = System.IO.File.OpenText(path))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    HostName = o["InstanceId"].ToString();
                    IP = o["IP"].ToString();
                    Endpoint = o["Endpoint"].ToString();
                }
            }
            else
            {
                Endpoint = IP;
            }
        }
    }
}