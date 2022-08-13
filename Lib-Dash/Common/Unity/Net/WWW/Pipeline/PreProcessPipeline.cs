#if Common_Unity
using System.Collections.Generic;

namespace Common.Unity.Net.WWW.Pipeline
{
    public interface IPreProcessPipeline
    {
        bool PreProcess(object message);
    }

    public class PreProcessPipeline<T> : IPreProcessPipeline
    {
        public delegate bool PreProcessDelegate(T message);

        private List<PreProcessDelegate> _actions = new List<PreProcessDelegate>();

        public void Add(PreProcessDelegate action)
        {
            _actions.Add(action);
        }

        public void Remove(PreProcessDelegate action)
        {
            _actions.Remove(action);
        }

        public void Remove(System.Delegate action)
        {
            Remove((PreProcessDelegate)action);
        }

        bool IPreProcessPipeline.PreProcess(object message)
        {
            return PreProcess((T) message);
        }

        public bool PreProcess(T message)
        {
            for (int i = 0; i < _actions.Count; ++i)
            {
                bool valid = _actions[i](message);
                if (valid == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif