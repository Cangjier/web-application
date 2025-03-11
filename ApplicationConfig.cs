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
    /// 启动接口
    /// </summary>
    public class StartupInterface
    {
        /// <summary>
        /// 启动接口
        /// </summary>
        /// <param name="target"></param>
        public StartupInterface(Json target)
        {
            Target = target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator Json(StartupInterface target)
        {
            return target.Target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator StartupInterface(Json target)
        {
            return new StartupInterface(target);
        }

        /// <summary>
        /// 封装对象
        /// </summary>
        public Json Target { get; }

        /// <summary>
        /// 参数
        /// </summary>
        public string[] Args
        {
            get
            {
                var argsArray = Target.GetOrCreateArray("args");
                return argsArray.Select(x => x.ToString()).ToArray();
            }
            set => Target.Set("args", value);
        }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path
        {
            get => Target.Read("path", string.Empty);
            set => Target.Set("path", value);
        }

        /// <summary>
        /// 工作目录
        /// </summary>
        public string WorkingDirectory
        {
            get => Target.Read("working-directory", string.Empty);
            set => Target.Set("working-directory", value);
        }

        /// <summary>
        /// 不创建窗口
        /// </summary>
        public bool CreateNoWindow
        {
            get => Target.Read("create-no-window",false);
        }
    }

    /// <summary>
    /// 环境变量接口
    /// </summary>
    public class EnvironmentInterface
    {
        /// <summary>
        /// 封装对象
        /// </summary>
        public Json Target { get; }

        /// <summary>
        /// 启动接口
        /// </summary>
        /// <param name="target"></param>
        public EnvironmentInterface(Json target)
        {
            Target = target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator Json(EnvironmentInterface target)
        {
            return target.Target;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="target"></param>
        public static implicit operator EnvironmentInterface(Json target)
        {
            return new (target);
        }

        /// <summary>
        /// 动作
        /// </summary>
        public string Action
        {
            get=> Target.Read("action", string.Empty);
            set => Target.Set("action", value);
        }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type
        {
            get => Target.Read("type", string.Empty);
            set => Target.Set("type", value);
        }

        /// <summary>
        /// 值
        /// </summary>
        public string Value
        {
            get => Target.Read("value", string.Empty);
            set => Target.Set("value", value);
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Key
        {
            get => Target.Read("key", string.Empty);
            set => Target.Set("key", value);
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
    /// 启动项
    /// </summary>
    public StartupInterface[] Startups
    {
        get
        {
            List<StartupInterface> result = [];
            var startups = Target.GetOrCreateArray("startup");
            foreach (var startup in startups)
            {
                result.Add(startup);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 环境变量
    /// </summary>
    public EnvironmentInterface[] Environments
    {
        get
        {
            List<EnvironmentInterface> result = [];
            var environments = Target.GetOrCreateArray("environments");
            foreach (var environment in environments)
            {
                result.Add(environment);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 转换成字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Target.ToString();
    }
}
