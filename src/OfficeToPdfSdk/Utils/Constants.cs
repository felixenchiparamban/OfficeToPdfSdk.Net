using System.Reflection;

namespace FelixELabs.OfficeToPdfSdk.Utils;

public static class Constants
{
    public static string AppName => typeof(Constants).Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "OfficeToPdfSdk";

    public static class FileExtensions
    {
        // Document extensions
        public const string Doc = "doc";
        public const string Docx = "docx";
        public const string Odt = "odt";

        // Spreadsheet extensions
        public const string Xls = "xls";
        public const string Xlsx = "xlsx";
        public const string Csv = "csv";
        public const string Ods = "ods";

        // Presentation extensions
        public const string Ppt = "ppt";
        public const string Pptx = "pptx";
        public const string Odp = "odp";
    }
}
