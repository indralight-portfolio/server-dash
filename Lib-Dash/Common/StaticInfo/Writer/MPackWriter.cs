using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.StaticInfo.Writer
{
    public class MPackWriter<T> : LocalFileWriter<T>
    {
        public override void Write(T data)
        {
            byte[] bytes = MessagePack.MessagePackSerializer.Serialize(data);
            WriteAllBytes(bytes);
        }
    }
}
