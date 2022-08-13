using System.IO;

namespace Common.StaticInfo.Writer
{
    public abstract class LocalFileWriter<T> : IStaticInfoWriter<T>
    {
        private string _path;

        public virtual void Init(string path)
        {
            _path = path;
        }

        public abstract void Write(T data);

        protected StreamWriter CreateFileStream()
        {
            var directoryPath = Path.GetDirectoryName(Directory.GetCurrentDirectory() + _path);
            if (System.IO.Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            _path = Directory.GetCurrentDirectory() + _path;
            if (File.Exists(_path) == true)
            {
                File.Delete(_path);
            }

            var src = File.CreateText(_path);
            return src;
        }
        protected void WriteAllBytes(byte[] bytes)
        {
            var directoryPath = Path.GetDirectoryName(Directory.GetCurrentDirectory() + _path);
            if (System.IO.Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            _path = Directory.GetCurrentDirectory() + _path;
            if (File.Exists(_path) == true)
            {
                File.Delete(_path);
            }
            File.WriteAllBytes(_path, bytes);
        }
    }
}