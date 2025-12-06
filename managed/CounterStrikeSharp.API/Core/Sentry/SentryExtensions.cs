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
using Sentry;

namespace CounterStrikeSharp.API.Core.Sentry;

/// <summary>
/// Extension methods for plugins to easily interact with Sentry error tracking.
/// </summary>
public static class SentryExtensions
{
    /// <summary>
    /// Captures an exception to Sentry with automatic plugin context.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="configureScope">Optional action to add additional context to the Sentry scope.</param>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some risky operation
    /// }
    /// catch (Exception ex)
    /// {
    ///     this.CaptureException(ex);
    /// }
    /// </code>
    /// </example>
    public static void CaptureException(this BasePlugin plugin, Exception exception, Action<Scope>? configureScope = null)
    {
        SentryService.CapturePluginException(plugin, exception, configureScope);
    }

    /// <summary>
    /// Adds a breadcrumb to the current Sentry scope.
    /// Breadcrumbs are trail of events that happened prior to an issue.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">Optional category for the breadcrumb. Defaults to the plugin name.</param>
    /// <param name="level">The breadcrumb level. Defaults to Info.</param>
    /// <example>
    /// <code>
    /// this.AddBreadcrumb("Player connected", "players", BreadcrumbLevel.Info);
    /// this.AddBreadcrumb("Round started", "game", BreadcrumbLevel.Info);
    /// </code>
    /// </example>
    public static void AddBreadcrumb(this BasePlugin plugin, string message, string? category = null, BreadcrumbLevel level = BreadcrumbLevel.Info)
    {
        if (!SentryService.IsEnabled) return;

        try
        {
            SentrySdk.AddBreadcrumb(
                message: message,
                category: category ?? plugin.ModuleName,
                level: level
            );
        }
        catch
        {
            // Silently fail - we don't want Sentry errors to affect the server
        }
    }

    /// <summary>
    /// Adds a breadcrumb with additional data to the current Sentry scope.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">Optional category for the breadcrumb. Defaults to the plugin name.</param>
    /// <param name="data">Additional data to attach to the breadcrumb.</param>
    /// <param name="level">The breadcrumb level. Defaults to Info.</param>
    public static void AddBreadcrumb(this BasePlugin plugin, string message, string? category, System.Collections.Generic.IDictionary<string, string>? data, BreadcrumbLevel level = BreadcrumbLevel.Info)
    {
        if (!SentryService.IsEnabled) return;

        try
        {
            SentrySdk.AddBreadcrumb(
                message: message,
                category: category ?? plugin.ModuleName,
                data: data,
                level: level
            );
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Captures a message to Sentry with automatic plugin context.
    /// Use this for non-exception events you want to track.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level. Defaults to Info.</param>
    /// <example>
    /// <code>
    /// this.CaptureMessage("Plugin initialized successfully", SentryLevel.Info);
    /// this.CaptureMessage("Unexpected state detected", SentryLevel.Warning);
    /// </code>
    /// </example>
    public static void CaptureMessage(this BasePlugin plugin, string message, SentryLevel level = SentryLevel.Info)
    {
        if (!SentryService.IsEnabled) return;

        try
        {
            SentrySdk.CaptureMessage(message, scope =>
            {
                scope.SetTag("plugin", plugin.ModuleName);
                scope.SetTag("plugin_version", plugin.ModuleVersion);
            }, level);
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Sets a tag on the current Sentry scope.
    /// Tags are key-value pairs that can be used to filter and search events.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    public static void SetSentryTag(this BasePlugin plugin, string key, string value)
    {
        if (!SentryService.IsEnabled) return;

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag(key, value);
            });
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Sets extra context data on the current Sentry scope.
    /// Extra data provides additional context that can help with debugging.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="key">The extra data key.</param>
    /// <param name="value">The extra data value (will be serialized).</param>
    public static void SetSentryExtra(this BasePlugin plugin, string key, object? value)
    {
        if (!SentryService.IsEnabled) return;

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetExtra(key, value);
            });
        }
        catch
        {
            // Silently fail
        }
    }
}
