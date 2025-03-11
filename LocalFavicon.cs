using Svg;
using System.Collections.Concurrent;
using System.IO;

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
                var bytes = await HttpClient.GetByteArrayAsync(url);
                using var stream = new MemoryStream(bytes);
                if (url.EndsWith(".svg"))
                {
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
            catch
            {
                return null;
            }
        }
    }
}
