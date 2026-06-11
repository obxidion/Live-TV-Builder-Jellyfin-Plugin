using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LiveTvBuilder.Api;

/// <summary>
/// Admin-only endpoints backing the plugin config page. The Providers endpoint
/// proxies the Live TV Builder API server-side so the browser never makes a
/// cross-origin request (no CORS needed on the upstream API). Apply writes the
/// chosen lineup into Jellyfin's Live TV configuration.
/// </summary>
[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("LiveTvBuilder")]
[Produces("application/json")]
public class LiveTvBuilderController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationManager _config;
    private readonly ITaskManager _taskManager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<LiveTvBuilderController> _logger;

    public LiveTvBuilderController(
        IHttpClientFactory httpClientFactory,
        IConfigurationManager config,
        ITaskManager taskManager,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _taskManager = taskManager;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<LiveTvBuilderController>();
    }

    private static string ApiBase =>
        Plugin.Instance!.Configuration.ApiBaseUrl.TrimEnd('/');

    /// <summary>
    /// Proxy: returns the upstream supported-country list. Sourced live from the
    /// API so a new country becomes selectable in the plugin with no plugin release.
    /// </summary>
    [HttpGet("Countries")]
    public async Task<ActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var url = $"{ApiBase}/api/countries";
        var client = _httpClientFactory.CreateClient();
        var body = await client
            .GetStringAsync(url, cancellationToken)
            .ConfigureAwait(false);
        return Content(body, "application/json");
    }

    /// <summary>
    /// Proxy: returns the upstream provider list, scoped to a country when given.
    /// Country scoping matters because the download endpoints reject a
    /// provider/country mismatch with HTTP 400.
    /// </summary>
    [HttpGet("Providers")]
    public async Task<ActionResult> GetProviders(
        [FromQuery] string? country,
        CancellationToken cancellationToken)
    {
        var url = $"{ApiBase}/api/providers";
        if (!string.IsNullOrWhiteSpace(country))
        {
            url += "?country=" + Uri.EscapeDataString(country);
        }

        var client = _httpClientFactory.CreateClient();
        var body = await client
            .GetStringAsync(url, cancellationToken)
            .ConfigureAwait(false);
        return Content(body, "application/json");
    }

    /// <summary>Saves the lineup and writes it into Jellyfin Live TV config.</summary>
    [HttpPost("Apply")]
    public ActionResult Apply([FromBody] ApplyRequest request)
    {
        var cfg = Plugin.Instance!.Configuration;
        cfg.Country = request.Country ?? cfg.Country;
        cfg.PostalCode = request.PostalCode ?? string.Empty;
        cfg.ProviderId = request.ProviderId ?? string.Empty;
        cfg.Languages = request.Languages ?? string.Empty;
        cfg.Addons = request.Addons ?? string.Empty;

        var configurator = new LiveTvConfigurator(
            _config,
            _taskManager,
            _loggerFactory.CreateLogger<LiveTvConfigurator>());
        configurator.Apply(cfg);

        _logger.LogInformation("Live TV Builder config applied for provider {ProviderId}", cfg.ProviderId);
        return Ok(new ApplyResult { Ok = true });
    }
}

/// <summary>Body for POST /LiveTvBuilder/Apply.</summary>
public class ApplyRequest
{
    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public string? ProviderId { get; set; }

    public string? Languages { get; set; }

    public string? Addons { get; set; }
}

/// <summary>Response for POST /LiveTvBuilder/Apply.</summary>
public class ApplyResult
{
    public bool Ok { get; set; }
}
