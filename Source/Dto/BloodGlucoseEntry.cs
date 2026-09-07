// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Serialization;

namespace BloodSugarBot.Dto;

#pragma warning disable format

public sealed record BloodGlucoseEntry
(
    [ property: JsonPropertyName("_id")        ] string  Id,
    [ property: JsonPropertyName("sysTime")    ] string? SysTime,
    [ property: JsonPropertyName("type")       ] string  Type,
    [ property: JsonPropertyName("date")       ] long?   Date,
    [ property: JsonPropertyName("dateString") ] string? DateString,
    [ property: JsonPropertyName("delta")      ] decimal Delta,
    [ property: JsonPropertyName("device")     ] string  Device,
    [ property: JsonPropertyName("direction")  ] string  Direction,
    [ property: JsonPropertyName("filtered")   ] int     Filtered,
    [ property: JsonPropertyName("noise")      ] int     Noise,
    [ property: JsonPropertyName("rssi")       ] int     Rssi,
    [ property: JsonPropertyName("sgv")        ] int     Sgv,
    [ property: JsonPropertyName("unfiltered") ] int     Unfiltered,
    [ property: JsonPropertyName("utcOffset")  ] int     UtcOffset,
    [ property: JsonPropertyName("mills")      ] long?   Mills
);

#pragma warning restore format
