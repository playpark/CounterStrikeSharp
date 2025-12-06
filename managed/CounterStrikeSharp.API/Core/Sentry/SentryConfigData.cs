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
/// Configuration data for Sentry error tracking integration.
/// </summary>
public sealed class SentryConfigData
{
    /// <summary>
    /// Enable or disable Sentry error tracking. Disabled by default.
    /// </summary>
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// The Sentry DSN (Data Source Name) for your project.
    /// Get this from your Sentry project settings.
    /// </summary>
    [JsonPropertyName("Dsn")]
    public string Dsn { get; set; } = "";

    /// <summary>
    /// Environment tag for Sentry events (e.g., "production", "staging", "development").
    /// </summary>
    [JsonPropertyName("Environment")]
    public string Environment { get; set; } = "production";

    /// <summary>
    /// Sample rate for error capture (0.0 to 1.0).
    /// 1.0 = capture all errors, 0.5 = capture 50% of errors.
    /// </summary>
    [JsonPropertyName("SampleRate")]
    public double SampleRate { get; set; } = 1.0;

    /// <summary>
    /// Include server information (map, player count, etc.) in Sentry events.
    /// </summary>
    [JsonPropertyName("IncludeServerInfo")]
    public bool IncludeServerInfo { get; set; } = true;

    /// <summary>
    /// Include player context (SteamID, name) in Sentry events.
    /// Consider privacy implications when enabling this option.
    /// </summary>
    [JsonPropertyName("IncludePlayerContext")]
    public bool IncludePlayerContext { get; set; } = true;

    /// <summary>
    /// Enable Sentry SDK debug logging. Useful for troubleshooting.
    /// </summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; } = false;
}
