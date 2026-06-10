using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.LiveTvBuilder.Configuration;

/// <summary>
/// Plugin settings, persisted locally in Jellyfin. The M3U/XMLTV URLs handed to
/// Live TV are reconstructed from these inputs on every save.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Base URL of the Live TV Builder API. Overridable for testing.</summary>
    public string ApiBaseUrl { get; set; } = "https://livetvbuilder.replit.app";

    /// <summary>Country code: US | CA | MX.</summary>
    public string Country { get; set; } = "US";

    /// <summary>Postal / ZIP / CP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Provider lineup id from /api/providers.</summary>
    public string ProviderId { get; set; } = string.Empty;

    /// <summary>Comma-separated language filter, e.g. "en,es". Empty = no filter.</summary>
    public string Languages { get; set; } = string.Empty;

    /// <summary>Comma-separated optional add-on ids (US-gated). Empty = none.</summary>
    public string Addons { get; set; } = string.Empty;

    /// <summary>Id of the TunerHost this plugin manages, for idempotent replace.</summary>
    public string ManagedTunerId { get; set; } = string.Empty;

    /// <summary>Id of the ListingsProvider this plugin manages, for idempotent replace.</summary>
    public string ManagedListingsId { get; set; } = string.Empty;
}
