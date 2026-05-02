<script lang="ts">
	interface Props {
		value: { status: string } | null;
	}

	const { value }: Props = $props();

	function colorFor(status: string | undefined): string {
		switch (status) {
			case "closed":
			case "looping":
				return "var(--success, #16a34a)";
			case "open":
				return "var(--destructive, #dc2626)";
			case "limited":
				return "var(--warning, #eab308)";
			default:
				return "var(--muted-foreground, #9ca3af)";
		}
	}

	const color = $derived(colorFor(value?.status));
	const label = $derived(value === null ? "no-data" : value.status);
</script>

<span
	class="inline-block size-2 rounded-full"
	style="background-color: {color};"
	data-testid="loop-dot"
	data-status={label}
	aria-label="loop status"
></span>
