using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Net.WWW
{
    public abstract class HeaderConstants
    {
        public const string AcceptHeader = "Accept";
        public const string ContentTypeHeader = "Content-Type";
        public const string VersionHeader = "version";
        public const string RevDashHeader = "rev_dash";
        public const string RevDataHeader = "rev_data";

        public const string SessionKeyHeader = "session_key";
        public const string DeviceNameHeader = "device_name";
        public const string DUIDHeader = "duid";
        
        public abstract class ContentType
        {
            public const string Json = "application/json";
            public const string MessagePack = "application/x-msgpack";
        }
    }
}
