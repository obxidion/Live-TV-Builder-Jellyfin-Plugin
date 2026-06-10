using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.LiveTvBuilder.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LiveTvBuilder;

/// <summary>
/// Writes (and idempotently replaces) the plugin-owned M3U TunerHost and
/// XMLTV ListingsProvider entries in Jellyfin's Live TV configuration.
/// </summary>
public class LiveTvConfigurator
{
    private readonly IConfigurationManager _config;
    private readonly ITaskManager _taskManager;
    private readonly ILogger<LiveTvConfigurator> _logger;

    public LiveTvConfigurator(
        IConfigurationManager config,
        ITaskManager taskManager,
        ILogger<LiveTvConfigurator> logger)
    {
        _config = config;
        _taskManager = taskManager;
        _logger = logger;
    }

    /// <summary>
    /// Applies the given plugin configuration to Jellyfin's Live TV options.
    /// Persists the resulting tuner/listings ids back onto <paramref name="cfg"/>.
    /// </summary>
    public void Apply(PluginConfiguration cfg)
    {
        var m3uUrl = BuildUrl(cfg, "m3u");
        var epgUrl = BuildUrl(cfg, "epg");

        var options = (LiveTvOptions)_config.GetConfiguration("livetv");

        // ---- M3U TunerHost (replace our previous one in place) ----
        var tuners = options.TunerHosts?.ToList() ?? new List<TunerHostInfo>();
        if (!string.IsNullOrEmpty(cfg.ManagedTunerId))
        {
            tuners.RemoveAll(t => string.Equals(t.Id, cfg.ManagedTunerId, StringComparison.Ordinal));
        }

        var tunerId = Guid.NewGuid().ToString("N");
        tuners.Add(new TunerHostInfo
        {
            Id = tunerId,
            Type = "m3u",
            Url = m3uUrl,
            FriendlyName = "Live TV Builder"
        });
        options.TunerHosts = tuners.ToArray();

        // ---- XMLTV ListingsProvider (replace our previous one in place) ----
        var listings = options.ListingProviders?.ToList() ?? new List<ListingsProviderInfo>();
        if (!string.IsNullOrEmpty(cfg.ManagedListingsId))
        {
            listings.RemoveAll(l => string.Equals(l.Id, cfg.ManagedListingsId, StringComparison.Ordinal));
        }

        var listingsId = Guid.NewGuid().ToString("N");
        listings.Add(new ListingsProviderInfo
        {
            Id = listingsId,
            Type = "xmltv",
            Path = epgUrl,
            EnableAllTuners = true
        });
        options.ListingProviders = listings.ToArray();

        _config.SaveConfiguration("livetv", options);

        cfg.ManagedTunerId = tunerId;
        cfg.ManagedListingsId = listingsId;
        Plugin.Instance!.SaveConfiguration();

        _logger.LogInformation(
            "Live TV Builder applied: tuner {TunerId}, listings {ListingsId}",
            tunerId,
            listingsId);

        TryRefreshGuide();
    }

    /// <summary>
    /// Best-effort kick of Jellyfin's "Refresh Guide" scheduled task so channels
    /// and EPG appear without waiting for the next scheduled cycle. Swallows
    /// failures (e.g. task key renamed in a future ABI) — Jellyfin will still
    /// refresh on its own schedule.
    /// </summary>
    private void TryRefreshGuide()
    {
        try
        {
            var worker = _taskManager.ScheduledTasks.FirstOrDefault(
                t => string.Equals(t.ScheduledTask.Key, "RefreshGuide", StringComparison.OrdinalIgnoreCase));
            if (worker != null)
            {
                _taskManager.Execute(worker, new TaskOptions());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Live TV Builder: could not trigger a guide refresh; Jellyfin will refresh on its schedule.");
        }
    }

    private static string BuildUrl(PluginConfiguration cfg, string kind)
    {
        var baseUrl = cfg.ApiBaseUrl.TrimEnd('/');
        var query = new List<string>
        {
            "country=" + Uri.EscapeDataString(cfg.Country),
            "zip=" + Uri.EscapeDataString(cfg.PostalCode),
            "providerId=" + Uri.EscapeDataString(cfg.ProviderId)
        };

        if (!string.IsNullOrWhiteSpace(cfg.Languages))
        {
            query.Add("languages=" + Uri.EscapeDataString(cfg.Languages));
        }

        if (!string.IsNullOrWhiteSpace(cfg.Addons))
        {
            query.Add("addons=" + Uri.EscapeDataString(cfg.Addons));
        }

        return $"{baseUrl}/api/download/{kind}?{string.Join("&", query)}";
    }
}
