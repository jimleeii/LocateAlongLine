using LocateAlongLine.Geometry;
using Microsoft.AspNetCore.Mvc;

namespace LocateAlongLine.Controllers;

/// <summary>
/// The linear referencing controller.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LinearReferencingController"/> class.
/// </remarks>
/// <param name="loggerFactory">The logger factory.</param>
[Route("api/[controller]")]
[ApiController]
public class LinearReferencingController(ILoggerFactory loggerFactory) : ControllerBase
{
	// The logger.
	private readonly ILogger<LinearReferencingController> logger = loggerFactory.CreateLogger<LinearReferencingController>();

	/// <summary>
	/// Locate point along route.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task of type IActionResult</returns>
	[HttpPost]
	public async Task<IActionResult> LocatePointAlongRoute([FromBody] LocatePointAlongRouteRequest request, CancellationToken cancellationToken)
	{
		logger.LogInformation("Locate point along route.");
		using var linearReferencing = new LinearReferencing(loggerFactory);
		Point? postion = await linearReferencing.LocatePointAlongRoute(request.Route, request.Measure, cancellationToken: cancellationToken);
		return Ok(postion);
	}
}