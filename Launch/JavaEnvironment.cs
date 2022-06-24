using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MCHUB
{
    /// <summary>
    /// Java环境类。指定某Java环境下执行jar文件的程序（"java.exe"、"javaw.exe"）。
    /// </summary>
    public class JavaEnvironment
    {
        public FileInfo java { get; private set; }
        public FileInfo javaw { get; private set; }
        public static JavaEnvironment Current
        {
            get
            {
                string sPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (sPath != null)
                {
                    return new JavaEnvironment()
                    {
                        java = new FileInfo(sPath + "\\bin\\java.exe"),
                        javaw = new FileInfo(sPath + "\\bin\\javaw.exe")
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
