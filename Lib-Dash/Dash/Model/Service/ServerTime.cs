using Common.Utility;
using System;

namespace Dash.Model.Service
{

    public class ServerTime
    {
        public DateTime Utc;
        public TimeSpan Offset;

        public DateTime Local => Utc.ToLocalTime(Offset);

        public ServerTime(DateTime utc, TimeSpan offset)
        {
            Utc = utc;
            Offset = offset;
        }
    }
}