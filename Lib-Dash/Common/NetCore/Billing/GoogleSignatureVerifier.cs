#if Common_NetCore && Common_Mobile

// BouncyCastle 설치 필요
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.NetCore.Billing
{
    public class GoogleSignatureVerifier
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        RSAParameters _rsaKeyInfo;

        public GoogleSignatureVerifier(string googlePublicKey)
        {
            RsaKeyParameters rsaParameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(googlePublicKey));
 
            byte[] rsaExp = rsaParameters.Exponent.ToByteArray();
            byte[] Modulus = rsaParameters.Modulus.ToByteArray();
 
            // Microsoft RSAParameters modulo wants leading zero's removed so create new array with leading zero's removed
            int Pos = 0;
            for (int i = 0; i < Modulus.Length; i++)
            {
                if (Modulus[i] == 0)
                {
                    Pos++;
                }
                else
                {
                    break;
                }
            }
            byte[] rsaMod = new byte[Modulus.Length - Pos];
            Array.Copy(Modulus, Pos, rsaMod, 0, Modulus.Length - Pos);
 
            // Fill the Microsoft parameters
            _rsaKeyInfo = new RSAParameters()
            {
                Exponent = rsaExp,
                Modulus = rsaMod
            };
 
        }

        public bool Verify(string message, string signature)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(_rsaKeyInfo);
                return rsa.VerifyData(Encoding.ASCII.GetBytes(message), "SHA1", Convert.FromBase64String(signature));
            }
        }
    }
}
#endif