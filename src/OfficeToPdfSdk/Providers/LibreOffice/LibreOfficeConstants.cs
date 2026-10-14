namespace FelixELabs.OfficeToPdfSdk.Providers.LibreOffice;

internal static class LibreOfficeConstants
{
    /// <summary>
    /// Common non-standard LibreOffice installation paths tried when "soffice" is not on PATH.
    /// </summary>
    public static class DefaultPaths
    {
        public static readonly string[] Linux =
        [
            "/usr/bin/soffice",
            "/usr/lib/libreoffice/program/soffice",
            "/opt/libreoffice/program/soffice",
        ];

        public static readonly string[] MacOs =
        [
            "/Applications/LibreOffice.app/Contents/MacOS/soffice",
            "/opt/homebrew/bin/soffice",
        ];

        public static readonly string[] Windows =
        [
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
        ];
    }
}

