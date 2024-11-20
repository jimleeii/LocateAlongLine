using System.Text.Json.Serialization;

namespace LocateAlongLine.Geometry;

/// <summary>
/// The path.
/// </summary>
[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
public sealed class Path
{
	/// <summary>
	/// Gets or sets the points.
	/// </summary>
	public Point[] Points { get; set; } = [];
}