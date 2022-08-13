namespace Dash
{
    public class VersionEntity
    {
        private int major { get; set; } = -1;
        private int minor { get; set; } = -1;
        private string hotfix { get; set; } = "";
        private string versionString { get; set; }

        public VersionEntity(string version)
        {
            versionString = version;
            var arr = version.Split('.');
            if (arr.Length == 3)
            {
                major = int.Parse(arr[0]);
                minor = int.Parse(arr[1]);
                hotfix = arr[2];
            }
        }

        public bool IsCompatible(VersionEntity other)
        {
            if (major < 0 || minor < 0)
            {
                return versionString == other.versionString;
            }
            else
            {
                return (major == other.major || other.major == 0) && minor == other.minor;
            }
        }
    }
}