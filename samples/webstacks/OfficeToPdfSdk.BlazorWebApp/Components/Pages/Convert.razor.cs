using FelixELabs.OfficeToPdfSdk.BlazorWebApp.Services;
using FelixELabs.OfficeToPdfSdk.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace FelixELabs.OfficeToPdfSdk.BlazorWebApp.Components.Pages
{
    public partial class Convert
    {
        [Inject]
        public IConversionServiceClient ConversionClient { get; set; } = null!;

        [Inject]
        public IJSRuntime Js { get; set; } = null!;

        [Inject]
        public ILogger<Convert> Logger { get; set; } = null!;

        private sealed class UploadedFile
        {
            public string? FileName { get; init; }
            public byte[]? FileBytes { get; init; }
        }

        private UploadedFile? _uploadedFile;
        private string? _message;
        private string _alertType = "danger";
        private byte[]? _pdfBytes;
        private string? _pdfFileName;
        private bool _showDownloadButton;

        private const long MaxFileSize = 50 * 1024 * 1024;

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                var bytes = new byte[file.Size];
                using (var stream = file.OpenReadStream(MaxFileSize))
                {
                    await stream.ReadExactlyAsync(bytes);
                }

                _uploadedFile = new UploadedFile 
                { 
                    FileName = file.Name, 
                    FileBytes = bytes 
                };

                _message = $"File ready: {file.Name}";
                _alertType = "success";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading file");
                _message = $"Error uploading file: {ex.Message}";
                _alertType = "danger";
                _uploadedFile = null;
                StateHasChanged();
            }
        }

        private async Task HandleConvert()
        {
            if (_uploadedFile?.FileBytes == null)
            {
                _message = "Please select a file first";
                _alertType = "warning";
                StateHasChanged();
                return;
            }

            _message = "Converting... This may take 1-3 minutes.";
            _alertType = "info";
            StateHasChanged();

            await DoConversionAsync();
        }

        private async Task DoConversionAsync()
        {
            try
            {
                var fileName = _uploadedFile?.FileName ?? "document";
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                
                var documentKind = extension switch
                {
                    ".doc" or ".docx" or ".odt" => OfficeDocumentKind.Document,
                    ".xls" or ".xlsx" or ".ods" => OfficeDocumentKind.Spreadsheet,
                    ".ppt" or ".pptx" or ".odp" => OfficeDocumentKind.Presentation,
                    _ => throw new InvalidOperationException($"Unsupported file type: {extension}")
                };

                var fileBytes = _uploadedFile?.FileBytes;
                if (fileBytes == null) throw new InvalidOperationException("File bytes are null");

                Logger.LogInformation("Starting conversion for {FileName} ({Size} bytes)", fileName, fileBytes.Length);

                // Use the HTTP client to convert via OfficeToPdfSdk.ConversionService
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
                var pdfBytes = await ConversionClient.ConvertAsync(fileBytes, documentKind, fileName, cts.Token);
                
                var pdfFileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";

                _pdfBytes = pdfBytes;
                _pdfFileName = pdfFileName;
                _message = "File ready! Click the download button below.";
                _alertType = "success";
                _showDownloadButton = true;
                
                Logger.LogInformation("Conversion successful. PDF size: {Size} bytes", pdfBytes.Length);
                StateHasChanged();
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Conversion timed out");
                _message = "Conversion timed out - file may be too large.";
                _alertType = "danger";
                StateHasChanged();
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogError(ex, "Conversion service error");
                _message = $"Conversion error: {ex.Message}";
                _alertType = "danger";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected conversion error");
                _message = $"Error: {ex.Message}";
                _alertType = "danger";
                StateHasChanged();
            }
        }

        private async Task DownloadPdf()
        {
            if (_pdfBytes != null && !string.IsNullOrEmpty(_pdfFileName))
            {
                await Js.InvokeVoidAsync("downloadFile", _pdfBytes, _pdfFileName);
                _uploadedFile = null;
                _pdfBytes = null;
                _pdfFileName = null;
                _showDownloadButton = false;
                _message = null;
                StateHasChanged();
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = ["B", "KB", "MB"];
            double len = bytes;
            var order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}



