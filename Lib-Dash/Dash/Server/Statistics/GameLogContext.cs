#if Common_Server
using Newtonsoft.Json;
using System;

namespace Dash.Server.Statistics
{
    public class GameLogContext
    {
        [JsonIgnore] public CommandType CommandType { get; set; }
        public ulong OidAccount;
        public DateTime Time;
        public string Command
        {
            get
            {
                return Commands.GetValue(CommandType);
            }
        }
        [JsonProperty("Level")]
        public int AccountLevel;
        public IPayload PayLoad;
    }
}
#endif