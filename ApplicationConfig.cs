using TidyHPC.LiteJson;

namespace WebApplication;

/// <summary>
/// 应用配置
/// </summary>
public class ApplicationConfig
{
    /// <summary>
    /// 路由接口
    /// </summary>
    public class RouterInterface
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="target"></param>
        public RouterInterface(Json target)
        {
            Target = target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator Json(RouterInterface target)
        {
            return target.Target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator RouterInterface(Json target)
        {
            return new RouterInterface(target);
        }

        /// <summary>
        /// 封装对象
        /// </summary>
        public Json Target { get; }

        /// <summary>
        /// 任务栏菜单路由地址
        /// </summary>
        public string TaskBarMenu
        {
            get => Target.Read("taskbar-menu", "/web-application-taskbar-menu");
            set => Target.Set("taskbar-menu", value);
        }

        /// <summary>
        /// 主页路由地址
        /// </summary>
        public string Home
        {
            get => Target.Read("home", "/home");
            set => Target.Set("home", value);
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ApplicationConfig()
    {
        Target = Json.TryLoad(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "app.json"), Json.NewObject);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="target"></param>
    public ApplicationConfig(Json target)
    {
        Target = target;
    }

    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target { get; }

    /// <summary>
    /// 路由配置
    /// </summary>
    public RouterInterface Router => Target.GetOrCreateObject("router");

    /// <summary>
    /// 转换成字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Target.ToString();
    }
}
