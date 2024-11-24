using TidyHPC.LiteJson;

namespace WebApplication;
/// <summary>
/// WebView环境配置
/// </summary>
public class WebViewEnvironmentConfig
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public WebViewEnvironmentConfig()
    {
        Target = Json.NewObject();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="target"></param>
    public WebViewEnvironmentConfig(Json target)
    {
        Target = target;
    }

    /// <summary>
    /// 用户数据目录
    /// </summary>
    public string UserDataDirectory
    {
        get => Target.Read("UserDataDirectory", Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WebApplications",
                    "DefaultUserData"));
        set => Target.Set("UserDataDirectory", value);
    }
}
