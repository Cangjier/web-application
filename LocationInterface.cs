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
    /// Constructor
    /// </summary>
    public LocationInterface()
    {
        Target = Json.NewObject();
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
    public Json X
    {
        get => Target.Get("x", Json.Null);
        set => Target.Set("x", value);
    }

    /// <summary>
    /// Y
    /// </summary>
    public Json Y
    {
        get => Target.Get("y", Json.Null);
        set => Target.Set("y", value);
    }

    /// <summary>
    /// Width
    /// </summary>
    public Json Width
    {
        get => Target.Get("width", Json.Null);
        set => Target.Set("width", value);
    }

    /// <summary>
    /// Height
    /// </summary>
    public Json Height
    {
        get => Target.Get("height", Json.Null);
        set => Target.Set("height", value);
    }
}
