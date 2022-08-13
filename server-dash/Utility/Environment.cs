using Common.Utility;
using Dash.Model.Service;

namespace server_dash.Utility
{
    public static class EnvUtility
    {
        const string LOCAL_POSTFIX = "-local";
        const string DEVELOPMENT = "dev";

        public static string EnvironmentName { get; private set; }
        public static bool IsLocal => IsDevelopment || EnvironmentName.Contains(LOCAL_POSTFIX);
        public static bool IsDevelopment => EnvironmentName.Contains(DEVELOPMENT);
        public static Env Env { get; private set; }

        public static void SetEnv(string environmentName)
        {
            EnvironmentName = environmentName;
            string envString = environmentName.Replace(LOCAL_POSTFIX, string.Empty).ToUpper();
            if (EnumInfo<Env>.TryParse(envString, out Env env) == true)
            {
                Env = env;
            }
            Env = Env.DEV;
        }
    }
}