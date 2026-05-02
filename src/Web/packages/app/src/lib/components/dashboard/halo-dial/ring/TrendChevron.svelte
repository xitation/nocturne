<script lang="ts">
	import { tweened } from "svelte/motion";
	import { cubicOut } from "svelte/easing";
	import { browser } from "$app/environment";
	import { trendAngle, CENTER, RING_RADIUS } from "../geometry";

	let {
		delta,
		color,
		stale,
	}: { delta: number; color: string; stale: boolean } = $props();

	// Captured at mount; preference changes mid-session won't update the tween duration.
	const reducedMotion =
		browser &&
		window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;

	const angle = tweened(trendAngle(delta), {
		duration: reducedMotion ? 0 : 600,
		easing: cubicOut,
	});

	$effect(() => {
		angle.set(trendAngle(delta));
	});
</script>

{#if !stale}
	<g
		transform="rotate({$angle} {CENTER} {CENTER}) translate({CENTER + RING_RADIUS + 5} {CENTER})"
	>
		<path d="M 0 -6.5 Q 5 -3.2 10 0 Q 5 3.2 0 6.5 Z" fill={color} />
	</g>
{/if}
