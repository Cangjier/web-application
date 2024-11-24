namespace WebApplication;
/// <summary>
/// 窗口模式
/// </summary>
public enum WindowMode
{
    /// <summary>
    /// 正常模式
    /// </summary>
    Normal,
    /// <summary>
    /// 单例，但窗口触发关闭时不会关闭窗口，而是隐藏窗口
    /// </summary>
    Singleton,
}
