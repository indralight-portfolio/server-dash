using System;
using Dash.Types;

namespace Dash.Net
{
    public class NotBoundMessageException : Exception
    {
        public NotBoundMessageException(int hashCode, ServiceAreaType type) :
            base($"No boundMessage[{hashCode}][{type}]")
        {
        }
        public NotBoundMessageException(int hashCode) :
            base($"No boundMessage[{hashCode}]")
        {
        }
    }

    public class AlreadyMessageBindingException : Exception
    {
        public AlreadyMessageBindingException(int handlerHashCode) :
            base($"AlreadyMessageBind[{handlerHashCode}]")
        {
        }
    }
}