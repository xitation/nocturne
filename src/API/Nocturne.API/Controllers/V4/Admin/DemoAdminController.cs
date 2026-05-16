using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nocturne.Core.Contracts.Multitenancy;
using Nocturne.Infrastructure.Data;
using Nocturne.Infrastructure.Data.Entities;

namespace Nocturne.API.Controllers.V4.Admin;

/// <summary>
/// Internal admin controller for demo tenant lifecycle management.
/// Called only by the demo background service (service-to-service); not exposed through the gateway.
/// </summary>
[ApiController]
[Route("api/v4/admin/demo")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class DemoAdminController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IDbContextFactory<NocturneDbContext> _factory;

    public DemoAdminController(ITenantService tenantService, IDbContextFactory<NocturneDbContext> factory)
    {
        _tenantService = tenantService;
        _factory = factory;
    }

    /// <summary>
    /// Idempotent provisioning: creates the demo tenant if it doesn't exist, otherwise returns current state.
    /// </summary>
    [HttpPost("provision")]
    [ProducesResponseType(typeof(DemoStateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Provision(CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existing = await db.Set<TenantEntity>()
            .Include(t => t.DemoConfig)
            .FirstOrDefaultAsync(t => t.IsDemo, ct);

        if (existing is not null)
            return Ok(ToDto(existing, alreadyExisted: true));

        var created = await _tenantService.CreateWithoutOwnerAsync("demo", "Nocturne Demo", ct);

        var tenant = await db.Set<TenantEntity>()
            .FirstAsync(t => t.Id == created.Id, ct);

        tenant.IsDemo = true;

        var config = new TenantDemoConfigEntity { TenantId = tenant.Id };
        db.Set<TenantDemoConfigEntity>().Add(config);

        await db.SaveChangesAsync(ct);

        tenant.DemoConfig = config;
        return Ok(ToDto(tenant, alreadyExisted: false));
    }

    /// <summary>
    /// Update demo tenant operational state.
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(DemoStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromBody] DemoStatusPatchDto patch, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var tenant = await db.Set<TenantEntity>()
            .Include(t => t.DemoConfig)
            .FirstOrDefaultAsync(t => t.IsDemo, ct);

        if (tenant is null)
            return NotFound();

        var config = tenant.DemoConfig;
        if (config is null)
            return NotFound();

        if (patch.NextResetAt.HasValue)
            config.NextResetAt = patch.NextResetAt.Value;

        if (patch.LastResetAt.HasValue)
            config.LastResetAt = patch.LastResetAt.Value;

        if (patch.IsActive.HasValue)
            tenant.IsActive = patch.IsActive.Value;

        await db.SaveChangesAsync(ct);

        return Ok(ToDto(tenant, alreadyExisted: true));
    }

    /// <summary>
    /// Get demo tenant current state.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(DemoStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var tenant = await db.Set<TenantEntity>()
            .Include(t => t.DemoConfig)
            .FirstOrDefaultAsync(t => t.IsDemo, ct);

        if (tenant is null)
            return NotFound();

        return Ok(ToDto(tenant, alreadyExisted: true));
    }

    private static DemoStateDto ToDto(TenantEntity tenant, bool alreadyExisted) => new(
        TenantId: tenant.Id,
        Slug: tenant.Slug,
        IsActive: tenant.IsActive,
        NextResetAt: tenant.DemoConfig?.NextResetAt,
        LastResetAt: tenant.DemoConfig?.LastResetAt,
        AccessMode: tenant.DemoConfig?.AccessMode ?? "open",
        BackfillDays: tenant.DemoConfig?.BackfillDays ?? 90,
        IntervalMinutes: tenant.DemoConfig?.IntervalMinutes ?? 5,
        ResetIntervalMinutes: tenant.DemoConfig?.ResetIntervalMinutes ?? 0,
        AlreadyExisted: alreadyExisted);
}

public record DemoStateDto(
    Guid TenantId,
    string Slug,
    bool IsActive,
    DateTime? NextResetAt,
    DateTime? LastResetAt,
    string AccessMode,
    int BackfillDays,
    int IntervalMinutes,
    int ResetIntervalMinutes,
    bool AlreadyExisted);

public record DemoStatusPatchDto(
    DateTime? NextResetAt = null,
    DateTime? LastResetAt = null,
    bool? IsActive = null);
