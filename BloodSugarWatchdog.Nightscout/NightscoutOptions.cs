// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Nightscout;

public sealed class NightscoutOptions
{
    public string Username { get; set; } = string.Empty;
    public string HttpClientUserAgent { get; set; } = string.Empty;
}
