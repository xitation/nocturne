import { render } from "vitest-browser-svelte";
import { describe, it, expect } from "vitest";
import Direction from "./Direction.svelte";

describe("Direction", () => {
	it("renders 'Steady' for Flat direction", () => {
		const { container } = render(Direction, {
			value: { direction: "Flat" },
		});
		const el = container.querySelector("[data-testid='direction']");
		expect(el!.textContent).toContain("Steady");
	});

	it("renders placeholder when value is null", () => {
		const { container } = render(Direction, { value: null });
		const el = container.querySelector("[data-testid='direction']");
		expect(el!.textContent).toContain("--");
	});

	it("renders 'Slowly rising' for FortyFiveUp", () => {
		const { container } = render(Direction, {
			value: { direction: "FortyFiveUp" },
		});
		const el = container.querySelector("[data-testid='direction']");
		expect(el!.textContent).toContain("Slowly rising");
		expect(el!.getAttribute("data-direction")).toBe("FortyFiveUp");
	});
});
