#if Common_NetCore
using System;
using System.Threading.Tasks;

namespace Dash.Server
{
    public class AsyncTaskWrapper
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        public static async void Call(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                _logger.Fatal(e.Message);
                _logger.Fatal(e.StackTrace);
            }
        }
    }
}
#endif