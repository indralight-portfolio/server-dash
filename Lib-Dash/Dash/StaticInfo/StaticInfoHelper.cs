#if Admin_Server
using Common.StaticInfo;

namespace Dash.StaticInfo
{
    public static class StaticInfoHelper
    {
        public static string GetNameComment<T>(this T info) where T : IHasName
        {
            string name = null;
            if (info is IHasName infoName)
            {
                if (infoName.GetName()?.TryGet(out name) == false)
                {
                    name = null;
                }
            }
            if (info is IHasComment infoComment)
            {
                name = name ?? infoComment.GetComment();
            }
            return name;
        }
    }
}
#endif