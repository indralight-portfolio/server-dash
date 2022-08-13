namespace Common.StaticInfo.Writer
{
    public interface IStaticInfoWriter<T>
    {
        void Init(string path);
        void Write(T data);
    }
}