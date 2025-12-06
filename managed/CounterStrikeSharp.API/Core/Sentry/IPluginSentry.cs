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
/// Interface for plugins that want to provide their own Sentry DSN for error tracking.
/// </summary>
/// <remarks>
/// <para>
/// <b>Current Limitation:</b> Due to Sentry SDK architecture, only a single DSN can be active
/// at a time. Plugin DSNs are currently registered for informational purposes and will trigger
/// a warning to server operators, but exceptions are still sent to the core DSN.
/// Full multi-DSN support may be added in a future version.
/// </para>
/// <para>
/// If <see cref="IsDefaultDsn"/> is true and the DSN is not empty, a warning will be shown
/// to server operators that the plugin author has configured error tracking.
/// </para>
/// <para>
/// For now, plugin authors who want dedicated error tracking should:
/// <list type="number">
/// <item>Use the extension method <c>this.CaptureException()</c> to manually capture exceptions</item>
/// <item>Add custom tags via the scope configuration to identify your plugin</item>
/// <item>Consider setting up Sentry issue routing rules based on the <c>plugin</c> tag</item>
/// </list>
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
    /// Called to add custom context, tags, or user information to a Sentry event.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> This method is not automatically called by the core exception handlers.
    /// To use this, call it manually when using <c>this.CaptureException()</c>:
    /// <code>
    /// this.CaptureException(ex, scope => ConfigureSentryScope(scope, ex));
    /// </code>
    /// </remarks>
    /// <param name="scope">The Sentry scope to configure.</param>
    /// <param name="exception">The exception being captured.</param>
    void ConfigureSentryScope(Scope scope, Exception exception);
}
