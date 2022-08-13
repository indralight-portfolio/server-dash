using System.Xml.Serialization;

namespace Common.StaticInfo.Writer
{
    using Utility;
    public class XmlWriter<T> : LocalFileWriter<T>
    {
        public override void Write(T data)
        {
            var stream = base.CreateFileStream();

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (UTF8StringWriter writer = new UTF8StringWriter())
            {
                serializer.Serialize(writer, data);
                stream.Write(writer.ToString());
            }

            stream.Close();
        }
    }
}