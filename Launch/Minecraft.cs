using ModuleLauncher.Re.Launcher;
using ModuleLauncher.Re.Locators;
using ModuleLauncher.Re.Models.Locators.Minecraft;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Windows.Input;

namespace MCHUB;

/// <summary>
/// Minecraft游戏类。描述一个Minecraft实例，包括其路径、版本信息和操作命令实例等。
/// </summary>
public sealed class Minecraft : ModuleLauncher.Re.Models.Locators.Minecraft.Minecraft
{
    // 版本路径
    public DirectoryInfo Path => Locality.Version;

    // .minecraft根路径
    public DirectoryInfo Root => Locality.Root;

    // 版本ID
    public string VersionID => Raw.Id;
    public string VersionType => Raw.Type;
    public ICommand LaunchCommand { get; init; }
    public ICommand RemoveCommand { get; init; }

    /// <summary>
    /// 获取一个游戏根路径下的所有Minecraft实例。
    /// </summary>
    /// <param name="path">指定游戏根路径，一般是名为".minecraft"文件夹。</param>
    /// <returns>返回一个列表实例。</returns>
    public static IList<Minecraft> GetMinecrafts(string path)
    {
        var list = new List<Minecraft>();
        var locator = new LocalityLocator(path);
        foreach (var item in locator.GetLocalVersions())
        {
            var json = File.ReadAllText(item.Json.FullName);
            list.Add(new Minecraft() { Locality = item, Raw = JToken.Parse(json).ToObject<MinecraftJson>() });
        }

        return list;
    }

    private Minecraft() : base()
    {
        var launchCommand = new StandardUICommand(StandardUICommandKind.Play);
        launchCommand.ExecuteRequested += LaunchCommand_ExecuteRequestedAsync;
        LaunchCommand = launchCommand;
        var removeCommand = new StandardUICommand(StandardUICommandKind.Delete);
        removeCommand.ExecuteRequested += RemoveCommand_ExecuteRequested;
        RemoveCommand = removeCommand;
    }

    private void RemoveCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        Root.Delete();
    }

    private async void LaunchCommand_ExecuteRequestedAsync(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await LaunchAsync(args.Parameter as LaunchArgument);
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
    public async Task<Process> LaunchAsync(
        ModuleLauncher.Re.Authenticators.AuthenticatorBase authenticator,
        JavaEnvironment javaEnvironment,
        bool fullScreen = false,
        int? minMemory = null,
        int? maxMemory = null
    )
    {
        var launcher =
            new Launcher(new ModuleLauncher.Re.Locators.Concretes.MinecraftLocator(new LocalityLocator(Root.FullName)))
            {
                Authentication = await authenticator.Authenticate(),
                Java = javaEnvironment.javaw.FullName,
                Fullscreen = fullScreen,
                MinimumMemorySize = minMemory
            };
        if (maxMemory != null) launcher.MaximumMemorySize = (int)maxMemory;
        return await launcher.Launch(VersionID);
    }

    /// <summary>
    /// 启动Minecraft实例（异步）。
    /// </summary>
    /// <param name="argument">指定启动参数。</param>
    /// <returns></returns>
    public async Task<Process> LaunchAsync(LaunchArgument argument)
    {
        return await LaunchAsync(argument.Authenticator, argument.JavaEnvironment, argument.Fullscreen,
            argument.MinMemory, argument.MaxMemory);
    }
}

public sealed class MinecraftRoot
{
    public IList<Minecraft> Minecrafts { get; init; }
    public DirectoryInfo DirectoryInfo { get; init; }
    public PerVersionType PerVersionType { get; init; }
    public DirectoryInfo Assets { get; init; }
    public DirectoryInfo Libraries { get; init; }
    public DirectoryInfo Versions { get; init; }
    public string Path => DirectoryInfo.FullName;

    public static MinecraftRoot GetRoot(string path)
    {
        var minecrafts = Minecraft.GetMinecrafts(path);
        var directoryInfo = new DirectoryInfo(path);
        return new MinecraftRoot
        {
            Minecrafts = minecrafts,
            DirectoryInfo = directoryInfo,
            PerVersionType = GetPerVersionType(path),
            Assets = minecrafts[0].Locality.Assets,
            Libraries = minecrafts[0].Locality.Libraries,
            Versions = minecrafts[0].Locality.Versions
        };
    }

    private static PerVersionType GetPerVersionType(string path)
    {
        var directory = new DirectoryInfo(path);
        foreach (var item in directory.GetDirectories())
            if (item.Name.Equals("versions", StringComparison.OrdinalIgnoreCase))
                return PerVersionType.PerVersion;
        return PerVersionType.SingleVersion;
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

public enum PerVersionType
{
    PerVersion,
    SingleVersion
}