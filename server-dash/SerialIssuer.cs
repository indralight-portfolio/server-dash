using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace server_dash
{
    public class SerialIssuer
    {
        private int _lastIssuedSerial;

        public SerialIssuer() : this(0)
        {
        }

        public SerialIssuer(int initSerial)
        {
            _lastIssuedSerial = initSerial;
        }

        public int Issue()
        {
            int incremented = Interlocked.Increment(ref _lastIssuedSerial);
            return incremented;
        }
    }
    public class RangeSerialIssuer
    {
        private int _lastIssuedSerial;
        private int _end;
        private int _threshold;
        private ConcurrentQueue<(int, int)> _ranges = new ConcurrentQueue<(int, int)>();
        public Action RequestSerialRange;

        public void SetRange(int begin, int end)
        {
            _ranges.Enqueue((begin, end));
        }
        public bool CanIssue()
        {
            return _ranges.Count != 0 || _end != _lastIssuedSerial;
        }
        public int Issue()
        {
            if(_end <= _lastIssuedSerial)
            {
                _ranges.TryDequeue(out var result);
                _lastIssuedSerial = result.Item1; //10000
                _end = result.Item2; //20000
                _threshold = _lastIssuedSerial + (int)((_end - _lastIssuedSerial) * 0.7f);//70%이상 차면 다음 시리얼을 가지고옴
            }
            else if(_threshold == _lastIssuedSerial)
            {
                RequestSerialRange?.Invoke();
            }
            int incremented = Interlocked.Increment(ref _lastIssuedSerial);
            return incremented;
        }
    }
}