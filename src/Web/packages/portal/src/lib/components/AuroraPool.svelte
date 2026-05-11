<script lang="ts">
    // Decorative floating layer — TIR bar + connector chips
    // positioned absolutely over the aurora canvas.

    const chips = [
        { id: "dexcom", file: "dexcom.png", name: "Dexcom", top: "8%", left: "44%", rot: -4 },
        { id: "loop", file: "loop.png", name: "Loop", top: "25%", left: "30%", rot: 6 },
        { id: "trio", file: "trio.jpg", name: "Trio", top: "30%", right: "30%", rot: -8 },
        { id: "libre", file: "libre.png", name: "FreeStyle Libre", top: "48%", left: "26%", rot: 5 },
        { id: "nightscout", file: "nightscout.png", name: "Nightscout", top: "44%", right: "26%", rot: -6 },
        { id: "tandem", file: "tandem.png", name: "Tandem", top: "70%", left: "20%", rot: 7 },
        { id: "omnipod", file: "omnipod.png", name: "Omnipod", top: "67%", right: "18%", rot: -3 },
        { id: "aaps", file: "aaps.png", name: "AAPS", top: "82%", right: "38%", rot: 9 },
        { id: "xdrip", file: "xdrip.jpg", name: "xDrip+", top: "84%", left: "8%", rot: -10 },
    ] as const;

type Chip = (typeof chips)[number];
    function chipStyle(c: Chip): string {
        const parts: string[] = [`top:${c.top}`, `transform:rotate(${c.rot}deg)`];
        if ("left" in c) parts.push(`left:${c.left}`);
        if ("right" in c) parts.push(`right:${c.right}`);
        return parts.join(";");
    }
</script>

<div class="absolute inset-0 pointer-events-none overflow-hidden" aria-hidden="true">

    <!-- Mobile: chips in a centered wrap grid near the top of the canvas -->
    <div class="md:hidden absolute top-16 left-0 right-0 px-5">
        <div class="flex flex-wrap gap-1.5 justify-center">
            {#each chips as chip}
                <div class="flex items-center gap-1.5 bg-[oklch(0.10_0.028_261/85%)] border border-[oklch(1_0_0/20%)] rounded-full py-1 pr-2.5 pl-1 backdrop-blur-sm">
                    <img src="/logos/{chip.file}" alt="" class="size-4 rounded object-cover" />
                    <span class="text-[10px] font-semibold text-white whitespace-nowrap">{chip.name}</span>
                </div>
            {/each}
        </div>
    </div>

    <!-- Desktop: scattered chips -->
    {#each chips as chip}
        <div
            class="hidden md:flex absolute items-center gap-[7px] bg-[oklch(0.10_0.028_261/85%)] border border-[oklch(1_0_0/20%)] rounded-full py-[5px] pr-3 pl-1.5 backdrop-blur-[6px] mix-blend-screen"
            style={chipStyle(chip)}
        >
            <img src="/logos/{chip.file}" alt="" class="size-5 rounded object-cover" />
            <span class="text-[11px] font-semibold text-white whitespace-nowrap">{chip.name}</span>
        </div>
    {/each}

</div>
