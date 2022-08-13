#if Common_NetCore && Common_Mobile
using Common.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Common.NetCore.Social
{
    public class GameCenterVerifier
    {
        public static Dictionary<string, byte[]> dicCerts = new Dictionary<string, byte[]>();

        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        public static bool Verify(GameCenterAuth auth)
        {
            try
            {
                var cert = GetCertificate(auth.PublicKeyUrl);
                if (cert.Verify())
                {
                    //RSA rsaCapi = (RSACryptoServiceProvider)cert.PublicKey.Key;
                    RSA rsa = cert.GetRSAPublicKey();
                    if (rsa != null)
                    {
                        var sha256 = new SHA256Managed();
                        var sig = ConcatSignature(auth.PlayerId, auth.BundleId, auth.Timestamp, auth.Salt);
                        var hash = sha256.ComputeHash(sig);

                        if (rsa.VerifyHash(hash, Convert.FromBase64String(auth.Signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) == true)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                //_logger.Fatal(e);
                return false;
            }
        }

        private static byte[] ToBigEndian(ulong value)
        {
            var buffer = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                buffer[7 - i] = unchecked((byte)(value & 0xff));
                value = value >> 8;
            }
            return buffer;
        }

        private static X509Certificate2 GetCertificate(string url)
        {
            if (dicCerts.ContainsKey(url) == false)
            {
                var client = new WebClient();
                var rawData = client.DownloadData(url);
                dicCerts[url] = rawData;                
            }
            return new X509Certificate2(dicCerts[url]);
        }

        private static byte[] ConcatSignature(string playerId, string bundleId, ulong timestamp, string salt)
        {
            var data = new List<byte>();
            data.AddRange(Encoding.UTF8.GetBytes(playerId));
            data.AddRange(Encoding.UTF8.GetBytes(bundleId));
            data.AddRange(ToBigEndian(timestamp));
            data.AddRange(Convert.FromBase64String(salt));
            return data.ToArray();
        }
    }
}
#endif