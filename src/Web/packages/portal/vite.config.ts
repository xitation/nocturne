import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
// WUCHALE-DISABLED: wuchale temporarily disabled
// import { wuchale } from '@wuchale/vite-plugin';
import lingo from 'vite-plugin-lingo';
import { blogManifest } from '@nocturne/cms/blog/vite-plugin';
import { resolve } from 'node:path';
import { cpSync } from 'node:fs';
import { defineConfig, type Plugin, type PluginOption } from 'vite';

function sharedLogos(): Plugin {
  return {
    name: 'shared-logos',
    buildStart() {
      cpSync(
        resolve(__dirname, '../app/static/logos'),
        resolve(__dirname, 'static/logos'),
        { recursive: true }
      );
    }
  };
}

export default defineConfig({
  plugins: [
    sharedLogos(),
    tailwindcss(),
    lingo({
      route: '/_translations',
      localesDir: '../../locales',
    }) as PluginOption,
    blogManifest({ contentDir: resolve(__dirname, 'src/content/blog') }),
    sveltekit()
  ],
  server: {
    host: "0.0.0.0",
    port: parseInt(process.env.PORT || "5173", 10),
    strictPort: true,
  },
  ssr: {
    noExternal: ['@nocturne/app', '@nocturne/ui', '@nocturne/cms', 'lucide-svelte']
  }
});
