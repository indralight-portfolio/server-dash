#pragma warning disable SYSLIB0011 // 형식 또는 멤버는 사용되지 않습니다.
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Common.StaticInfo.Writer
{
    public class BinaryWriter<T> : LocalFileWriter<T>
    {
        static BinaryWriter()
        {
        }
        public override void Write(T data)
        {
            var fileStream = CreateFileStream();
            using BsonWriter bsonWriterObjet = new BsonWriter(fileStream.BaseStream);
            JsonSerializer jsonSerializer = new JsonSerializer();
            foreach(var converter in JsonGlobalSettings.WriterSettings.Converters)
            {
                jsonSerializer.Converters.Add(converter);
            }
            jsonSerializer.Serialize(bsonWriterObjet, data);
            fileStream.Close();
        }
    }
}
#pragma warning restore SYSLIB0011 // 형식 또는 멤버는 사용되지 않습니다.