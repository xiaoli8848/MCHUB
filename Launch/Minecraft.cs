using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuleLauncher.Re.Launcher;
using ModuleLauncher.Re.Locators;
using ModuleLauncher.Re.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LocalVersion = ModuleLauncher.Re.Models.Locators.LocalVersion;
using System.Windows.Input;

namespace MCHUB
{
    /// <summary>
    /// Minecraft游戏类。描述一个Minecraft实例，包括其路径、版本信息和操作命令实例等。
    /// </summary>
    public sealed class Minecraft
    {
        public DirectoryInfo Path { get; private set; } //版本路径
        public DirectoryInfo Root { get; private set; } //.minecraft根路径
        public LocalVersion Version { get; private set; }   //版本信息
        public LocalityLocator Locator { get; private set; }    //minecraft locator
        public string VersionID { get; private set; }
        public ICommand LaunchCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        /// <summary>
        /// 获取一个游戏根路径下的所有Miencraft实例。
        /// </summary>
        /// <param name="path">指定游戏根路径，一般是名为".minecraft"文件夹。</param>
        /// <returns>返回一个列表实例。</returns>
        public static IList<Minecraft> GetMinecrafts(string path)
        {
            List<Minecraft> list = new List<Minecraft>();
            LocalityLocator locator = new LocalityLocator(path);
            foreach (var item in locator.GetLocalVersions())
            {
                list.Add(new Minecraft(item.Version, item.Root, item, locator));
            }
            return list;
        }
        private Minecraft(DirectoryInfo path, DirectoryInfo root, LocalVersion version, LocalityLocator locator) : base()
        {
            this.Path = path;
            this.Root = root;
            this.Version = version;
            this.Locator = locator;
            this.VersionID = new DirectoryInfo(this.Version.Version.FullName).Name;
            StandardUICommand launchCommand = new StandardUICommand(StandardUICommandKind.Play);
            launchCommand.ExecuteRequested += LaunchCommand_ExecuteRequestedAsync;
            this.LaunchCommand = launchCommand;
            StandardUICommand removeCommand = new StandardUICommand(StandardUICommandKind.Delete);
            removeCommand.ExecuteRequested += RemoveCommand_ExecuteRequested;
            this.RemoveCommand = removeCommand;
        }

        private void RemoveCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            this.Root.Delete();
        }

        private async void LaunchCommand_ExecuteRequestedAsync(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            await launchAsync(args.Parameter as LaunchArgument);
        }

        /// <summary>
        /// 启动Minecraft实例（异步）。
        /// </summary>
        /// <param name="authenticator">提供一个身份验证令牌实例。</param>
        /// <param name="javaEnvironment">提供一个Java环境实例。</param>
        /// <param name="fullScreen">指定游戏窗口是否默认全屏显示，默认否（False）。</param>
        /// <param name="minMemory">指定最小内存，默认为空（Null）。</param>
        /// <param name="maxMemory">指定最大内存，默认为空（Null）。</param>
        /// <returns></returns>
        public async Task<Process> launchAsync(
            ModuleLauncher.Re.Authenticators.AuthenticatorBase authenticator, 
            JavaEnvironment javaEnvironment,
            bool fullScreen=false,
            int? minMemory=null,
            int? maxMemory=null
            )
        {
            var launcher = new Launcher(new ModuleLauncher.Re.Locators.Concretes.MinecraftLocator(Locator))
            {
                Authentication = await authenticator.Authenticate(),
                Java = javaEnvironment.javaw.FullName,
                Fullscreen = fullScreen,
                MinimumMemorySize = minMemory,
            };
            if(maxMemory != null)
            {
                launcher.MaximumMemorySize = (int)maxMemory;
            }
            return await launcher.Launch(VersionID);
        }

        /// <summary>
        /// 启动Minecraft实例（异步）。
        /// </summary>
        /// <param name="argument">指定启动参数。</param>
        /// <returns></returns>
        public async Task<Process> launchAsync(LaunchArgument argument)
        {
            return await launchAsync(argument.Authenticator, argument.JavaEnvironment, argument.Fullscreen, argument.MinMemory, argument.MaxMemory);
        }
    }

    public sealed class LaunchArgument
    {
        public ModuleLauncher.Re.Authenticators.AuthenticatorBase Authenticator { get; set; }
        public JavaEnvironment JavaEnvironment { get; set; }
        public bool Fullscreen { get; set; }
        public int? MinMemory { get; set; }
        public int? MaxMemory { get; set; }
    }

}
