using System;
using Dash.Types;

namespace server_dash.Net.Handlers
{
    public class BindMessageAttribute : Attribute
    {
        public ServiceAreaType[] AreaTypes { get; private set; }
        public BindMessageAttribute(params ServiceAreaType[] areaTypes)
        {
            AreaTypes = areaTypes;
        }
    }

    public class CoroutineAttribute : Attribute
    {
        public ServiceAreaType[] AreaTypes { get; private set; }
        public CoroutineAttribute(params ServiceAreaType[] areaTypes)
        {
            AreaTypes = areaTypes;
        }
    }

    public class BindRawMessageAttribute : Attribute
    {
        public int HashCode { get; }
        public BindRawMessageAttribute(int hashCode)
        {
            HashCode = hashCode;
        }
    }
    public class BindInternalMessageAttribute : Attribute
    {
        public BindInternalMessageAttribute()
        {
        }
    }
}