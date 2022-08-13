using System.Collections.Generic;
using System.Text;

namespace Dash
{
    public interface ILoggable
    {
        string LogStr { get; }
    }

    public static class LogExtension
    {
        public static string LogOid(this ulong oidAccount)
        {
            return $"[oid:@{oidAccount}]";
        }

        public static string LogOids(this List<ulong> oidAccounts)
        {
            var sb = new StringBuilder();
            sb.Append("[oid:");
            for(int i = 0; i < oidAccounts.Count; ++i)
            {
                sb.Append('@');
                sb.Append(oidAccounts[i]);
                sb.Append('|');
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}