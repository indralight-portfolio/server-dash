using System;

namespace Common.StaticInfo.Writer
{
    public static class WriterFactory
    {
        public static IStaticInfoWriter<T> Create<T>(string filePath, StaticInfoFormatType formatType)
        {
            IStaticInfoWriter<T> reader = null;
            if (filePath.LastIndexOf(".json", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                reader = new JsonWriter<T>();
            }
            else if (filePath.LastIndexOf(".xml", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                reader = new XmlWriter<T>();
            }
            else if (filePath.LastIndexOf(".byte", StringComparison.InvariantCultureIgnoreCase) != -1 && formatType == StaticInfoFormatType.Binary)
            {
                reader = new BinaryWriter<T>();
            }
            else if (filePath.LastIndexOf(".byte", StringComparison.InvariantCultureIgnoreCase) != -1 && formatType == StaticInfoFormatType.MPack)
            {
                reader = new MPackWriter<T>();
            }

            if (reader == null)
            {
                throw new Exception($"Create writer failed, path : {filePath}");
            }

            reader.Init(filePath);

            return reader;
        }
    }
}