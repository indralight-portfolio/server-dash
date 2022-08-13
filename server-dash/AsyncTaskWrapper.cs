using System;
using System.Threading.Tasks;

namespace server_dash
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