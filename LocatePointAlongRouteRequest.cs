using LocateAlongLine.Geometry;
using System.Text.Json.Serialization;

namespace LocateAlongLine;

/// <summary>
/// The locate point along route request.
/// </summary>
[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
public sealed class LocatePointAlongRouteRequest
{
	/// <summary>
	/// Gets or sets the route.
	/// </summary>
	public Line Route { get; set; } = new();

	/// <summary>
	/// Gets or sets the measure.
	/// </summary>
	public double Measure { get; set; }
}