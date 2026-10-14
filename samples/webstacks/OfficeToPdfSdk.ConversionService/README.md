# OfficeToPdfSdk.ConversionService

A cross-platform REST API service for converting Office documents to PDF.

## Overview

This is a sample ASP.NET Core web application that demonstrates how to use the `OfficeToPdfSdk.Hosting` library to expose document conversion capabilities via HTTP REST endpoints.

### Architecture

```
Client (Blazor, Desktop, etc.)
           ↓
    HTTP REST API
           ↓
OfficeToPdfSdk.ConversionService
           ↓
OfficeToPdfSdk.Hosting (HttpConversionServer)
           ↓
OfficeToPdfSdk (Core)
           ↓
Office Conversion Providers
```

## Prerequisites

### Windows
- .NET 10.0 SDK
- Office (Microsoft Word, Excel, PowerPoint) installed

### Linux/Docker
- .NET 10.0 runtime
- LibreOffice installed (included in Docker image)

### macOS
- .NET 10.0 SDK
- Office (Microsoft Office for Mac) or LibreOffice

## Getting Started

### Local Development

1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

2. **Run the service:**
   ```bash
   dotnet run
   ```

3. **Access Swagger UI:**
   - Open browser to `http://localhost:5000`
   - Swagger documentation available at `http://localhost:5000/swagger`

### Docker

1. **Build the image:**
   ```bash
   docker build -t office-to-pdf-service:latest .
   ```

2. **Run the container:**
   ```bash
   docker run -p 5000:5000 office-to-pdf-service:latest
   ```

3. **Test the service:**
   ```bash
   curl http://localhost:5000/health
   ```

### Docker Compose

The recommended way to run the service in a container.

```bash
# From the samples/webstacks/ directory
docker compose up --build
```

The service starts on `http://localhost:5000`. Wait for the health check to pass (LibreOffice initialises on first run — ~60 s), then test:

```bash
# 1. Health check
curl http://localhost:5000/health

# 2. Service info
curl http://localhost:5000/info

# 3. Convert a document (file path inside the container)
curl -s -X POST http://localhost:5000/api/conversion \
  -H "Content-Type: application/json" \
  -d '{
    "filePath": "/app/documents/sample.docx",
    "documentKind": 0,
    "outputFilePath": "/app/documents/output.pdf"
  }'
```

To watch logs:
```bash
docker compose logs -f conversion-service
```

To stop:
```bash
docker compose down
```

## API Endpoints

### Health Check
```
GET /health
```
Returns service health status.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-05-22T12:00:00Z"
}
```

### Service Information
```
GET /info
```
Returns service information and endpoint details.

**Response:**
```json
{
  "service": "Office to PDF Conversion Service",
  "version": "1.0",
  "endpoint": "http://0.0.0.0:5000",
  "timestamp": "2026-05-22T12:00:00Z"
}
```

### Convert Document
```
POST /api/conversion
Content-Type: application/json
```

Converts an office document to PDF.

**Request Body:**
```json
{
  "fileBytes": "BASE64_ENCODED_DOCUMENT",
  "documentKind": 0,
  "fileName": "document.docx"
}
```

**Request Parameters:**
- `fileBytes` (string, required) - Document bytes encoded as Base64
- `documentKind` (integer, required) - Document type:
  - `0` = Word (.doc, .docx)
  - `1` = Excel (.xls, .xlsx)
  - `2` = PowerPoint (.ppt, .pptx)
- `fileName` (string, optional) - Original file name for logging

**Success Response (200 OK):**
```json
{
  "success": true,
  "pdfBytes": "BASE64_ENCODED_PDF",
  "error": null,
  "stackTrace": null
}
```

**Error Response (400/500):**
```json
{
  "success": false,
  "pdfBytes": null,
  "error": "Error message describing what went wrong",
  "stackTrace": null
}
```

### Supported Document Types
```
GET /api/conversion/supported-types
```

Returns list of supported document types.

**Response:**
```json
[
  {
    "kind": "Word",
    "value": 0
  },
  {
    "kind": "Excel",
    "value": 1
  },
  {
    "kind": "PowerPoint",
    "value": 2
  }
]
```

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Server": {
    "Url": "http://0.0.0.0:5000"
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Production)
- `ASPNETCORE_URLS` - Service URLs (default: http://+:5000)
- `Logging__LogLevel__Default` - Log level

## Usage Examples

### cURL
```bash
# Create a test file and encode it
BASE64_FILE=$(base64 -w 0 < document.docx)

# Send conversion request
curl -X POST http://localhost:5000/api/conversion \
  -H "Content-Type: application/json" \
  -d "{
    \"fileBytes\": \"$BASE64_FILE\",
    \"documentKind\": 0,
    \"fileName\": \"document.docx\"
  }" \
  -o response.json

# Decode response PDF
jq -r '.pdfBytes' response.json | base64 -d > output.pdf
```

### C# HttpClient
```csharp
using var client = new HttpClient();

var request = new
{
    fileBytes = Convert.ToBase64String(fileBytes),
    documentKind = 0,
    fileName = "document.docx"
};

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/conversion",
    request
);

var result = await response.Content.ReadAsAsync<ConversionResponse>();

if (result.Success)
{
    var pdfBytes = Convert.FromBase64String(result.PdfBytes);
    File.WriteAllBytes("output.pdf", pdfBytes);
}
```

### JavaScript/Fetch
```javascript
async function convertDocument(fileBytes, fileName) {
    const response = await fetch('http://localhost:5000/api/conversion', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            fileBytes: btoa(String.fromCharCode.apply(null, fileBytes)),
            documentKind: 0,
            fileName: fileName
        })
    });

    const result = await response.json();
    
    if (result.success) {
        const pdfBytes = atob(result.pdfBytes);
        const buffer = new Uint8Array(pdfBytes.length);
        for (let i = 0; i < pdfBytes.length; i++) {
            buffer[i] = pdfBytes.charCodeAt(i);
        }
        return buffer;
    } else {
        throw new Error(result.error);
    }
}
```

## Performance

- **First conversion:** 1-2 seconds (Office COM initialization)
- **Typical conversion:** 2-5 seconds
- **Large files (>100MB):** 30+ seconds
- **Timeout:** 5 minutes by default

## Platform Support

| Platform | Status | Provider | Notes |
|----------|--------|----------|-------|
| Windows | ✅ Full | Microsoft Office | Uses COM, requires Office installed |
| Linux | ✅ Full | LibreOffice | Included in Docker image |
| macOS | ✅ Full | Office/LibreOffice | Requires manual setup |

## Troubleshooting

### "Office provider unavailable" on Linux/Docker

The Dockerfile includes LibreOffice. If you're running outside Docker, install it:

```bash
# Ubuntu/Debian
apt-get install libreoffice-writer libreoffice-calc libreoffice-impress

# CentOS/RHEL
yum install libreoffice-writer libreoffice-calc libreoffice-impress

# macOS
brew install libreoffice
```

### High Memory Usage

Office/LibreOffice processes can consume significant memory. For production:
- Run multiple instances behind a load balancer
- Use Docker with memory limits
- Monitor resource usage

### Request Timeout

For large files, increase the timeout:

**In Program.cs:**
```csharp
builder.Services.Configure<FormOptions>(x => {
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
});
```

## Deployment

### Windows Service

To run as a Windows Service using the included Windows service installer:

See `OfficeToPdfSdk.NamedPipeService` sample for Windows Service example.

### Docker Production

```bash
docker run \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Logging__LogLevel__Default=Warning \
  --memory=2g \
  --restart=unless-stopped \
  office-to-pdf-service:latest
```

### Kubernetes

Example deployment manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: office-to-pdf-service
spec:
  replicas: 2
  selector:
    matchLabels:
      app: office-to-pdf-service
  template:
    metadata:
      labels:
        app: office-to-pdf-service
    spec:
      containers:
      - name: service
        image: office-to-pdf-service:latest
        ports:
        - containerPort: 5000
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "2000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
```

## Related Projects

- **OfficeToPdfSdk** - Core conversion library
- **OfficeToPdfSdk.Hosting** - Server abstraction layer
- **OfficeToPdfSdk.BlazorWebApp** - Client sample
- **OfficeToPdfSdk.NamedPipeService** - Windows service sample

## License

Same as OfficeToPdfSdk

## Support

For issues or questions, refer to the main OfficeToPdfSdk documentation.
