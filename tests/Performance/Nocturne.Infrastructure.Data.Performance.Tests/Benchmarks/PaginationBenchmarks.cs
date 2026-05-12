using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Nocturne.Infrastructure.Data.Entities.V4;
using Nocturne.Infrastructure.Data.Performance.Tests.Infrastructure;

namespace Nocturne.Infrastructure.Data.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class PaginationBenchmarks
{
    private PostgresFixture _fixture = null!;
    private Guid _tenantId;

    // Fixed at 100k rows — we're varying pagination depth, not data volume
    private const int RowCount = 100_000;

    [Params(0, 1_000, 10_000, 50_000)]
    public int Offset;

    [GlobalSetup]
    public async Task Setup()
    {
        _fixture = new PostgresFixture();
        await _fixture.InitializeAsync();

        _tenantId = Guid.CreateVersion7();
        await using var ctx = _fixture.CreateContext();
        await DataSeeder.SeedSensorGlucoseAsync(ctx, _tenantId, RowCount);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _fixture.DisposeAsync();
    }

    [Benchmark(Description = "Offset_Pagination")]
    public async Task<List<SensorGlucoseEntity>> OffsetPagination()
    {
        if (!_fixture.IsInitialized) return [];
        await using var ctx = _fixture.CreateContext();

        return await ctx.SensorGlucose.AsNoTracking()
            .Where(e => e.TenantId == _tenantId)
            .OrderByDescending(e => e.Timestamp)
            .Skip(Offset)
            .Take(100)
            .ToListAsync();
    }
}
