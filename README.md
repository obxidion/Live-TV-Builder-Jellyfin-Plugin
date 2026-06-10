# Live TV Builder — Jellyfin Plugin

Auto-configure Jellyfin's native Live TV from a
[Live TV Builder](https://livetvbuilder.replit.app) lineup. Pick your
**country**, **postal/ZIP code**, **provider lineup**, and **languages** on a
settings page inside Jellyfin, and the plugin wires up an **M3U tuner** + an
**XMLTV guide** for you — no copy-pasting URLs.

This is the **Tier 1 "config helper"** plugin described in the
[spec](../docs/jellyfin-plugin-spec.md). It reuses Jellyfin's built-in M3U/XMLTV
support and Live TV Builder's existing public, auto-refreshing download URLs, so
it needs no account login and no changes to the Live TV Builder backend.


## How it works

1. The config page calls a small admin-only endpoint in the plugin
   (`/LiveTvBuilder/Providers`) which **proxies** `GET /api/providers` from Live
   TV Builder server-side — so the browser never makes a cross-origin request.
2. On save, `POST /LiveTvBuilder/Apply` stores your inputs and constructs two
   URLs:
   - `…/api/download/m3u?country=…&zip=…&providerId=…&languages=…`
   - `…/api/download/epg?…same…`
3. The plugin writes those into Jellyfin's Live TV config as an `m3u` TunerHost
   and an `xmltv` ListingsProvider (replacing its own previous entries), then
   Jellyfin keeps them fresh on its normal guide-refresh schedule.

## Requirements

- Jellyfin **10.10.x** (adjust `targetAbi` in `build.yaml` and the package
  versions in the `.csproj` for other versions).
- .NET 8 SDK (to build locally).

## Build locally

```bash
dotnet build Jellyfin.Plugin.LiveTvBuilder/Jellyfin.Plugin.LiveTvBuilder.csproj -c Release
```

The compiled `Jellyfin.Plugin.LiveTvBuilder.dll` is the plugin artifact. For
manual install, copy it into a `Live TV Builder` folder under your Jellyfin
`plugins` directory and restart the server.

## Install via the plugin repository (recommended)

1. In Jellyfin: **Dashboard → Plugins → Repositories → +**.
2. Add the repository URL (your hosted `manifest.json`):
   `https://raw.githubusercontent.com/obxidion/Live-TV-Builder-Jellyfin-Plugin/main/manifest.json`
3. **Catalog → Live TV Builder → Install**, then restart Jellyfin.
4. Configure under **Dashboard → Plugins → Live TV Builder**.


## License

MIT (see `LICENSE`). 
