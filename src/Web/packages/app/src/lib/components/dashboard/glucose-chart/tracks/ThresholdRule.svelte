<script lang="ts">
  import { Rule } from "layerchart";
  import { getGlucoseChartContext } from "../chart-context.svelte";

  interface Props {
    level: "high" | "low" | "veryHigh" | "veryLow";
    class?: string;
    strokeDasharray?: string;
  }

  let { level, class: className = "", strokeDasharray = "4,4" }: Props = $props();

  const ctx = getGlucoseChartContext();
  // Pass raw threshold value — layerchart's <Rule> runs it through yScale
  const y = $derived(ctx.engine.thresholds[level]);
</script>

<Rule {y} class={className} stroke-dasharray={strokeDasharray} />
