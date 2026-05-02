<script lang="ts">
	interface Props {
		value: { direction: string } | null;
	}

	const { value }: Props = $props();

	const LABELS: Record<string, string> = {
		DoubleUp: "Rising fast",
		SingleUp: "Rising",
		FortyFiveUp: "Slowly rising",
		Flat: "Steady",
		FortyFiveDown: "Slowly falling",
		SingleDown: "Falling",
		DoubleDown: "Falling fast",
		NotComputable: "Unknown",
		RateOutOfRange: "Out of range",
	};

	const text = $derived.by(() => {
		if (value === null) return "--";
		return LABELS[value.direction] ?? value.direction;
	});
</script>

<span
	class="inline-flex items-center gap-1 text-[10px] text-foreground"
	style="font-family: var(--font-mono);"
	data-testid="direction"
	data-direction={value?.direction ?? "none"}
>
	<span>{text}</span>
</span>
