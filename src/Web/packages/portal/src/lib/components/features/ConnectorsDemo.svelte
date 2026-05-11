<script lang="ts">
  import { Search, Check } from "@lucide/svelte";

  interface Connector {
    file: string;
    name: string;
    kind: string;
    aliases?: string[];
    comingSoon?: true;
    issue?: number;
  }

  const CONNECTORS: Connector[] = [
    {
      file: "dexcom.png",
      name: "Dexcom",
      kind: "CGM",
      aliases: ["Clarity", "g6", "g7"],
    },
    {
      file: "libre.png",
      name: "FreeStyle Libre",
      kind: "CGM",
      aliases: ["Libre", "FSL", "Abbott"],
    },
    { file: "glooko.png", name: "Glooko", kind: "Cloud" },
    { file: "medtronic.jpg", name: "Medtronic", kind: "Pump" },
    {
      file: "omnipod.png",
      name: "Omnipod",
      kind: "Pump",
      aliases: ["Insulet", "DASH", "5"],
    },
    { file: "loop.png", name: "Loop", kind: "Looping" },
    { file: "trio.jpg", name: "Trio", kind: "Looping" },
    {
      file: "aaps.png",
      name: "AndroidAPS",
      kind: "Looping",
      aliases: ["AAPS", "android"],
    },
    {
      file: "xdrip.jpg",
      name: "xDrip+",
      kind: "App",
      aliases: ["xDrip", "android"],
    },
    { file: "mylife.png", name: "myLife", kind: "Pump" },
    {
      file: "myfitnesspal.jpg",
      name: "MyFitnessPal",
      kind: "Food",
      aliases: ["MFP"],
    },
    { file: "tidepool.jpg", name: "Tidepool", kind: "Cloud" },
    { file: "sugarmate.png", name: "Sugarmate", kind: "App" },
    { file: "spike.png", name: "Spike", kind: "App" },
    { file: "juggluco.png", name: "Juggluco", kind: "App" },
    { file: "glucotracker.png", name: "GlucoTracker", kind: "App" },
    {
      file: "nightscout.png",
      name: "Nightscout",
      kind: "Legacy",
      aliases: ["NS"],
    },
    { file: "discord.png", name: "Discord", kind: "Notify" },
    { file: "slack.png", name: "Slack", kind: "Notify" },
    { file: "telegram.png", name: "Telegram", kind: "Notify" },
    {
      file: "home-assistant.png",
      name: "Home Assistant",
      kind: "Smart Home",
      aliases: ["HA", "smart home"],
    },
    // Coming soon
    {
      file: "tandem.png",
      name: "Tandem",
      kind: "Pump",
      aliases: ["TConnect"],
      comingSoon: true,
      issue: 72,
    },
    {
      file: "medtrum.png",
      name: "Medtrum",
      kind: "CGM",
      comingSoon: true,
      issue: 106,
    },
    {
      file: "n8n.png",
      name: "n8n",
      kind: "Cloud",
      comingSoon: true,
      issue: 112,
    },
    {
      file: "twilio.png",
      name: "Twilio / SMS",
      kind: "Messaging",
      aliases: ["SMS", "Twilio"],
      comingSoon: true,
      issue: 137,
    },
    {
      file: "oura.png",
      name: "Oura",
      kind: "App",
      aliases: ["Oura Ring"],
      comingSoon: true,
      issue: 151,
    },
    {
      file: "whatsapp.png",
      name: "WhatsApp",
      kind: "Messaging",
      aliases: ["WA", "whatapp"],
      comingSoon: true,
      issue: 177,
    },
    {
      file: "imessage.png",
      name: "iMessage",
      kind: "Messaging",
      aliases: ["Apple Messages", "iMsg", "ios"],
      comingSoon: true,
      issue: 178,
    },
    {
      file: "google-chat.png",
      name: "Google Chat",
      kind: "Messaging",
      aliases: ["GChat"],
      comingSoon: true,
      issue: 179,
    },
    {
      file: "msteams.png",
      name: "MS Teams",
      kind: "Messaging",
      aliases: ["Teams", "Microsoft Teams"],
      comingSoon: true,
      issue: 180,
    },
    {
      file: "email.png",
      name: "Email",
      kind: "Notify",
      comingSoon: true,
      issue: 181,
    },
  ];

  const TYPE_TARGETS = ["Dexcom", "Libre", "Loop", "Omnipod", "xDrip"];

  interface Props {
    height?: number;
  }
  let { height = 340 }: Props = $props();

  let query = $state("");
  let userTyping = $state(false);

  $effect(() => {
    if (userTyping) return;
    let cancelled = false;
    let ti = 0;
    let i = 0;
    let timer: ReturnType<typeof setTimeout>;

    const tick = () => {
      if (cancelled) return;
      const word = TYPE_TARGETS[ti];
      if (i <= word.length) {
        query = word.slice(0, i);
        i++;
        timer = setTimeout(tick, 130);
      } else {
        timer = setTimeout(() => {
          if (cancelled) return;
          let j = word.length;
          const erase = () => {
            if (cancelled) return;
            if (j >= 0) {
              query = word.slice(0, j);
              j--;
              timer = setTimeout(erase, 60);
            } else {
              i = 0;
              ti = (ti + 1) % TYPE_TARGETS.length;
              timer = setTimeout(tick, 400);
            }
          };
          erase();
        }, 1600);
      }
    };
    tick();
    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  });

  function matches(c: Connector, q: string): boolean {
    const lower = q.toLowerCase();
    return (
      c.name.toLowerCase().includes(lower) ||
      c.kind.toLowerCase().includes(lower) ||
      (c.aliases?.some((a) => a.toLowerCase().includes(lower)) ?? false)
    );
  }

  let filtered = $derived(
    query ? CONNECTORS.filter((c) => matches(c, query)) : CONNECTORS,
  );

  let activeMatches = $derived(filtered.filter((c) => !c.comingSoon).length);
  let comingSoonMatches = $derived(filtered.filter((c) => c.comingSoon).length);
</script>

<div
  class="rounded-[14px] overflow-hidden border border-white/10 bg-[oklch(0.10_0.025_261)] flex flex-col"
  style:height="{height}px"
>
  <!-- Search bar -->
  <div
    class="px-[18px] py-3.5 border-b border-white/[0.08] flex items-center gap-3 bg-[oklch(0.12_0.025_261)] shrink-0"
  >
    <Search class="size-5 text-muted-foreground shrink-0" />
    <input
      class="flex-1 bg-transparent border-none outline-none text-[17px] text-foreground font-medium placeholder:text-muted-foreground/60 min-h-7 w-0"
      placeholder="Find your device or app…"
      aria-label="Search connectors"
      bind:value={query}
      onfocus={() => (userTyping = true)}
      onblur={() => {
        if (!query) userTyping = false;
      }}
      autocomplete="off"
      spellcheck="false"
    />
    {#if !userTyping}
      <span
        class="cursor-blink ml-0.5 inline-block w-0.5 h-5 bg-glucose-in-range align-middle pointer-events-none"
        aria-hidden="true"
      ></span>
    {/if}
    <span
      class="font-mono text-[12px] text-glucose-in-range px-2.5 py-1 rounded-full bg-glucose-in-range/15 border border-glucose-in-range/30 shrink-0"
    >
      {filtered.length} match{filtered.length === 1 ? "" : "es"}
    </span>
  </div>

  <!-- Connector grid -->
  <div
    class="flex-1 p-3.5 overflow-hidden grid content-start gap-2"
    style="grid-template-columns: repeat(auto-fill, minmax(130px, 1fr))"
  >
    {#each filtered.slice(0, 18) as c (c.file)}
      {@const isMatch =
        !!query && c.name.toLowerCase().includes(query.toLowerCase())}
      <div
        class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg border transition-all duration-200
                    {isMatch
          ? 'bg-glucose-in-range/[0.14] border-glucose-in-range/50'
          : 'bg-white/[0.04] border-white/[0.06]'}"
      >
        <img
          src="/logos/{c.file}"
          alt={c.name}
          class="size-5 rounded object-cover shrink-0"
        />
        <span class="text-[13px] text-foreground font-medium truncate"
          >{c.name}</span
        >
        {#if isMatch}
          <Check class="size-3.5 text-glucose-in-range shrink-0 ml-auto" />
        {/if}
      </div>
    {/each}
  </div>
</div>

<style>
  @keyframes blink {
    0%,
    100% {
      opacity: 1;
    }
    50% {
      opacity: 0;
    }
  }
  .cursor-blink {
    animation: blink 1s steps(2) infinite;
  }
</style>
