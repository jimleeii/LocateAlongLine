using LocateAlongLine.Extensions;
using LocateAlongLine.Geometry;

namespace LocateAlongLine;

/// <summary>
/// The line locate.
/// </summary>
/// <param name="loggerFactory">The logger factory.</param>
internal class LinearReferencing(ILoggerFactory loggerFactory) : IDisposable
{
	/// <summary>
	/// Kilometers.
	/// </summary>
	private const double Km = 1000.0;

	/// <summary>
	/// Radius of the Earth in kilometers.
	/// </summary>
	private const double EarthRadiusKm = 6371.0;

	// The logger.
	private ILogger<LinearReferencing>? logger = loggerFactory.CreateLogger<LinearReferencing>();

	// Flag to indicate if the resource has already been disposed
	private bool disposed = false;

	/// <summary>
	/// Locates point along route.
	/// </summary>
	/// <param name="line">The line.</param>
	/// <param name="measure">The measure.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A ValueTask of type Point</returns>
	public async ValueTask<Point?> LocatePointAlongRoute(Line line,
		double measure,
		CancellationToken cancellationToken = default)
	{
		Walker walker = new();
		bool hasM = line.HasM();
		foreach (Point[] points in line.Paths.Select(static p => p.Points))
		{
			if (cancellationToken.IsCancellationRequested)
				cancellationToken.ThrowIfCancellationRequested();

			for (int i = 1; i < points.Length; i++)
			{
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				Point start = points[i - 1];
				Point end = points[i];

				walker.PreDistance = walker.Distance;
				double length = await GeodesicLength(start.Y, start.X, end.Y, end.X, "meters", cancellationToken);
				if (hasM)
					walker.Distance = end.M!.Value;
				else
					walker.Distance += length;

				logger!.LogInformation("Length:: {Length}", walker.Distance);

				if (measure <= walker.Distance)
				{
					double distance = Equals(walker.PreDistance, 0) ? measure : measure - walker.PreDistance;
					double bearing = await CalculateBearingTo(start.Y, start.X, end.Y, end.X, cancellationToken);
					var (lat, lon) = await CalculateDestination(start.Y, start.X, bearing, distance, cancellationToken);
					return new Point() { X = lon, Y = lat };
				}
			}
		}
		return default;
	}

	/// <summary>
	/// Dispose pattern callable by consumers.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose pattern.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			logger = null;
		}

		// Free unmanaged resources (if any) here

		disposed = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	~LinearReferencing()
	{
		Dispose(false);
	}

	/// <summary>
	/// Calculate bearing converts to.
	/// </summary>
	/// <param name="lat1">The latitude1.</param>
	/// <param name="lon1">The longitude1.</param>
	/// <param name="lat2">The latitude2.</param>
	/// <param name="lon2">The longitude2.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A double</returns>
	private static ValueTask<double> CalculateBearingTo(double lat1, double lon1, double lat2, double lon2, CancellationToken cancellationToken = default)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			// Convert degrees to radians
			double lat1Rad = DegreesToRadians(lat1);
			double lon1Rad = DegreesToRadians(lon1);
			double lat2Rad = DegreesToRadians(lat2);
			double lon2Rad = DegreesToRadians(lon2);

			// Calculate the difference in longitudes
			double dLon = lon2Rad - lon1Rad;

			// Calculate the bearing
			double y = Math.Sin(dLon) * Math.Cos(lat2Rad);
			double x = (Math.Cos(lat1Rad) * Math.Sin(lat2Rad)) -
					   (Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon));

			double bearingRad = Math.Atan2(y, x);

			// Convert radians to degrees
			double bearingDeg = RadiansToDegrees(bearingRad);

			// Normalize the bearing to 0-360 degrees
			return ValueTask.FromResult((bearingDeg + 360) % 360);
		}
		cancellationToken.ThrowIfCancellationRequested();
		return ValueTask.FromCanceled<double>(cancellationToken);
	}

	/// <summary>
	/// Calculate the destination.
	/// </summary>
	/// <param name="lat">The latitude.</param>
	/// <param name="lon">The longitude.</param>
	/// <param name="bearing">The bearing in degree.</param>
	/// <param name="distance">The distance in meter.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A (double, double)</returns>
	private static ValueTask<(double lat, double lon)> CalculateDestination(double lat, double lon, double bearing, double distance, CancellationToken cancellationToken = default)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			// Distance provided is in meters convert to km
			distance /= Km;
			// Convert distance to angular distance in radians based on the earth's radius
			distance /= EarthRadiusKm;
			// Convert degrees to radians
			double latRad = DegreesToRadians(lat);
			double lonRad = DegreesToRadians(lon);
			double bearingRad = DegreesToRadians(bearing);

			// Calculate the destination point
			double lat2Rad = Math.Asin((Math.Sin(latRad) * Math.Cos(distance)) +
									   (Math.Cos(latRad) * Math.Sin(distance) * Math.Cos(bearingRad)));

			double lon2Rad = lonRad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(distance) * Math.Cos(latRad),
												 Math.Cos(distance) - (Math.Sin(latRad) * Math.Sin(lat2Rad)));

			// Normalize to -180...+180
			lon2Rad = ((lon2Rad + (3 * Math.PI)) % (2 * Math.PI)) - Math.PI;

			// Convert radians back to degrees
			double lat2 = RadiansToDegrees(lat2Rad);
			double lon2 = RadiansToDegrees(lon2Rad);

			return ValueTask.FromResult((lat: lat2, lon: lon2));
		}
		cancellationToken.ThrowIfCancellationRequested();
		return ValueTask.FromCanceled<(double lat, double lon)>(cancellationToken);
	}

	/// <summary>
	/// Geodesics the length.
	/// </summary>
	/// <param name="lat1">The lat1.</param>
	/// <param name="lon1">The lon1.</param>
	/// <param name="lat2">The lat2.</param>
	/// <param name="lon2">The lon2.</param>
	/// <param name="unit">The unit.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A double</returns>
	private static ValueTask<double> GeodesicLength(double lat1, double lon1, double lat2, double lon2, string unit = "kilometers", CancellationToken cancellationToken = default)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			// Convert degrees to radians
			double lat1Rad = DegreesToRadians(lat1);
			double lon1Rad = DegreesToRadians(lon1);
			double lat2Rad = DegreesToRadians(lat2);
			double lon2Rad = DegreesToRadians(lon2);

			// Haversine formula
			double dlat = lat2Rad - lat1Rad;
			double dlon = lon2Rad - lon1Rad;

			double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) +
					   (Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Sin(dlon / 2) * Math.Sin(dlon / 2));

			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			switch (unit)
			{
				case "kilometers":
					// Distance in kilometers
					return ValueTask.FromResult(EarthRadiusKm * c);

				case "meters":
					// Distance in meters
					return ValueTask.FromResult(EarthRadiusKm * c * Km);

				default:
					break;
			}
			return default;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return ValueTask.FromCanceled<double>(cancellationToken);
	}

	/// <summary>
	/// Converts to the degrees.
	/// </summary>
	/// <param name="radians">The radians.</param>
	/// <returns>A double</returns>
	private static double RadiansToDegrees(double radians)
	{
		return radians * 180.0 / Math.PI;
	}

	/// <summary>
	/// Converts to the radians.
	/// </summary>
	/// <param name="degrees">The degrees.</param>
	/// <returns>A double</returns>
	private static double DegreesToRadians(double degrees)
	{
		return degrees * Math.PI / 180.0;
	}
}