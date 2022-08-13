#if Common_Unity
using System;
using System.Collections.Generic;

namespace Common.Unity.Net.WWW.Pipeline
{
    public class RawResponsePipeline
    {
        public delegate void RawStringResponseProcessDelegate(string message);
        public delegate void RawBytesResponseProcessDelegate(ReadOnlyMemory<byte> message);

        private List<RawStringResponseProcessDelegate> _stringActions = new List<RawStringResponseProcessDelegate>();
        private List<RawBytesResponseProcessDelegate> _bytesActions = new List<RawBytesResponseProcessDelegate>();

        public void Add(RawStringResponseProcessDelegate action)
        {
            _stringActions.Add(action);
        }

        public void Add(RawBytesResponseProcessDelegate action)
        {
            _bytesActions.Add(action);
        }

        public void Remove(RawStringResponseProcessDelegate action)
        {
            _stringActions.Remove(action);
        }

        public void PreProcess(string data)
        {
            for (int i = 0; i < _stringActions.Count; ++i)
            {
                _stringActions[i](data);
            }
        }

        public void PreProcess(ReadOnlyMemory<byte> data)
        {
            for (int i = 0; i < _bytesActions.Count; ++i)
            {
                _bytesActions[i](data);
            }
        }
    }
}
#endif