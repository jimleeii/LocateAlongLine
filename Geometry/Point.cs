using System.Text.Json.Serialization;

namespace LocateAlongLine.Geometry;

/// <summary>
/// The point.
/// </summary>
[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
public sealed class Point
{
    /// <summary>
    /// Gets or sets the X.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the M.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? M { get; set; } = default;
}