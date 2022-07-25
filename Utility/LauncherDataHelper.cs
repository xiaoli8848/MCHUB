using ModuleLauncher.Re.Authenticators;
using ModuleLauncher.Re.Models.Authenticators;
using Newtonsoft.Json;
using System.Timers;

namespace MCHUB.Utility;

public static class LauncherDataHelper
{
    public static List<FileInfo> TempFiles = new();
    public static List<User> Users = new();

    public const double USER_FRESH_TIME = 60000;

    public const string MICROSOFT_OAUTH_URL =
        @"https://login.live.com/oauth20_authorize.srf?client_id=00000000402b5328&response_type=code &scope=service%3A%3Auser.auth.xboxlive.com%3A%3AMBI_SSL&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf";

    /// <summary>
    /// 创建并提供一个缓存文件信息。
    /// </summary>
    /// <param name="name">文件标识，作为文件名，含后缀。</param>
    /// <returns>提供一个文件信息，指示创建的缓存文件。</returns>
    public static FileInfo CreateTempFile(string name)
    {
        var temp = Path.GetTempFileName();
        var result = new FileInfo(temp).Directory.ToString() + Path.DirectorySeparatorChar + name;
        File.Move(temp, result);
        var file = new FileInfo(result);
        TempFiles.Add(file);
        return file;
    }

    public static void RemoveTempFiles()
    {
        foreach (var file in TempFiles) File.Delete(file.FullName);
    }

    public static void Init()
    {
        var timer_userRefresh = new Timer
        {
            Enabled = true,
            Interval = USER_FRESH_TIME
        };
        timer_userRefresh.Start();
        timer_userRefresh.Elapsed += RefreshUsers;
    }

    private static async void RefreshUsers(object source, ElapsedEventArgs e)
    {
        await RefreshUsers();
    }

    public static async Task RefreshUsers()
    {
        List<Task<AuthenticateResult>> tasks = new();
        foreach (MicrosoftUser item in Users.FindAll(user => user is MicrosoftUser))
            await item.Authenticator.Authenticate();
    }
}

[JsonObject(MemberSerialization.OptIn)]
public abstract class User
{
    public virtual string Name { get; }
    public virtual AuthenticatorBase Authenticator { get; }
    public abstract FileInfo GetPersonPicture();
}

public class OfflineUser : User
{
    public override string Name { get; }
    [JsonIgnore] public override OfflineAuthenticator Authenticator { get; }
    private string PersonPictureCode { get; }

    public OfflineUser(string name, FileInfo persionPicture = null)
    {
        Name = name;
        Authenticator = new OfflineAuthenticator(Name);
        if (persionPicture != null)
            PersonPictureCode = FileDataHelper.FileToBase64Str(persionPicture.FullName);
    }

    public override FileInfo GetPersonPicture()
    {
        if (PersonPictureCode != null)
        {
            var info = LauncherDataHelper.CreateTempFile(Name + ".png");
            FileDataHelper.Base64ToOriFile(PersonPictureCode, info.FullName);
            return info;
        }
        else
        {
            return null;
        }
    }
}

public class MicrosoftUser : User
{
    public override string Name { get; }
    public override MicrosoftAuthenticator Authenticator { get; }
    public bool IsAvailabel = false;
    private AuthenticateResult AuthenticateResult { get; set; }

    public MicrosoftUser(string code, bool loginNow = true)
    {
        if (loginNow == true)
        {
            try
            {
                Authenticator = new MicrosoftAuthenticator(code);
                var authentication = Authenticator.Authenticate();
                authentication.Wait();
                Name = authentication.Result.Name;
                IsAvailabel = true;
            }
            catch (Exception)
            {
                throw new UserException(this, "试图登录用户时出错。");
            }
        }
        else
        {
            Name = null;
            Authenticator = new MicrosoftAuthenticator(code);
        }
    }

    public override FileInfo GetPersonPicture()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取Microsoft OAuth时的Code，用于<see cref="MicrosoftAuthenticator"/>的初始化。
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetCodeAsync()
    {
        string code = null;
        var webView = new WebView2() { Source = new Uri(LauncherDataHelper.MICROSOFT_OAUTH_URL) };
        var grid = new Grid();
        grid.Children.Add(webView);
        var browserWindow = new Window() { Content = grid };
        webView.NavigationStarting += (_, e) =>
        {
            var uri = e.Uri.ToString();
            var codeptr = uri.IndexOf("code=");
            if (codeptr == -1)
            {
                return;
            }
            else
            {
                code = uri.Substring(codeptr + 5, uri.IndexOf("&lc=") - codeptr - 5);
                browserWindow.Close();
            }
        };
        browserWindow.Activate();
        return await Task.Run(() =>
        {
            while (true)
                if (code == null)
                    System.Threading.Thread.Sleep(500);
                else
                    break;
            return code;
        });
    }
}

public static class FileDataHelper
{
    /// <summary>
    /// 文件转为base64编码
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string FileToBase64Str(string filePath)
    {
        var base64Str = string.Empty;
        try
        {
            using (var filestream = new FileStream(filePath, FileMode.Open))
            {
                var bt = new byte[filestream.Length];

                //调用read读取方法
                filestream.Read(bt, 0, bt.Length);
                base64Str = Convert.ToBase64String(bt);
                filestream.Close();
            }

            return base64Str;
        }
        catch (Exception)
        {
            return base64Str;
        }
    }

    /// <summary>
    /// 文件base64解码
    /// </summary>
    /// <param name="base64Str">文件base64编码</param>
    /// <param name="outPath">生成文件路径</param>
    public static void Base64ToOriFile(string base64Str, string outPath)
    {
        var contents = Convert.FromBase64String(base64Str);
        using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        {
            fs.Write(contents, 0, contents.Length);
            fs.Flush();
        }
    }
}