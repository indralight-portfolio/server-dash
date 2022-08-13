#if Common_Unity
namespace Common.Unity.Net.WWW
{
    public class HttpMessageAttribute : System.Attribute
    {
        public string API { get; }

        public HttpMessageAttribute(string api)
        {
            this.API = api;
        }
    }
}
#endif