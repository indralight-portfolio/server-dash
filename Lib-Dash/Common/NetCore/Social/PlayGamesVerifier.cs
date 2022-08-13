#if Common_NetCore && Common_Mobile

// Google.Apis.Auth 설치 필요
using Google.Apis.Auth;
using System;
using System.Threading.Tasks;

namespace Common.NetCore.Social
{
    public class PlayGamesVerifier
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private const string GOOGLE_WEB_CLIENT = "917497646166-h6ssn8otm9fau1n50kf689tpaurlkmc9.apps.googleusercontent.com";

        public static async Task<bool> Verify(string idToken)
        {
            try
            {
                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                //_logger.Info($"payload.Audience: {payload.Audience}");
                if (payload.Audience.Equals(GOOGLE_WEB_CLIENT) == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //_logger.Fatal(e);
                return false;
            }
        }
    }
}
#endif