<script lang="ts">
  import { tryGetRealtimeStore } from "$lib/stores/realtime-store.svelte";
  import { STALE_THRESHOLD_MS } from "$lib/constants/staleness";
  import { formatGlucoseValue } from "$lib/utils/formatting";
  import {
    glucoseUnits,
    sidebarWidget,
    haloDialConfig,
  } from "$lib/stores/appearance-store.svelte";
  import { GlucoseValueIndicator } from "$lib/components/shared";
  import HaloDial from "$lib/components/dashboard/halo-dial/HaloDial.svelte";
  import { createChartDataEngine } from "$lib/components/dashboard/glucose-chart/engine/chart-data-engine.svelte";
  import GlucoseChartShell from "$lib/components/dashboard/glucose-chart/GlucoseChartShell.svelte";
  import GlucoseTrack from "$lib/components/dashboard/glucose-chart/tracks/GlucoseTrack.svelte";
  import ThresholdRules from "$lib/components/dashboard/glucose-chart/tracks/ThresholdRules.svelte";

  const realtimeStore = tryGetRealtimeStore();

  // Engine for the sidebar chart — no predictions, no inspection
  // svelte-ignore state_referenced_locally
  const sidebarEngine = createChartDataEngine({ enablePredictions: false, focusHours: 3 });

  // Collapsed state needs basic BG info
  const rawCurrentBG = $derived(realtimeStore?.currentBG ?? 0);
  const lastUpdated = $derived(realtimeStore?.lastUpdated ?? 0);
  const now = $derived(realtimeStore?.now ?? Date.now());
  const isConnected = $derived(realtimeStore?.isConnected ?? false);
  const isStale = $derived(now - lastUpdated > STALE_THRESHOLD_MS);
  const isDisconnected = $derived(!isConnected);
  const isLoading = $derived(
    rawCurrentBG === 0 && (realtimeStore?.entries.length ?? 0) === 0
  );
  const units = $derived(glucoseUnits.current);
  const displayBG = $derived(formatGlucoseValue(rawCurrentBG, units));
  const widget = $derived(sidebarWidget.current);
</script>

<!-- Expanded state: widget based on preference -->
<div class="group-data-[collapsible=icon]:hidden">
  {#if widget === "halo-dial"}
    <div class="flex justify-center">
      <HaloDial configOverride={haloDialConfig.current} />
    </div>
  {:else}
    <div class="px-2">
      <GlucoseChartShell engine={sidebarEngine} heightClass="h-[200px]">
        {#snippet tracks(_ctx)}
          <ThresholdRules />
          <GlucoseTrack />
        {/snippet}
      </GlucoseChartShell>
    </div>
  {/if}
</div>

<!-- Collapsed state: just show current BG -->
<div class="hidden group-data-[collapsible=icon]:flex justify-center">
  <GlucoseValueIndicator
    displayValue={displayBG}
    rawBgMgdl={rawCurrentBG}
    {isLoading}
    {isStale}
    {isDisconnected}
    size="xs"
    class="text-lg"
  />
</div>
