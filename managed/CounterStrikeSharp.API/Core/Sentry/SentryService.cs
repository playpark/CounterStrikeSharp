/*
 *  This file is part of CounterStrikeSharp.
 *  CounterStrikeSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  CounterStrikeSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with CounterStrikeSharp.  If not, see <https://www.gnu.org/licenses/>. *
 */

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Config;
using Microsoft.Extensions.Logging;
using Sentry;

namespace CounterStrikeSharp.API.Core.Sentry;

/// <summary>
/// Service for Sentry error tracking integration.
/// Implements <see cref="IStartupService"/> for automatic initialization.
/// </summary>
public class SentryService : IStartupService, IDisposable
{
    private readonly ILogger<SentryService> _logger;
    private IDisposable? _sentryDisposable;
    private static SentryService? _instance;
    private readonly ConcurrentDictionary<string, PluginSentryInfo> _pluginSentryInfo = new();

    /// <summary>
    /// Gets the singleton instance of the Sentry service.
    /// </summary>
    public static SentryService? Instance => _instance;

    /// <summary>
    /// Returns true if Sentry is enabled and initialized.
    /// </summary>
    public static bool IsEnabled => _instance?._sentryDisposable != null;

    public SentryService(ILogger<SentryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes the Sentry SDK with configuration from CoreConfig.
    /// </summary>
    public void Load()
    {
        _instance = this;

        if (!CoreConfig.Sentry.Enabled)
        {
            _logger.LogDebug("Sentry is disabled in configuration");
            return;
        }

        if (string.IsNullOrWhiteSpace(CoreConfig.Sentry.Dsn))
        {
            _logger.LogWarning("Sentry is enabled but no DSN is configured. Sentry will not capture errors.");
            return;
        }

        try
        {
            _sentryDisposable = SentrySdk.Init(options =>
            {
                options.Dsn = CoreConfig.Sentry.Dsn;
                options.Environment = CoreConfig.Sentry.Environment;
                options.SampleRate = (float)CoreConfig.Sentry.SampleRate;
                options.Debug = CoreConfig.Sentry.Debug;
                options.Release = $"counterstrikesharp@{Assembly.GetExecutingAssembly().GetName().Version}";
                options.IsGlobalModeEnabled = true;

                options.SetBeforeSend((sentryEvent, hint) =>
                {
                    if (CoreConfig.Sentry.IncludeServerInfo)
                    {
                        EnrichWithServerInfo(sentryEvent);
                    }
                    return sentryEvent;
                });
            });

            _logger.LogInformation("Sentry initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Sentry");
        }
    }

    /// <summary>
    /// Captures an exception to Sentry with optional scope configuration.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="configureScope">Optional action to configure the Sentry scope.</param>
    public static void CaptureException(Exception exception, Action<Scope>? configureScope = null)
    {
        if (!IsEnabled) return;

        try
        {
            SentrySdk.CaptureException(exception, scope =>
            {
                configureScope?.Invoke(scope);
            });
        }
        catch
        {
            // Silently fail - we don't want Sentry errors to affect the server
        }
    }

    /// <summary>
    /// Captures an exception from a plugin with automatic plugin context.
    /// </summary>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <param name="pluginVersion">The version of the plugin.</param>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="configureScope">Optional action to configure the Sentry scope.</param>
    public static void CapturePluginException(string? pluginName, string? pluginVersion, Exception exception, Action<Scope>? configureScope = null)
    {
        if (!IsEnabled) return;

        CaptureException(exception, scope =>
        {
            scope.SetTag("plugin", pluginName ?? "unknown");
            scope.SetTag("plugin_version", pluginVersion ?? "unknown");
            configureScope?.Invoke(scope);
        });
    }

    /// <summary>
    /// Captures an exception from a BasePlugin with automatic plugin context.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="configureScope">Optional action to configure the Sentry scope.</param>
    public static void CapturePluginException(BasePlugin plugin, Exception exception, Action<Scope>? configureScope = null)
    {
        if (!IsEnabled) return;

        CaptureException(exception, scope =>
        {
            scope.SetTag("plugin", plugin.ModuleName);
            scope.SetTag("plugin_version", plugin.ModuleVersion);

            // Call plugin's custom scope configuration if it implements IPluginSentry
            if (plugin is IPluginSentry sentryPlugin)
            {
                try
                {
                    sentryPlugin.ConfigureSentryScope(scope, exception);
                }
                catch
                {
                    // Don't let plugin's scope config break Sentry capture
                }
            }

            // Apply any additional configuration passed by caller
            configureScope?.Invoke(scope);
        });
    }

    /// <summary>
    /// Sets player context on a Sentry scope.
    /// </summary>
    /// <param name="scope">The Sentry scope.</param>
    /// <param name="player">The player controller.</param>
    public static void SetPlayerContext(Scope scope, CCSPlayerController? player)
    {
        if (player == null || !CoreConfig.Sentry.IncludePlayerContext) return;

        try
        {
            if (!player.IsValid) return;

            scope.SetTag("player_steamid", player.AuthorizedSteamID?.ToString() ?? "unknown");
            scope.SetTag("player_name", player.PlayerName ?? "unknown");
            scope.SetTag("player_team", player.Team.ToString());
            scope.SetTag("player_slot", player.Slot.ToString());
        }
        catch
        {
            // Player may become invalid during exception handling
        }
    }

    /// <summary>
    /// Registers a plugin with the Sentry service for plugin-specific DSN tracking.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <param name="config">Optional plugin config that may override the DSN.</param>
    public void RegisterPlugin(IPlugin plugin, IBasePluginConfig? config = null)
    {
        if (plugin is IPluginSentry sentrySupportedPlugin)
        {
            var dsn = sentrySupportedPlugin.SentryDsn;
            var isDefault = sentrySupportedPlugin.IsDefaultDsn;

            // Check if config overrides the DSN
            if (config is IPluginSentryConfig sentryConfig &&
                !string.IsNullOrWhiteSpace(sentryConfig.SentryDsn))
            {
                dsn = sentryConfig.SentryDsn;
                isDefault = false; // Config override is not a default
                _logger.LogDebug("Plugin '{PluginName}' Sentry DSN overridden by config", plugin.ModuleName);
            }

            if (isDefault && !string.IsNullOrWhiteSpace(dsn))
            {
                _logger.LogWarning(
                    "[Sentry] Plugin '{PluginName}' has a default/hardcoded Sentry DSN. " +
                    "Exception data may be sent to the plugin author. " +
                    "Configure 'SentryDsn' in the plugin config to override or disable.",
                    plugin.ModuleName);
            }

            if (!string.IsNullOrWhiteSpace(dsn))
            {
                _pluginSentryInfo[plugin.ModuleName] = new PluginSentryInfo(dsn, isDefault);
                _logger.LogDebug("Registered plugin '{PluginName}' with custom Sentry DSN", plugin.ModuleName);
            }
        }
    }

    /// <summary>
    /// Unregisters a plugin from the Sentry service.
    /// </summary>
    /// <param name="pluginName">The name of the plugin to unregister.</param>
    public void UnregisterPlugin(string pluginName)
    {
        _pluginSentryInfo.TryRemove(pluginName, out _);
    }

    /// <summary>
    /// Gets the Sentry info for a plugin if it has a custom DSN configured.
    /// </summary>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <returns>The plugin's Sentry info, or null if not registered.</returns>
    public PluginSentryInfo? GetPluginSentryInfo(string pluginName)
    {
        return _pluginSentryInfo.TryGetValue(pluginName, out var info) ? info : null;
    }

    private static void EnrichWithServerInfo(SentryEvent sentryEvent)
    {
        try
        {
            sentryEvent.SetTag("server_map", Server.MapName ?? "unknown");
            sentryEvent.SetTag("server_player_count", Utilities.GetPlayers()?.Count.ToString() ?? "0");
            sentryEvent.SetTag("server_max_players", Server.MaxPlayers.ToString());
            sentryEvent.SetTag("server_tickrate", Server.TickInterval.ToString("F6"));
            sentryEvent.SetTag("platform", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux");
        }
        catch
        {
            // Server info may not be available during startup/shutdown
        }
    }

    /// <summary>
    /// Disposes of the Sentry SDK resources.
    /// </summary>
    public void Dispose()
    {
        _sentryDisposable?.Dispose();
        _sentryDisposable = null;
        _instance = null;
    }
}

/// <summary>
/// Information about a plugin's Sentry configuration.
/// </summary>
public record PluginSentryInfo(string Dsn, bool IsDefault);
