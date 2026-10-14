namespace FelixELabs.OfficeToPdfSdk.Providers.LibreOffice;

internal static class LibreOfficeApplicationDetector
{
    /// <summary>
    /// Returns the resolved path to the soffice executable, or null if LibreOffice is not found.
    /// Checks the provided <paramref name="executablePath"/> first (which may be a bare command
    /// name like "soffice" resolved via PATH), then falls back to well-known installation paths.
    /// </summary>
    public static string? ResolveExecutable(string executablePath)
    {
        if (IsExecutable(executablePath))
        {
            return executablePath;
        }

        var fallbacks = GetFallbackPaths();
        foreach (var path in fallbacks)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static bool IsExecutable(string path)
    {
        if (path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar))
        {
            return File.Exists(path);
        }

        // Bare command name — check if it is resolvable via PATH
        var which = OperatingSystem.IsWindows() ? "where" : "which";

        try
        {
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = which,
                Arguments = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process?.WaitForExit(3000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static string[] GetFallbackPaths()
    {
        if (OperatingSystem.IsWindows())
        {
            return LibreOfficeConstants.DefaultPaths.Windows;
        }

        if (OperatingSystem.IsMacOS())
        {
            return LibreOfficeConstants.DefaultPaths.MacOs;
        }

        return LibreOfficeConstants.DefaultPaths.Linux;
    }
}

