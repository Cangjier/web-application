using Cangjie.TypeSharp;
using Svg;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using TidyHPC.Extensions;
using TidyHPC.Loggers;

namespace WebApplication;

/// <summary>
/// 本地图标缓存，用于设置Form的Icon属性
/// </summary>
public class LocalFavicon
{
    private HttpClient HttpClient { get; } = new();

    private ConcurrentDictionary<string, Icon> Cache { get; } = new();

    /// <summary>
    /// 获取或下载图标
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<Icon?> GetOrDownload(string url)
    {
        if (Cache.TryGetValue(url, out var icon))
        {
            return icon;
        }
        else
        {
            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
                using var responseStream = await response.Content.ReadAsStreamAsync();
                Stream decodedStream = responseStream;

                if (encoding == "gzip")
                {
                    decodedStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }
                else if (encoding == "deflate")
                {
                    decodedStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                }
                else if (encoding == "br")
                {
                    decodedStream = new BrotliStream(responseStream, CompressionMode.Decompress);
                }
                else if (!string.IsNullOrEmpty(encoding))
                {
                    throw new NotSupportedException($"Unsupported Content-Encoding: {encoding}");
                }

                using var memoryStream = new MemoryStream();
                await decodedStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                using var stream = new MemoryStream(bytes);
                if (url.EndsWith(".svg"))
                {
                    Logger.Info($"svg:{Util.UTF8.GetString(bytes)}");
                    var svg = SvgDocument.Open<SvgDocument>(stream);
                    icon = Icon.FromHandle(svg.Draw(32, 32).GetHicon());
                }
                else
                {
                    icon = Icon.FromHandle(new Bitmap(stream).GetHicon()); // 设置窗口图标
                }

                Cache.TryAdd(url, icon);
                return icon;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }

    /// <summary>
    /// 从本地添加图标
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public Icon? LoadFromFilePath(string fileName)
    {
        var fullPath = Path.GetFullPath(fileName, Path.GetDirectoryName(Environment.ProcessPath) ?? "");
        if (Cache.TryGetValue(fullPath, out var icon))
        {
            return icon;
        }
        else
        {
            try
            {
                using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                if (fileName.EndsWith(".svg"))
                {
                    var svg = SvgDocument.Open<SvgDocument>(stream);
                    icon = Icon.FromHandle(svg.Draw(32, 32).GetHicon());
                }
                else
                {
                    icon = new Icon(fullPath);
                }
                Cache.TryAdd(fullPath, icon);
                return icon;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }

    /// <summary>
    /// 从嵌入资源添加图标
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public Icon? LoadFromResource(string resourceName)
    {
        if (Cache.TryGetValue(resourceName, out var icon))
        {
            return icon;
        }
        else
        {
            try
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
                    throw new FileNotFoundException($"Resource not found: {resourceName}");
                }
                if (resourceName.EndsWith(".svg"))
                {
                    var svg = SvgDocument.Open<SvgDocument>(stream);
                    icon = Icon.FromHandle(svg.Draw(32, 32).GetHicon());
                }
                else
                {
                    icon = new Icon(stream);
                }
                Cache.TryAdd(resourceName, icon);
                return icon;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}
