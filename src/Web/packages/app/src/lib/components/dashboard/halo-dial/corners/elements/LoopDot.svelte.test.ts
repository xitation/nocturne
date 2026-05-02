import { render } from "vitest-browser-svelte";
import { describe, it, expect } from "vitest";
import LoopDot from "./LoopDot.svelte";

describe("LoopDot", () => {
	it("renders green dot for closed status", () => {
		const { container } = render(LoopDot, {
			value: { status: "closed" },
		});
		const el = container.querySelector("[data-testid='loop-dot']");
		expect(el).not.toBeNull();
		expect(el!.getAttribute("data-status")).toBe("closed");
		const style = el!.getAttribute("style") ?? "";
		expect(style).toContain("--success");
	});

	it("renders muted dot when value is null", () => {
		const { container } = render(LoopDot, { value: null });
		const el = container.querySelector("[data-testid='loop-dot']");
		expect(el!.getAttribute("data-status")).toBe("no-data");
		const style = el!.getAttribute("style") ?? "";
		expect(style).toContain("--muted-foreground");
	});

	it("renders red dot for open status", () => {
		const { container } = render(LoopDot, {
			value: { status: "open" },
		});
		const el = container.querySelector("[data-testid='loop-dot']");
		const style = el!.getAttribute("style") ?? "";
		expect(style).toContain("--destructive");
	});
});
