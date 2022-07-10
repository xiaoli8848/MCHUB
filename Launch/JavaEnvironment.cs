namespace MCHUB;

/// <summary>
/// Java环境类。指定某Java环境下执行jar文件的程序（"java.exe"、"javaw.exe"）。
/// </summary>
public class JavaEnvironment
{
    public FileInfo java { get; init; }
    public FileInfo javaw { get; init; }
    public static JavaEnvironment Current
    {
        get
        {
            var sPath = Environment.GetEnvironmentVariable("JAVA_HOME");
            return sPath != null
                ? new JavaEnvironment()
                {
                    java = new FileInfo(sPath + "\\bin\\java.exe"),
                    javaw = new FileInfo(sPath + "\\bin\\javaw.exe")
                }
                : null;
        }
    }
}
