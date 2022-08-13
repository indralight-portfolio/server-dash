using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace Dash.Net
{
    /// <summary>
    /// 이 클래스를 통해 얻은 byte[]를 호출이후 캐싱두는 것이 Unsafe함.
    /// </summary>
    public class UnsafeBufferPool
    {
        /// <summary>
        /// Buffer Size : [ 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096,  ...]
        /// Index       : [ 0, 1, 2, 3,  4,  5,  6,   7,   8,   9,    10,   11,    ...]
        /// </summary>
        private List<List<byte[]>> _buffers;
        private object _lockObject = new object();
        public UnsafeBufferPool()
        {
            _buffers = new List<List<byte[]>>();
            for (int i = 0; i < 20; ++i)
            {
                _buffers.Add(new List<byte[]>());
            }
        }

        public ArraySegment<byte> MakeManagedBuffer(ArraySegment<byte> targetBuffer)
        {
            byte[] buffer = null;
            lock (_lockObject)
            {
                int bufferListIndex = GetBufferListIndex(targetBuffer.Count);
                List<byte[]> bufferList = _buffers[bufferListIndex];
                if (bufferList.Count == 0)
                {
                    buffer = new byte[GetBufferSize(bufferListIndex)];
                }
                else
                {
                    buffer = bufferList[bufferList.Count - 1];
                    bufferList.RemoveAt(bufferList.Count - 1);
                }
            }

            Buffer.BlockCopy(targetBuffer.Array, targetBuffer.Offset, buffer, 0, targetBuffer.Count);
            return new ArraySegment<byte>(buffer, 0, targetBuffer.Count);
        }

        public void ReturnBuffer(ArraySegment<byte> buffer)
        {
            lock (_lockObject)
            {
                int index = GetBufferListIndex(buffer.Count);
                _buffers[index].Add(buffer.Array);
            }
        }

        private static int GetBufferSize(int index)
        {
            int size = 2;
            for (int i = 0; i < index; ++i)
            {
                size *= 2;
            }

            return size;
        }

        private static int GetBufferListIndex(int bufferLength)
        {
            // 하드 코드로 좀더 최적화할 수 있을 듯.

            // bufferLength : 1 index : 0
            // bufferLength : 2 index : 0
            // bufferLength : 3 index : 1
            // bufferLength : 4 index : 1
            // bufferLength : 9 index : 3
            int size = 2;
            for (int i = 0; i < 20; ++i)
            {
                if (bufferLength <= size)
                {
                    return i;
                }

                size *= 2;
            }

            throw new NotSupportedException($"BufferSize not allowable : {bufferLength}");
        }
    }
}