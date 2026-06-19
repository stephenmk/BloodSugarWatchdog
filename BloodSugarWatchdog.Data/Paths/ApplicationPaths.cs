// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Data.Sqlite;

namespace BloodSugarWatchdog.Data.Paths;

public static class ApplicationPaths
{
    public static string GetSqliteConnectionString(string username)
    {
        var dirPath = Path.Join(EnvironmentPaths.LocalDataPath, "BloodSugarWatchdog", username);

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Join(dirPath, "data.db"),
            Pooling = true,
        };

        return builder.ToString();
    }
}
