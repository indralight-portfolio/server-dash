namespace Common.Utility
{
    public class RandomProvider
    {
        public static int GetSeed()
        {
            return System.DateTime.UtcNow.Millisecond;
        }
    }
}