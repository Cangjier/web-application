using TidyHPC.LiteJson;

namespace WebApplication;

/// <summary>
/// 窗口位置接口
/// </summary>
public class LocationInterface
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="target"></param>
    public LocationInterface(Json target)
    {
        Target = target;
    }

    /// <summary>
    /// Wrapper
    /// </summary>
    public Json Target { get; }

    /// <summary>
    /// Implicit conversion
    /// </summary>
    /// <param name="locationInterface"></param>
    public static implicit operator Json(LocationInterface locationInterface)
    {
        return locationInterface.Target;
    }

    /// <summary>
    /// Implicit conversion
    /// </summary>
    /// <param name="json"></param>
    public static implicit operator LocationInterface(Json json)
    {
        return new LocationInterface(json);
    }

    /// <summary>
    /// X
    /// </summary>
    public Json X => Target.Get("x", Json.Null);

    /// <summary>
    /// Y
    /// </summary>
    public Json Y => Target.Get("y", Json.Null);

    /// <summary>
    /// Width
    /// </summary>
    public Json Width => Target.Get("width", Json.Null);

    /// <summary>
    /// Height
    /// </summary>
    public Json Height => Target.Get("height", Json.Null);
}
