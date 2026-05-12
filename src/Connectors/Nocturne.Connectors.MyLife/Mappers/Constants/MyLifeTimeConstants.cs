namespace Nocturne.Connectors.MyLife.Mappers.Constants;

internal static class MyLifeTimeConstants
{
    internal const int CarbSuppressionWindowMs = 2 * 60 * 1000;

    /// <summary>
    /// Maximum overlap window for cross-month consolidation context.
    /// Uses the max configurable TempBasalConsolidationWindowMinutes (30 min).
    /// </summary>
    internal const int MaxConsolidationOverlapMs = 30 * 60 * 1000;
}