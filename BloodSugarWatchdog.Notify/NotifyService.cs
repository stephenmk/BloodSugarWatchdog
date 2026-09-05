// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Notify;

public interface INotifyService
{
    Task<bool> PostImageAsync(string path, string caption, CancellationToken ct = default);
}

internal sealed partial class NotifyService
(
    ILogger<NotifyService> logger,
    HttpClient httpClient,
    IOptions<NotifyOptions> options
) :
    INotifyService
{
    public async Task<bool> PostImageAsync(string path, string caption, CancellationToken ct)
    {
        var opt = options.Value;
        using var content = new MultipartFormDataContent();

        // Caption.
        content.Add(new StringContent(caption), opt.ImageCaptionKey);

        // Image.
        await using var fileStream = File.OpenRead(path);
        content.Add(new StreamContent(fileStream), opt.ImageContentKey, Path.GetFileName(path));

        // Other form properties.
        foreach (var (key, val) in opt.FormDataContent)
            content.Add(new StringContent(val), key);

        try
        {
            using var response = await httpClient.PostAsync(opt.ImageRequestUri, content, ct);

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
