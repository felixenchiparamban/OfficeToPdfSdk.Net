using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Extensions;

namespace FelixELabs.OfficeToPdfSdk.UnitTests.Extensions;

public class DocumentKindDetectorTests
{
    [Theory]
    [InlineData("document.doc", OfficeDocumentKind.Document)]
    [InlineData("document.docx", OfficeDocumentKind.Document)]
    [InlineData("spreadsheet.xls", OfficeDocumentKind.Spreadsheet)]
    [InlineData("spreadsheet.xlsx", OfficeDocumentKind.Spreadsheet)]
    [InlineData("data.csv", OfficeDocumentKind.Spreadsheet)]
    [InlineData("presentation.ppt", OfficeDocumentKind.Presentation)]
    [InlineData("presentation.pptx", OfficeDocumentKind.Presentation)]
    public void FromPathWithValidExtensionReturnsCorrectDocumentKind(string path, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromPath(path);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("file.txt", OfficeDocumentKind.Unknown)]
    [InlineData("file.pdf", OfficeDocumentKind.Unknown)]
    [InlineData("file.jpg", OfficeDocumentKind.Unknown)]
    [InlineData("file.unknown", OfficeDocumentKind.Unknown)]
    [InlineData("file", OfficeDocumentKind.Unknown)]
    public void FromPathWithUnknownExtensionReturnsUnknown(string path, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromPath(path);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(".doc", OfficeDocumentKind.Document)]
    [InlineData(".DOCX", OfficeDocumentKind.Document)]
    [InlineData(".Doc", OfficeDocumentKind.Document)]
    [InlineData("doc", OfficeDocumentKind.Document)]
    [InlineData("DOCX", OfficeDocumentKind.Document)]
    public void FromPathWithCaseInsensitiveExtensionReturnsCorrectDocumentKind(string extension, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromExtension(extension);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(".xls", OfficeDocumentKind.Spreadsheet)]
    [InlineData(".XLSX", OfficeDocumentKind.Spreadsheet)]
    [InlineData(".csv", OfficeDocumentKind.Spreadsheet)]
    [InlineData("csv", OfficeDocumentKind.Spreadsheet)]
    public void FromExtensionWithSpreadsheetExtensionsReturnsSpreadsheet(string extension, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromExtension(extension);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(".ppt", OfficeDocumentKind.Presentation)]
    [InlineData(".PPTX", OfficeDocumentKind.Presentation)]
    [InlineData("ppt", OfficeDocumentKind.Presentation)]
    [InlineData("PPTX", OfficeDocumentKind.Presentation)]
    public void FromExtensionWithPresentationExtensionsReturnsPresentation(string extension, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromExtension(extension);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData(".txt")]
    [InlineData(".unknown")]
    [InlineData("xyz")]
    public void FromExtensionWithUnknownExtensionReturnsUnknown(string extension)
    {
        // Act
        var result = DocumentKindDetector.FromExtension(extension);

        // Assert
        Assert.Equal(OfficeDocumentKind.Unknown, result);
    }

    [Fact]
    public void FromPathWithComplexPathDetectsExtensionCorrectly()
    {
        // Act
        var result = DocumentKindDetector.FromPath("/path/to/my.document.docx");

        // Assert
        Assert.Equal(OfficeDocumentKind.Document, result);
    }

    [Fact]
    public void FromPathWithPathContainingDotsUsesLastExtension()
    {
        // Act
        var result = DocumentKindDetector.FromPath("my.backup.docx");

        // Assert
        Assert.Equal(OfficeDocumentKind.Document, result);
    }

    [Theory]
    [InlineData("document.DOC", OfficeDocumentKind.Document)]
    [InlineData("SPREADSHEET.XLSX", OfficeDocumentKind.Spreadsheet)]
    [InlineData("PRESENTATION.PPTX", OfficeDocumentKind.Presentation)]
    public void FromPathWithUppercaseExtensionReturnsCorrectKind(string path, OfficeDocumentKind expected)
    {
        // Act
        var result = DocumentKindDetector.FromPath(path);

        // Assert
        Assert.Equal(expected, result);
    }
}

