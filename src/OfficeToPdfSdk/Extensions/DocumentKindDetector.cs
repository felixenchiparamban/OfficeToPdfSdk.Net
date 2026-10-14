using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Utils;

namespace FelixELabs.OfficeToPdfSdk.Extensions;

internal static class DocumentKindDetector
{
    public static OfficeDocumentKind FromPath(string path)
    {
        return FromExtension(Path.GetExtension(path));
    }

    public static OfficeDocumentKind FromExtension(ReadOnlySpan<char> extension)
    {
        if (extension.Length > 0 && extension[0] == '.')
        {
            extension = extension[1..];
        }

        var comparison = StringComparison.OrdinalIgnoreCase;

        if (extension.Equals(Constants.FileExtensions.Doc.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Docx.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Odt.AsSpan(), comparison))
        {
            return OfficeDocumentKind.Document;
        }

        if (extension.Equals(Constants.FileExtensions.Xls.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Xlsx.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Csv.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Ods.AsSpan(), comparison))
        {
            return OfficeDocumentKind.Spreadsheet;
        }

        if (extension.Equals(Constants.FileExtensions.Ppt.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Pptx.AsSpan(), comparison) ||
            extension.Equals(Constants.FileExtensions.Odp.AsSpan(), comparison))
        {
            return OfficeDocumentKind.Presentation;
        }

        return OfficeDocumentKind.Unknown;
    }
}
