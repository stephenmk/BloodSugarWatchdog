// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Notify;

public interface INotifyService
{
    Task<bool> PostImageAsync(string path, string caption, CancellationToken ct);
}

internal sealed partial class NotifyService
(
    ILogger<NotifyService> logger,
    HttpClient httpClient,
    NotifyOptions options
) :
    INotifyService
{
    public async Task<bool> PostImageAsync(string path, string caption, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();

        // Caption.
        content.Add(new StringContent(caption), options.ImageCaptionKey);

        // Image.
        await using var fileStream = File.OpenRead(path);
        content.Add(new StreamContent(fileStream), options.ImageContentKey, Path.GetFileName(path));

        // Other form properties.
        foreach (var (key, val) in options.FormDataContent)
            content.Add(new StringContent(val), key);

        try
        {
            using var response = await httpClient.PostAsync(options.ImageRequestUri, content, ct);

            if (response.IsSuccessStatusCode)
                return true;

            LogStatusFailure(response.StatusCode.ToString());
        }
        catch (HttpRequestException ex)
        {
            LogRequestError(ex.Message);
        }

        return false;
    }

    [LoggerMessage(LogLevel.Warning, "Notification API returned http status code {Code}")]
    partial void LogStatusFailure(string code);

    [LoggerMessage(LogLevel.Warning, "Notification API http request error: {Message}")]
    partial void LogRequestError(string message);
}
