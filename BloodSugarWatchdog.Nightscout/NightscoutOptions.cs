// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Nightscout;

public sealed record NightscoutOptions
{
    public const string ConfigSectionPath = nameof(NightscoutService);

    public required string ApiEndpoint { get; init; }
    public required string HttpClientUserAgent { get; init; }
    public required string Username { get; init; }
}
