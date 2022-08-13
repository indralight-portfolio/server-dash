namespace Common.Utility
{
    public class StringHash
    {
        public static int GetHashCode(string s)
        {
            int hash = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                hash += s[i];
                hash += hash << 10;
                hash ^= hash >> 6;
            }

            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;

            return hash;
        }
    }
}