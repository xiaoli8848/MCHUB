﻿using ModuleLauncher.Re.Authenticators;
using ModuleLauncher.Re.Models.Authenticators;
using Newtonsoft.Json;
using System.Timers;

namespace MCHUB;

public static class LauncherDataHelper
{
    public static List<FileInfo> TempFiles = new();
    public static List<User> Users = new();
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
        foreach (FileInfo file in TempFiles)
        {
            File.Delete(file.FullName);
        }
    }

    public static void Init()
    {
        var timer_userRefresh = new Timer
        {
            Enabled = true,
            Interval = 60000
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
        foreach (MojangUser item in Users.FindAll(user => user is MojangUser))
        {
            await item.Authenticator.Authenticate();
        }
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
    [JsonIgnore]
    public override OfflineAuthenticator Authenticator { get; }
    private string PersonPictureCode { get; }

    public OfflineUser(string name, FileInfo persionPicture = null)
    {
        this.Name = name;
        this.Authenticator = new OfflineAuthenticator(this.Name);
        if (persionPicture != null)
            this.PersonPictureCode = FileDataHelper.FileToBase64Str(persionPicture.FullName);
    }

    public override FileInfo GetPersonPicture()
    {
        if (this.PersonPictureCode != null)
        {
            FileInfo info = LauncherDataHelper.CreateTempFile(this.Name + ".png");
            FileDataHelper.Base64ToOriFile(this.PersonPictureCode, info.FullName);
            return info;
        }
        else
        {
            return null;
        }
    }
}

[Obsolete]
public class MojangUser : User
{
    public override string Name { get; }
    public override MojangAuthenticator Authenticator { get; }
    public bool IsAvailabel = false;

    public MojangUser(string username, string password, bool loginNow = true)
    {
        if (loginNow == true)
        {
            try
            {
                this.Authenticator = new MojangAuthenticator(username, password);
                Task<AuthenticateResult> authentication = this.Authenticator.Authenticate();
                authentication.Wait();
                this.Name = authentication.Result.Name;
                IsAvailabel = true;
            }
            catch (Exception)
            {
                throw new UserException(this, "试图登录用户时出错。");
            }
        }
        else
        {
            this.Name = null;
            this.Authenticator = new MojangAuthenticator(username, password);
        }
    }

    public override FileInfo GetPersonPicture()
    {
        throw new NotImplementedException();
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
