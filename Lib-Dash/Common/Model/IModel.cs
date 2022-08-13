using System.Collections.Generic;

namespace Common.Model
{
    public interface IModel
    {
        string GetMainKey();
        List<string> GetSubKeys();
        bool IsAutoIncKeysValid();
    }
}
