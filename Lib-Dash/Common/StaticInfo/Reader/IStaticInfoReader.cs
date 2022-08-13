using System.Collections;
using System.Threading.Tasks;

namespace Common.StaticInfo.Reader
{
    public interface IStaticInfoReader<T>
    {
        void Init(string path);
        T Read(bool isList);
        Task<T> ReadAsync(bool isList);
    }
}