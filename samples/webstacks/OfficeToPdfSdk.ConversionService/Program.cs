using FelixELabs.OfficeToPdfSdk;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);


builder
    .Services
    .AddOfficeToPdfSdk(builder.Configuration);

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

builder
    .Services
    .AddControllers();

builder
    .Services
    .AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/health", () => new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow
    })
    .WithName("HealthCheck")
    .Produces(200);

app.MapGet("/info", () => new
    {
        service = "Office to PDF Conversion Service",
        version = "1.0",
        endpoint = "http://localhost:5000",
        timestamp = DateTime.UtcNow
    })
    .WithName("ServiceInfo")
    .Produces(200);

await app.RunAsync();
