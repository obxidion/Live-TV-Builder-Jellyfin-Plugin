using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.LiveTvBuilder.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.LiveTvBuilder;

/// <summary>
/// The Live TV Builder plugin. Adds a config page that auto-wires Jellyfin's
/// native Live TV (M3U tuner + XMLTV guide) from a lineup the admin picks.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Live TV Builder";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("b9f8e2a4-6c1d-4f3a-9e7b-2a5c8d1f0e34");

    /// <inheritdoc />
    public override string Description =>
        "Auto-configure Jellyfin Live TV from a Live TV Builder lineup " +
        "(country, postal code, provider, languages).";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.Configuration.configPage.html",
                    GetType().Namespace)
            }
        };
    }
}
