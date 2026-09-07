// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Serialization;

namespace BloodSugarBot.Dto;

#pragma warning disable format

public sealed record BloodGlucoseTreatment
(
    [ property: JsonPropertyName("_id")               ] string  Id,
    [ property: JsonPropertyName("timestamp")         ] long?   Timestamp,
    [ property: JsonPropertyName("eventType")         ] string  EventType,
    [ property: JsonPropertyName("enteredBy")         ] string? EnteredBy,
    [ property: JsonPropertyName("uuid")              ] string? Uuid,
    [ property: JsonPropertyName("insulin")           ] double? Insulin,
    [ property: JsonPropertyName("insulinType")       ] string? InsulinType,
    [ property: JsonPropertyName("insulinInjections") ] string? InsulinInjections,
    [ property: JsonPropertyName("created_at")        ] string? CreatedAt,
    [ property: JsonPropertyName("sysTime")           ] string? SysTime,
    [ property: JsonPropertyName("utcOffset")         ] int     UtcOffset,
    [ property: JsonPropertyName("carbs")             ] double? Carbs,
    [ property: JsonPropertyName("mills")             ] long?   Mills,
    [ property: JsonPropertyName("date")              ] long?   Date,
    [ property: JsonPropertyName("notes")             ] string? Notes
);

#pragma warning restore format
