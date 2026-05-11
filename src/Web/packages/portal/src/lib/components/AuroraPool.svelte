<script lang="ts">
    interface Props {
        textBlock?: HTMLElement | null;
    }
    let { textBlock = null }: Props = $props();

    // ── Chip definitions ──────────────────────────────────────────────────────
    // hPos: left or right as % of container width. Preserved from the original
    // scattered layout so initial positions look like the current design.
    const CHIP_DEFS = [
        { id: "dexcom",     file: "dexcom.png",        name: "Dexcom",          topPct: 8,  hPos: { left:  44 } },
        { id: "loop",       file: "loop.png",           name: "Loop",             topPct: 25, hPos: { left:  30 } },
        { id: "trio",       file: "trio.jpg",           name: "Trio",             topPct: 30, hPos: { right: 30 } },
        { id: "libre",      file: "libre.png",          name: "FreeStyle Libre",  topPct: 48, hPos: { left:  26 } },
        { id: "nightscout", file: "nightscout.png",     name: "Nightscout",       topPct: 44, hPos: { right: 26 } },
        { id: "tandem",     file: "tandem.png",         name: "Tandem",           topPct: 70, hPos: { left:  20 } },
        { id: "omnipod",    file: "omnipod.png",        name: "Omnipod",          topPct: 67, hPos: { right: 18 } },
        { id: "aaps",       file: "aaps.png",           name: "AAPS",             topPct: 82, hPos: { right: 38 } },
        { id: "xdrip",      file: "xdrip.jpg",          name: "xDrip+",           topPct: 84, hPos: { left:   8 } },
    ] as const;

    const INIT_ROTS = [-4, 6, -8, 5, -6, 7, -3, 9, -10] as const;

    // ── Physics constants ─────────────────────────────────────────────────────
    const BOB_FREQ_V   = 0.7;   // rad/s — vertical bob frequency
    const BOB_AMP_V    = 20;    // px/s² force amplitude (vertical)
    const BOB_FREQ_H   = 0.45;  // rad/s — horizontal drift frequency
    const BOB_AMP_H    = 8;     // px/s² force amplitude (horizontal)
    const LINEAR_DAMP  = 0.015; // fraction of velocity lost per frame (not per second)
    const ANGULAR_DAMP = 0.025; // same for rotation
    const RESTITUTION  = 0.55;  // bounciness (0 = dead stop, 1 = perfectly elastic)
    const SPRING_K     = 600;   // drag spring constant (px/s² per px of offset)
    const MAX_DT       = 0.05;  // seconds — cap to prevent tunnelling on hidden tabs
    const ANG_KICK     = 30;    // deg/s angular impulse added on collision
    const TEXT_PAD     = 10;    // px padding added around text block collision rect

    // ── Physics state (plain JS — NOT $state, no reactivity overhead) ─────────
    interface CS {
        cx: number; cy: number;         // center position in container-local px
        vx: number; vy: number;         // velocity px/s
        rot: number; rotV: number;      // rotation deg, angular velocity deg/s
        phase: number;                  // bobbing phase offset (0–2π), unique per chip
        r: number; w: number; h: number; // collision radius, measured pixel size
        isDragged: boolean;
    }

    // containerEl is $state so $effect tracks it (triggers after bind:this fires)
    let containerEl: HTMLDivElement | null = $state(null);

    // chipElRefs is plain — populated synchronously by the assignRef action before $effect runs
    const chipElRefs: (HTMLElement | null)[] = Array(CHIP_DEFS.length).fill(null);

    const cs: CS[] = CHIP_DEFS.map((_, i) => ({
        cx: 0, cy: 0, vx: 0, vy: 0,
        rot: INIT_ROTS[i], rotV: 0,
        phase: (i / CHIP_DEFS.length) * Math.PI * 2,
        r: 30, w: 60, h: 30,
        isDragged: false,
    }));

    let lastTime = 0;
    let rafId    = 0;
    let pointer  = { x: 0, y: 0 };
    let dragIdx  = -1;
    let textRect: { x: number; y: number; w: number; h: number } | null = null;

    // ── Svelte action: collect chip element refs without triggering reactivity ──
    function assignRef(el: HTMLElement, i: number) {
        chipElRefs[i] = el;
        return { destroy() { chipElRefs[i] = null; } };
    }

    // ── Measurement helpers ───────────────────────────────────────────────────
    function measureSizes() {
        chipElRefs.forEach((el, i) => {
            if (!el) return;
            const r = el.getBoundingClientRect();
            cs[i].w = r.width;
            cs[i].h = r.height;
            // Treat the pill as a circle with radius = half the longest dimension.
            // Overestimates a little but gives a pleasant "bubble" feel on collision.
            cs[i].r = Math.max(r.width, r.height) / 2;
        });
    }

    function initPositions() {
        if (!containerEl) return;
        const cw = containerEl.offsetWidth;
        const ch = containerEl.offsetHeight;
        CHIP_DEFS.forEach((def, i) => {
            cs[i].cy = ch * (def.topPct / 100);
            cs[i].cx = 'left' in def.hPos
                ? cw * ((def.hPos as { left: number }).left   / 100)
                : cw * (1 - (def.hPos as { right: number }).right / 100);
        });
        applyTransforms();
    }

    function measureTextRect() {
        if (!containerEl || !textBlock) { textRect = null; return; }
        const cr = containerEl.getBoundingClientRect();
        const tr = textBlock.getBoundingClientRect();
        textRect = {
            x: tr.left - cr.left - TEXT_PAD,
            y: tr.top  - cr.top  - TEXT_PAD,
            w: tr.width  + TEXT_PAD * 2,
            h: tr.height + TEXT_PAD * 2,
        };
    }

    // ── Collision functions ───────────────────────────────────────────────────
    function wallCollide(c: CS) {
        if (!containerEl) return;
        const cw = containerEl.offsetWidth;
        const ch = containerEl.offsetHeight;
        if (c.cx - c.r < 0)  { c.cx = c.r;      c.vx =  Math.abs(c.vx) * RESTITUTION; c.rotV += ANG_KICK * (Math.random() * 2 - 1); }
        if (c.cx + c.r > cw) { c.cx = cw - c.r; c.vx = -Math.abs(c.vx) * RESTITUTION; c.rotV += ANG_KICK * (Math.random() * 2 - 1); }
        if (c.cy - c.r < 0)  { c.cy = c.r;      c.vy =  Math.abs(c.vy) * RESTITUTION; c.rotV += ANG_KICK * (Math.random() * 2 - 1); }
        if (c.cy + c.r > ch) { c.cy = ch - c.r; c.vy = -Math.abs(c.vy) * RESTITUTION; c.rotV += ANG_KICK * (Math.random() * 2 - 1); }
    }

    function chipCollide(a: CS, b: CS) {
        const dx    = b.cx - a.cx;
        const dy    = b.cy - a.cy;
        const dist2 = dx * dx + dy * dy;
        const minD  = a.r + b.r;
        if (dist2 >= minD * minD || dist2 < 1e-6) return;

        const dist    = Math.sqrt(dist2);
        const nx      = dx / dist;
        const ny      = dy / dist;
        const overlap = minD - dist;

        // Push apart (each takes half if neither is dragged)
        if (!a.isDragged) { a.cx -= nx * overlap * 0.5; a.cy -= ny * overlap * 0.5; }
        if (!b.isDragged) { b.cx += nx * overlap * 0.5; b.cy += ny * overlap * 0.5; }

        // Relative velocity along collision normal
        const relV = (b.vx - a.vx) * nx + (b.vy - a.vy) * ny;
        if (relV > 0) return; // already separating — skip impulse

        // Equal-mass impulse exchange
        const j = -(1 + RESTITUTION) * relV / 2;
        if (!a.isDragged) { a.vx -= j * nx; a.vy -= j * ny; a.rotV += ANG_KICK * Math.sign(a.vx || 1); }
        if (!b.isDragged) { b.vx += j * nx; b.vy += j * ny; b.rotV += ANG_KICK * Math.sign(b.vx || 1); }
    }

    function textCollide(c: CS) {
        if (!textRect) return;
        const { x, y, w, h } = textRect;

        // Closest point on the AABB to the chip center
        const clampX = Math.max(x, Math.min(x + w, c.cx));
        const clampY = Math.max(y, Math.min(y + h, c.cy));
        const dx     = c.cx - clampX;
        const dy     = c.cy - clampY;
        const dist2  = dx * dx + dy * dy;
        if (dist2 >= c.r * c.r || dist2 < 1e-6) return;

        const dist = Math.sqrt(dist2);
        const nx   = dx / dist;
        const ny   = dy / dist;

        // Push chip out of overlap
        c.cx += nx * (c.r - dist);
        c.cy += ny * (c.r - dist);

        // Reflect velocity component into the surface
        const dot = c.vx * nx + c.vy * ny;
        if (dot < 0) {
            c.vx -= (1 + RESTITUTION) * dot * nx;
            c.vy -= (1 + RESTITUTION) * dot * ny;
            c.rotV += ANG_KICK * (Math.random() * 2 - 1);
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────
    function applyTransforms() {
        chipElRefs.forEach((el, i) => {
            if (!el) return;
            const c = cs[i];
            // translate positions the top-left corner; transform-origin rotates around center
            el.style.transform       = `translate(${c.cx - c.w / 2}px, ${c.cy - c.h / 2}px) rotate(${c.rot}deg)`;
            el.style.transformOrigin = `${c.w / 2}px ${c.h / 2}px`;
        });
    }

    // ── Physics loop ──────────────────────────────────────────────────────────
    function tick(now: number) {
        const dt = Math.min((now - lastTime) / 1000, MAX_DT);
        lastTime = now;
        const t  = now / 1000;

        // Integrate forces
        for (let i = 0; i < cs.length; i++) {
            const c = cs[i];
            if (c.isDragged) {
                // Spring pull toward pointer — gives "throws" on release
                c.vx += (pointer.x - c.cx) * SPRING_K * dt;
                c.vy += (pointer.y - c.cy) * SPRING_K * dt;
            } else {
                // Sine-wave bob forces — unique phase per chip so they desync naturally
                c.vy += Math.sin(t * BOB_FREQ_V + c.phase)        * BOB_AMP_V * dt;
                c.vx += Math.sin(t * BOB_FREQ_H + c.phase * 1.3)  * BOB_AMP_H * dt;
            }
            c.vx   *= (1 - LINEAR_DAMP);
            c.vy   *= (1 - LINEAR_DAMP);
            c.rotV *= (1 - ANGULAR_DAMP);
            c.cx   += c.vx   * dt;
            c.cy   += c.vy   * dt;
            c.rot  += c.rotV * dt;
        }

        // Collisions
        for (let i = 0; i < cs.length; i++)           if (!cs[i].isDragged) wallCollide(cs[i]);
        for (let i = 0; i < cs.length - 1; i++)
            for (let j = i + 1; j < cs.length; j++)   chipCollide(cs[i], cs[j]);
        for (let i = 0; i < cs.length; i++)           if (!cs[i].isDragged) textCollide(cs[i]);

        applyTransforms();
        rafId = requestAnimationFrame(tick);
    }

    // ── Pointer / drag handlers ───────────────────────────────────────────────
    function onPointerDown(e: PointerEvent, i: number) {
        e.stopPropagation();
        dragIdx        = i;
        cs[i].isDragged = true;
        cs[i].vx       = 0;
        cs[i].vy       = 0;
        if (containerEl) {
            const cr = containerEl.getBoundingClientRect();
            pointer  = { x: e.clientX - cr.left, y: e.clientY - cr.top };
        }
        // Pointer capture: element keeps receiving events even if pointer leaves it
        containerEl?.setPointerCapture(e.pointerId);
    }

    function onPointerMove(e: PointerEvent) {
        if (dragIdx < 0 || !containerEl) return;
        const cr = containerEl.getBoundingClientRect();
        pointer  = { x: e.clientX - cr.left, y: e.clientY - cr.top };
    }

    function onPointerUp(_e: PointerEvent) {
        if (dragIdx >= 0) {
            cs[dragIdx].isDragged = false;
            dragIdx = -1;
        }
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    // Physics init — runs once when containerEl is set (bind:this fires on mount)
    $effect(() => {
        if (!containerEl) return;
        measureSizes();
        initPositions();
        lastTime = performance.now();
        rafId    = requestAnimationFrame(tick);
        const ro = new ResizeObserver(() => measureTextRect());
        ro.observe(containerEl);
        return () => { cancelAnimationFrame(rafId); ro.disconnect(); };
    });

    // Text rect — re-measures whenever the textBlock prop changes
    // (parent's bind:this fires after its own mount, after this effect)
    $effect(() => { measureTextRect(); });
</script>

<div class="absolute inset-0 overflow-hidden" aria-hidden="true" bind:this={containerEl}
     onpointermove={onPointerMove}>

    <!-- Mobile: static chip wrap (unchanged from original) -->
    <div class="md:hidden absolute top-16 left-0 right-0 px-5">
        <div class="flex flex-wrap gap-1.5 justify-center">
            {#each CHIP_DEFS as def}
                <div class="flex items-center gap-1.5 bg-[oklch(0.10_0.028_261/85%)] border border-[oklch(1_0_0/20%)] rounded-full py-1 pr-2.5 pl-1 backdrop-blur-sm">
                    <img src="/logos/{def.file}" alt="" class="size-4 rounded object-cover" />
                    <span class="text-[10px] font-semibold text-white whitespace-nowrap">{def.name}</span>
                </div>
            {/each}
        </div>
    </div>

    <!-- Desktop: physics-driven chips -->
    <!--
        Each chip starts at top:0 left:0 and is repositioned entirely by
        applyTransforms() via el.style.transform each rAF frame.
        pointer-events-auto + touch-none lets pointer events through despite
        the aria-hidden parent being pointer-events-none in the original.
    -->
    {#each CHIP_DEFS as def, i}
        <div
            class="hidden md:flex absolute top-0 left-0 items-center gap-[7px]
                   bg-[oklch(0.10_0.028_261/85%)] border border-[oklch(1_0_0/20%)]
                   rounded-full py-[5px] pr-3 pl-1.5 backdrop-blur-[6px] mix-blend-screen
                   pointer-events-auto cursor-grab active:cursor-grabbing select-none touch-none"
            use:assignRef={i}
            onpointerdown={(e) => onPointerDown(e, i)}
            onpointerup={onPointerUp}
        >
            <img src="/logos/{def.file}" alt="" class="size-5 rounded object-cover" />
            <span class="text-[11px] font-semibold text-white whitespace-nowrap">{def.name}</span>
        </div>
    {/each}

</div>
