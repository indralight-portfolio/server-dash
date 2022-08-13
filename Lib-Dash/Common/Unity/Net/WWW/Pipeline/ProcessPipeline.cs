#if Common_Unity
using System.Collections.Generic;

namespace Common.Unity.Net.WWW.Pipeline
{
    public interface IProcessPipeline
    {
        void Add(System.Delegate action);
        void Remove(System.Delegate action);
        HttpMessageProcessResult Process(object data);
    }

    public class ProcessPipeline<T> : IProcessPipeline
    {
        private List<System.Func<T, bool>> _actions = new List<System.Func<T, bool>>();

        public void Add(System.Func<T, bool> action)
        {
            _actions.Add(action);
        }

        void IProcessPipeline.Add(System.Delegate action)
        {
            Add((System.Func<T, bool>)action);
        }

        public void Remove(System.Func<T, bool> action)
        {
            _actions.Remove(action);
        }

        public void Remove(System.Delegate action)
        {
            Remove((System.Func<T, bool>)action);
        }

        HttpMessageProcessResult IProcessPipeline.Process(object data)
        {
            return Process((T)data);
        }

        public HttpMessageProcessResult Process(T data)
        {
            HttpMessageProcessResult result = new HttpMessageProcessResult();
            result.ResultType = HttpMessageProcessResultType.Success;
            try
            {
                for (int i = 0; i < _actions.Count; ++i)
                {
                    bool valid = _actions[i](data);
                    if (valid == false)
                    {
                        result.ResultType = HttpMessageProcessResultType.Error;
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                result.ResultType = HttpMessageProcessResultType.Exception;
            }

            return result;
        }
    }
}
#endif