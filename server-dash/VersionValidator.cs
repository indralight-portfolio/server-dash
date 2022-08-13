using Dash;

namespace server_dash
{
    public class VersionValidator
    {
        public static bool Validate(string clientVersion)
        {
            try
            {
                bool valid = true;
                var clientVersionEntity = new VersionEntity(clientVersion);
                var serverVersionEntity = new VersionEntity(BuildVersion.Version);
                valid &= serverVersionEntity.IsCompatible(clientVersionEntity);

                return valid;
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidateRev(string revDash, string revData)
        {
            if(string.IsNullOrEmpty(revDash) || string.IsNullOrEmpty(revDash))
            {
                return false;
            }
            bool valid = true;
            // lib-dash, data-dash revision check
            valid &= revDash.Equals(BuildVersion.RevDash) && revData.Equals(BuildVersion.RevData);

            return valid;
        }
    }
}