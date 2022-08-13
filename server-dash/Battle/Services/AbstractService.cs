using System.Collections;
using System.Threading.Tasks;

namespace server_dash.Battle.Services
{
    public abstract class AbstractService
    {
        protected IEnumerator WaitTask(Task task)
        {
            while (task.IsCompleted == false)
            {
                yield return null;
            }

            if (task.Exception != null)
            {
                throw task.Exception;
            }
        }
    }
}