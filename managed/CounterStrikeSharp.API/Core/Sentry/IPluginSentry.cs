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
/// Interface for plugins that want to provide their own Sentry DSN.
/// Implement this interface to send exceptions from your plugin to a custom Sentry project.
/// </summary>
/// <remarks>
/// <para>
/// If <see cref="IsDefaultDsn"/> is true and the DSN is not empty, a warning will be shown
/// to server operators that exception data may be sent to the plugin author.
/// </para>
/// <para>
/// Server operators can override the plugin's DSN by implementing <see cref="IPluginSentryConfig"/>
/// and providing a <c>SentryDsn</c> property in the plugin's configuration file.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyPlugin : BasePlugin, IPluginSentry
/// {
///     public string? SentryDsn => "https://xxx@xxx.ingest.sentry.io/xxx";
///     public bool IsDefaultDsn => true; // This is a hardcoded DSN
///
///     public void ConfigureSentryScope(Scope scope, Exception exception)
///     {
///         scope.SetTag("my_custom_tag", "value");
///     }
/// }
/// </code>
/// </example>
public interface IPluginSentry
{
    /// <summary>
    /// The Sentry DSN (Data Source Name) for this plugin.
    /// If provided, exceptions from this plugin will be sent to this DSN.
    /// Return null or empty string to use the core Sentry DSN (if configured).
    /// </summary>
    string? SentryDsn { get; }

    /// <summary>
    /// Indicates whether the DSN is a default/hardcoded value provided by the plugin author.
    /// If true and the DSN is not empty, a warning will be shown to server operators
    /// that exception data may be sent to the plugin author.
    /// </summary>
    /// <remarks>
    /// Set this to false if you're reading the DSN from a configuration file
    /// that the server operator has explicitly configured.
    /// </remarks>
    bool IsDefaultDsn { get; }

    /// <summary>
    /// Called when Sentry captures an exception for this plugin.
    /// Use this method to add custom context, tags, or user information to the Sentry event.
    /// </summary>
    /// <param name="scope">The Sentry scope to configure.</param>
    /// <param name="exception">The exception being captured.</param>
    void ConfigureSentryScope(Scope scope, Exception exception);
}
