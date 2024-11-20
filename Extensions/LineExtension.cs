using LocateAlongLine.Geometry;

namespace LocateAlongLine.Extensions;

/// <summary>
/// The line extension.
/// </summary>
public static class LineExtension
{
	/// <summary>
	/// Has M.
	/// </summary>
	/// <param name="line">The line.</param>
	/// <returns>A bool</returns>
	public static bool HasM(this Line line) => Array.TrueForAll(line.Paths, static seg => Array.TrueForAll(seg.Points, static p => p!.M >= 0));
}