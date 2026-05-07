import { describe, it, expect, vi } from "vitest";

vi.mock("$app/environment", () => ({ browser: false, dev: false }));
vi.mock("$app/navigation", () => ({}));
vi.mock("$app/state", () => ({}));
vi.mock("$app/server", () => ({
  getRequestEvent: vi.fn(),
  query: (fn: unknown) => fn,
  command: (fn: unknown) => fn,
  form: (fn: unknown) => fn,
}));
vi.mock("@sveltejs/kit", () => ({
  error: vi.fn(),
  redirect: vi.fn(),
}));

const { getGlucoseColorContinuous, getGlucoseColorByMode } = await import(
  "./chart-colors"
);

describe("getGlucoseColorContinuous", () => {
  it("returns an oklch() string", () => {
    const c = getGlucoseColorContinuous(120);
    expect(c).toMatch(/^oklch\(/);
  });

  it("clamps below the lowest anchor", () => {
    expect(getGlucoseColorContinuous(20)).toBe(getGlucoseColorContinuous(40));
  });

  it("clamps above the highest anchor", () => {
    expect(getGlucoseColorContinuous(500)).toBe(getGlucoseColorContinuous(320));
  });
});

describe("getGlucoseColorByMode", () => {
  const thresholds = { veryLow: 55, low: 70, high: 180, veryHigh: 250 };

  it("returns var() reference in discrete mode", () => {
    expect(getGlucoseColorByMode(120, "discrete", thresholds)).toMatch(/^var\(--glucose-/);
  });

  it("returns oklch() in continuous mode", () => {
    expect(getGlucoseColorByMode(120, "continuous", thresholds)).toMatch(/^oklch\(/);
  });
});
