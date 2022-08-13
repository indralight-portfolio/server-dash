using System.Collections;
using System.Threading.Tasks;

namespace server_dash.Utility
{
    public class TaskEnumerator<T>
    {
        public T TaskResult => _task.Result;

        private Task<T> _task;

        public TaskEnumerator(Task<T> task)
        {
            _task = task;
        }

        public IEnumerator WaitTask()
        {
            while (_task.IsCompleted == false)
            {
                yield return null;
            }

            if (_task.Exception != null)
            {
                throw _task.Exception;
            }
        }
    }
}