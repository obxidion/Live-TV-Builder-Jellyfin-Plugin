[![Listed on JellyWatch Hub](https://jellywatch.app/hub/live-tv-builder-jellyfin-plugin/badge.svg)](https://jellywatch.app/hub/live-tv-builder-jellyfin-plugin)
# Live TV Builder — Jellyfin Plugin

Auto-configure Jellyfin's native Live TV from a
[Live TV Builder](https://livetvbuilder.replit.app) lineup. Pick your
**country**, **postal/ZIP code**, **provider lineup**, and **languages** on a
settings page inside Jellyfin, and the plugin wires up an **M3U tuner** + an
**XMLTV guide** for you. No more hunting for or copy-pasting URLs that you have to sift through 14,000 channels just to find the channels you want.

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

- Jellyfin Server **12.2** (target ABI `12.2.0.0`).
- .NET **10 SDK** (to build locally).

Plugin **1.1.0.0** targets Jellyfin 12.2. Older Jellyfin 10.10 servers should
keep the previous plugin release. Back up your Jellyfin configuration before
upgrading the server. The plugin identity and configuration fields are unchanged.

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


## Publishing this upgrade

1. Copy the contents of this source folder into the repository root, including
   the hidden `.github` folder and `.gitignore`. Do not nest the folder inside
   the existing repository.
2. Commit and push the changes to `main`.
3. Create and publish a GitHub Release tagged **`v1.1.0.0`**, targeting that commit.
4. Wait for **Build & Release Plugin** to succeed. It builds and attaches the
   installable plugin ZIP and updates `manifest.json` with its URL and checksum.
5. On Jellyfin 12.2, refresh the plugin catalog, install the update, and restart.

The included `manifest.json` preserves the existing published version. Do not
replace it with an empty template or manually advertise the new version before
its release asset exists. If the repository manifest changes before you push,
preserve the newer entries too.

This archive is **source code for updating GitHub**, not an installable plugin
ZIP. The Release workflow creates the installable ZIP. The plugin compiles
against Jellyfin 12.2; installation and guide refresh should still be checked on
your Jellyfin server.

## License

MIT (see `LICENSE`). 
