# OfficeToPdfSdk.Net

A lightweight, cross-platform .NET 10 SDK for converting Office documents to PDF. Works with LibreOffice under the hood, runs on Linux.

## Why You Might Need This

You have a .NET application that needs to convert Word, Excel, or PowerPoint files to PDF. Maybe you're building a document management system, an automation service, or you need to generate PDFs in batch. This SDK keeps it simple: you give it a file, you get a PDF back.

It handles the tricky bits like:
- Detecting document types automatically
- Running LibreOffice headlessly without UI
- Managing file I/O efficiently (streaming, not loading everything into memory)
- Proper error handling and logging
- Clean dependency injection patterns for .NET apps

## Quick Start

### Installation

```bash
dotnet add package OfficeToPdfSdk
```

### Basic Usage

```csharp
// Register the SDK in your DI container
builder.Services.AddOfficeToPdfSdk(builder.Configuration);

// Inject and use
public class DocumentProcessor
{
    private readonly IOfficeToPdfConverter _converter;
    
    public DocumentProcessor(IOfficeToPdfConverter converter)
    {
        _converter = converter;
    }
    
    public async Task ConvertDocumentAsync()
    {
        await _converter.ConvertAsync(
            inputPath: "document.docx",
            outputPath: "document.pdf"
        );
    }
}
```

### Configuration

Add this to your `appsettings.json`:

```json
{
  "OfficeToPdfModuleOptions": {
    "IsEnabled": true,
    "TemporaryFolderPath": "/tmp/office-conversions"
  }
}
```

The temporary folder is where the SDK caches files during conversion. If not set, it uses the system temp directory.

## Supported Formats

**Input:**
- Documents: `.doc`, `.docx`, `.odt`
- Spreadsheets: `.xls`, `.xlsx`, `.ods`
- Presentations: `.ppt`, `.pptx`, `.odp`

**Output:**
- PDF (`.pdf`)

## Deployment Options

### 1. In-Process (Simple Apps)

Just add the SDK to your ASP.NET Core or console app. Conversions run in the same process.

```csharp
builder.Services.AddOfficeToPdfSdk(builder.Configuration);
```

**Pros:** Simple, no network overhead  
**Cons:** LibreOffice process impacts your app; conversions block while running

### 2. Microservice (Recommended for Web)

Run a dedicated conversion service and call it over HTTP. See `samples/webstacks/OfficeToPdfSdk.ConversionService/` for a complete example.

```bash
cd samples/webstacks/OfficeToPdfSdk.ConversionService
dotnet run
```

Call it from your main app:

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var response = await client.PostAsync("/api/convert", content);
```

**Pros:** Isolated, scalable, supports async workloads  
**Cons:** Network latency, requires separate infrastructure

## Real-World Example: Blazor Web App

Check out `samples/webstacks/OfficeToPdfSdk.BlazorWebApp/` for a complete interactive example:

1. Upload a document
2. Click convert
3. Download the PDF

It demonstrates:
- File upload via Blazor `InputFile`
- Long-running conversions with proper timeout config
- HTTP client integration with the conversion service
- Error handling and user feedback


## License

MIT License — see [LICENSE](LICENSE) for the full text. You're free to use this in commercial projects.

---

**Built with:** .NET 10, LibreOffice, xUnit  
**Status:** Actively maintained
