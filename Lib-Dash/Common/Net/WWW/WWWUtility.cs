using System;
using System.Text.RegularExpressions;

namespace Common.Net.WWW
{
    public static class WWWUtility
    {
        public static string ReplacePathVariable(string path, params object[] pathVariables)
        {
            if (pathVariables == null || pathVariables.Length == 0)
            {
                return path;
            }
            
            Regex ex = new Regex("{[0-9]}");
            MatchCollection matches = ex.Matches(path);

            if (matches.Count > 0 && pathVariables.Length != matches.Count)
            {
                throw new Exception("Path Variables doesn't match with params");
            }

            return string.Format(path, pathVariables);
        }
    }
}