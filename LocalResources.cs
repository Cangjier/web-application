using TidyHPC.Extensions;
using TidyHPC.Loggers;

namespace WebApplication;
internal class LocalResources
{
    /// <summary>
    /// 读取嵌入资源文件
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static string? LoadStringFromResource(string resourceName)
    {
        var resources = typeof(LocalFavicon).Assembly.GetManifestResourceNames();
        var matchedResource = resources.FirstOrDefault(r => r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        if (matchedResource == null)
        {
            resources.Foreach(r => Logger.Debug($"Available resource: {r}"));
            Logger.Error($"Resource not found: {resourceName}");
            return null;
        }
        using var stream = typeof(LocalFavicon).Assembly.GetManifestResourceStream(matchedResource);
        if (stream == null)
        {
            Logger.Error($"Failed to load resource stream: {resourceName}");
            return null;
        }
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 读取嵌入资源文件流
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static Stream? LoadStreamFromResource(string resourceName)
    {
        var resources = typeof(LocalFavicon).Assembly.GetManifestResourceNames();
        var matchedResource = resources.FirstOrDefault(r => r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        if (matchedResource == null)
        {
            resources.Foreach(r => Logger.Debug($"Available resource: {r}"));
            Logger.Error($"Resource not found: {resourceName}");
            return null;
        }
        var stream = typeof(LocalFavicon).Assembly.GetManifestResourceStream(matchedResource);
        if (stream == null)
        {
            Logger.Error($"Failed to load resource stream: {resourceName}");
            return null;
        }
        return stream;
    }
}
