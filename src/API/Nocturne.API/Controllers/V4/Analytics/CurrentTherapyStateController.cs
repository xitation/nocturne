using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Remote.Attributes;
using Nocturne.Core.Contracts.Glucose;
using Nocturne.Core.Models;

namespace Nocturne.API.Controllers.V4.Analytics;

/// <summary>
/// Returns small "right now" therapy values consumed by the dashboard Halo Dial:
/// the active pump operational mode and the current insulin sensitivity expressed
/// as a percentage of the profile baseline.
/// </summary>
[ApiController]
[Tags("Current Therapy State")]
[Route("api/v4/current-therapy-state")]
[Authorize]
public class CurrentTherapyStateController : ControllerBase
{
    private readonly IStateSpanService _stateSpanService;

    public CurrentTherapyStateController(IStateSpanService stateSpanService)
    {
        _stateSpanService = stateSpanService;
    }

    /// <summary>
    /// Get the current pump mode and (later) sensitivity for the active tenant.
    /// </summary>
    [HttpGet]
    [RemoteQuery]
    [ProducesResponseType(typeof(CurrentTherapyStateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentTherapyStateResponse>> GetCurrentTherapyState(
        CancellationToken cancellationToken = default)
    {
        var pumpMode = await _stateSpanService.GetCurrentPumpModeAsync(cancellationToken);
        return Ok(new CurrentTherapyStateResponse
        {
            CurrentPumpMode = pumpMode,
        });
    }
}

/// <summary>
/// Snapshot of "right now" therapy state for the Halo Dial.
/// </summary>
public class CurrentTherapyStateResponse
{
    /// <summary>
    /// The active pump operational mode, derived from the most recently started
    /// open-ended <see cref="StateSpanCategory.PumpMode"/> span. Null when no
    /// pump-mode span is currently open.
    /// </summary>
    public PumpModeState? CurrentPumpMode { get; set; }
}
