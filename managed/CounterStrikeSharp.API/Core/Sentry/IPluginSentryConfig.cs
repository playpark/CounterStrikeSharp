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

using System.Text.Json.Serialization;

namespace CounterStrikeSharp.API.Core.Sentry;

/// <summary>
/// Interface for plugin configuration classes that want to allow server operators
/// to configure a custom Sentry DSN.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface in your plugin's configuration class to allow server operators
/// to override the plugin's default Sentry DSN.
/// </para>
/// <para>
/// The DSN can be set in the plugin's configuration file (JSON or TOML).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyPluginConfig : BasePluginConfig, IPluginSentryConfig
/// {
///     [JsonPropertyName("SentryDsn")]
///     public string? SentryDsn { get; set; } = null;
///
///     // ... other config properties
/// }
/// </code>
/// </example>
public interface IPluginSentryConfig
{
    /// <summary>
    /// The Sentry DSN (Data Source Name) configured by the server operator.
    /// If set, this will override the plugin's default DSN (if any).
    /// Set to null or empty string to disable plugin-specific Sentry reporting,
    /// or to use the core Sentry DSN.
    /// </summary>
    [JsonPropertyName("SentryDsn")]
    string? SentryDsn { get; set; }
}
