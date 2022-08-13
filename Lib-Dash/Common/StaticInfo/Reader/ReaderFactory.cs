namespace Common.StaticInfo.Reader
{
    public static class ReaderFactory
    {
        public static IStaticInfoReader<T> Create<T>(string filePath, StaticInfoFormatType formatType)
        {
            IStaticInfoReader<T> reader = null;
            if (filePath.LastIndexOf(".json", System.StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                reader = new JsonReader<T>();
            }
            if (filePath.LastIndexOf(".xml", System.StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                reader = new XmlReader<T>();
            }
            if (filePath.LastIndexOf(".bytes", System.StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                if(formatType == StaticInfoFormatType.MPack)
                {
                    reader = new MPackReader<T>();
                }
                else if(formatType == StaticInfoFormatType.Binary)
                {
                    reader = new BinaryReader<T>();
                }
            }

            reader?.Init(filePath);

            return reader;
        }
    }
}