using System.IO;
using System.Text;

namespace Common.Utility
{
    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
