<script lang="ts">
  import { tryGetRealtimeStore } from "$lib/stores/realtime-store.svelte";
  import { STALE_THRESHOLD_MS } from "$lib/constants/staleness";
  import { formatGlucoseValue } from "$lib/utils/formatting";
  import { glucoseUnits } from "$lib/stores/appearance-store.svelte";
  import { GlucoseValueIndicator } from "$lib/components/shared";
  import HaloDial from "$lib/components/dashboard/halo-dial/HaloDial.svelte";

  const realtimeStore = tryGetRealtimeStore();

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
</script>

<!-- Expanded state: full HaloDial -->
<div class="flex justify-center group-data-[collapsible=icon]:hidden">
  <HaloDial />
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
