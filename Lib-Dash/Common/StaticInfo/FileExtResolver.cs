namespace Common.StaticInfo
{

    public static class FileExtResolver
    {
        public static string[] Resolve(StaticInfoFormatType formatType)
        {
            if (formatType == StaticInfoFormatType.Anything)
            {
                return new string[] { };
            }
            else if (formatType == StaticInfoFormatType.MPackOrJson)
            {
                return new string[] { $".bytes", $".json" };
            }
            else if (formatType == StaticInfoFormatType.MPack)
            {
                return new string[] { $".bytes" };
            }
            else if(formatType == StaticInfoFormatType.BinaryOrJson)
            {
                return new string[] { $".bytes", $".json" };
            }
            else if (formatType == StaticInfoFormatType.Binary)
            {
                return new string[] { $".bytes" };
            }
            return new string[] { $".{formatType.ToString().ToLower()}" };
        }
    }
}