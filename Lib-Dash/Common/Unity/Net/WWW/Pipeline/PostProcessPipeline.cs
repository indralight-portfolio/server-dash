using System;
using System.Collections.Generic;

namespace Common.Unity.Net.WWW.Pipeline
{
    public enum HttpMessageProcessResultType
    {
        Success = 0,
        Error,
        Exception,
    }

    public class HttpMessageProcessResult
    {
        public HttpMessageProcessResultType ResultType;
        public string ErrorMessage;
    }
    
    public class PostProcessPipeline
    {
        public delegate void PostProcessDelegate(HttpMessageProcessResult result);
        private List<PostProcessDelegate> _actions = new List<PostProcessDelegate>();
        public void PostProcess(HttpMessageProcessResult result)
        {
            for (int i = 0; i < _actions.Count; ++i)
            {
                _actions[i](result);
            }
        }

        public void Add(PostProcessDelegate action)
        {
            _actions.Add(action);
        }

        public void Remove(PostProcessDelegate action)
        {
            _actions.Remove(action);
        }
    }
}