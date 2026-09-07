// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarBot.Notify;

internal sealed class NotifyOptions
{
    public const string ConfigSectionPath = nameof(NotifyService);

    public required string HttpClientUserAgent { get; init; }
    public required string ApiEndpoint { get; init; }

    public required string ImageCaptionKey { get; init; }
    public required string ImageRequestUri { get; init; }
    public required string ImageContentKey { get; init; }

    public required Dictionary<string, string> FormDataContent { get; init; }
}
