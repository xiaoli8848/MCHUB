using MCHUB.Utility;

namespace MCHUB;

/// <summary>
/// 启动错误类。描述启动游戏时发生的错误。
/// </summary>
public class LaunchException : ApplicationException
{
    public Minecraft GameInstance;
    public LaunchException(Minecraft gameInstance, string message) : base(message)
    {
        GameInstance = gameInstance;
    }
}

/// <summary>
/// 启动参数错误类。描述启动游戏时发现的参数错误。
/// </summary>
public class LaunchArgumentException : LaunchException
{
    public string ParameterName { get; init; }
    public string ParameterValue { get; init; }
    public LaunchArgumentException(Minecraft gameInstance, string parameterName, string parameterValue) : base(gameInstance, "启动参数错误。名为" + parameterName + "的参数被意外赋值为" + parameterValue + "。")
    {
        this.ParameterName = parameterName;
        this.ParameterValue = parameterValue;
    }

    public LaunchArgumentException(Minecraft gameInstance, string parameterName) : base(gameInstance, "启动参数错误。名为" + parameterName + "的参数被意外赋值。")
    {
        this.ParameterName = parameterName;
    }
}

/// <summary>
/// 启动参数为空错误类。描述启动游戏时发现的参数为空错误。
/// </summary>
public class LaunchArgumentNullException : LaunchArgumentException
{
    public LaunchArgumentNullException(Minecraft gameInstance, string parameterName) : base(gameInstance, parameterName, "Null")
    {
    }
}

/// <summary>
/// 用户错误类。描述执行用户操作时发生的错误。
/// </summary>
public class UserException : ApplicationException
{
    public User User;
    public UserException(User user, string message) : base(message)
    {
        User = user;
    }
}
