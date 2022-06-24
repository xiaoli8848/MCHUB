﻿using ModuleLauncher.Re.Authenticators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCHUB
{
    public class LauncherDataHelper
    {
        public static List<FileInfo> TempFiles = new();
        /// <summary>
        /// 创建并提供一个缓存文件信息。
        /// </summary>
        /// <param name="name">文件标识，作为文件名，含后缀。</param>
        /// <returns>提供一个文件信息，指示创建的缓存文件。</returns>
        public static FileInfo CreateTempFile(string name)
        {
            var fullName = "ms-appdata:///temp/" + name;
            _ = File.Create(fullName);
            var result = new FileInfo(fullName);
            TempFiles.Add(result);
            return result;
        }
    }

    public abstract class User
    {
        public virtual string Name { get; }
        public virtual AuthenticatorBase Authenticator { get; }
        public abstract FileInfo GetPersonPicture();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class OfflineUser : User
    {
        public override string Name { get; }
        [JsonIgnore]
        public override OfflineAuthenticator Authenticator { get; }
        private string PersonPictureCode { get; }

        public OfflineUser(string name, FileInfo persionPicture = null)
        {
            Name = name;
            Authenticator = new OfflineAuthenticator(Name);
            if(persionPicture != null)
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

    [JsonObject(MemberSerialization.OptIn)]
    [Obsolete]
    public class MojangUser : User
    {
        public override string Name { get; }
        public override MojangAuthenticator Authenticator { get; }

        public MojangUser(string name, string password)
        {
            Name = name;
            Authenticator = new MojangAuthenticator(name, password);
        }

        public override FileInfo GetPersonPicture()
        {
            return null;
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
            string base64Str = string.Empty;
            try
            {
                using (FileStream filestream = new FileStream(filePath, FileMode.Open))
                {
                    byte[] bt = new byte[filestream.Length];

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
}
