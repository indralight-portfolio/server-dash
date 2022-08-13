using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Common.Utility
{
    public class SslHelper
    {
        public static bool CheckServerCertificate(object sender, X509Certificate certificate,
               X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // the lazy version here is:
            return true;

            // better version - check that the CA thumbprint is in the chain
            //if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            //{
            //    // check that the untrusted ca is in the chain
            //    var ca = new X509Certificate2("ca.pem");
            //    var caFound = chain.ChainElements
            //        .Cast<X509ChainElement>()
            //        .Any(x => x.Certificate.Thumbprint == ca.Thumbprint);

            //    // note you could also hard-code the expected CA thumbprint,
            //    // but pretty easy to load it from the pem file that aiven provide

            //    return caFound;
            //}
            //return false;
        }
    }
}
