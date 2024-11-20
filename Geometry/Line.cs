using System.Text.Json.Serialization;

namespace LocateAlongLine.Geometry;

/// <summary>
/// The line.
/// </summary>
[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
public sealed class Line
{
	/// <summary>
	/// Gets or sets the paths.
	/// </summary>
	public Path[] Paths { get; set; } = [];
}