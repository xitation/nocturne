/**
 * BG color resolvers for the Halo Dial. Two modes:
 *   - **Discrete** delegates to the existing `getGlucoseColor`/CSS variable
 *     pipeline so the dial reads as part of the rest of the dashboard.
 *   - **Continuous** is the only frontend-computed colour in the codebase;
 *     it ports `bgColorContinuous` from the source design (concepts/shared.jsx
 *     lines 24–54) and is a documented exception to the "backend computes
 *     colors" rule because the spectrum is presentation, not categorisation.
 */

import { DEFAULT_THRESHOLDS } from "$lib/constants";
import { getGlucoseColor } from "$lib/utils/chart-colors";
import { HaloDialColorMode } from "$lib/api";

/** Discrete buckets, returns a `var(--glucose-*)` reference. */
export function bgColorDiscrete(mgdl: number): string {
  return getGlucoseColor(mgdl, {
    veryLow: DEFAULT_THRESHOLDS.veryLow,
    low: DEFAULT_THRESHOLDS.low,
    high: DEFAULT_THRESHOLDS.high,
    veryHigh: DEFAULT_THRESHOLDS.veryHigh,
  });
}

/** Anchor stops: [mgdl, hue, chroma, lightness] in oklch space. */
const SPECTRUM_STOPS: ReadonlyArray<readonly [number, number, number, number]> = [
  [40, 25, 0.22, 0.58],
  [55, 40, 0.2, 0.62],
  [70, 85, 0.18, 0.78],
  [90, 150, 0.16, 0.74],
  [120, 175, 0.15, 0.72],
  [150, 200, 0.16, 0.72],
  [180, 235, 0.17, 0.7],
  [220, 275, 0.18, 0.65],
  [260, 310, 0.2, 0.62],
  [320, 340, 0.22, 0.58],
];

/** Continuous oklch interpolation between anchor stops; clamps below/above the table. */
export function bgColorContinuous(mgdl: number): string {
  const first = SPECTRUM_STOPS[0];
  const last = SPECTRUM_STOPS[SPECTRUM_STOPS.length - 1];

  let lo = first;
  let hi = last;

  if (mgdl <= first[0]) {
    lo = first;
    hi = first;
  } else if (mgdl >= last[0]) {
    lo = last;
    hi = last;
  } else {
    for (let i = 0; i < SPECTRUM_STOPS.length - 1; i++) {
      if (mgdl >= SPECTRUM_STOPS[i][0] && mgdl <= SPECTRUM_STOPS[i + 1][0]) {
        lo = SPECTRUM_STOPS[i];
        hi = SPECTRUM_STOPS[i + 1];
        break;
      }
    }
  }

  const t = lo[0] === hi[0] ? 0 : (mgdl - lo[0]) / (hi[0] - lo[0]);

  // Shortest hue path so we never sweep the long way around the wheel.
  let h0 = lo[1];
  let h1 = hi[1];
  if (Math.abs(h1 - h0) > 180) {
    if (h1 > h0) h0 += 360;
    else h1 += 360;
  }

  const h = (h0 + (h1 - h0) * t) % 360;
  const c = lo[2] + (hi[2] - lo[2]) * t;
  const l = lo[3] + (hi[3] - lo[3]) * t;
  return `oklch(${l.toFixed(3)} ${c.toFixed(3)} ${h.toFixed(2)})`;
}

/** Pick the right resolver for the active color mode. */
export function bgColor(mgdl: number, mode: HaloDialColorMode): string {
  return mode === HaloDialColorMode.Continuous
    ? bgColorContinuous(mgdl)
    : bgColorDiscrete(mgdl);
}
