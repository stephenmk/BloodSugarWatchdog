// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using System.Net.Http.Json;
using BloodSugarBot.Dto;

namespace BloodSugarBot.Nightscout;

internal sealed class NightscoutHttpClient(HttpClient httpClient)
{
    public Task<ImmutableArray<BloodGlucoseEntry>> GetEntriesAsync(CancellationToken ct)
        => httpClient.GetFromJsonAsync<ImmutableArray<BloodGlucoseEntry>>("entries.json", ct);

    public Task<ImmutableArray<BloodGlucoseTreatment>> GetTreatmentsAsync(CancellationToken ct)
        => httpClient.GetFromJsonAsync<ImmutableArray<BloodGlucoseTreatment>>("treatments.json", ct);
}
