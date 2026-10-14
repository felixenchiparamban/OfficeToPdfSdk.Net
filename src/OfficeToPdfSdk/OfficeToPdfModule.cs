using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OfficeToPdfSdk.UnitTests")]

namespace FelixELabs.OfficeToPdfSdk;

/// <summary>
/// Marker class used for structured logging with ILogger OfficeToPdfModule;.
/// This class serves as the logging category identity for the OfficeToPdf module,
/// allowing Serilog and other logging frameworks to filter and route logs by type name.
/// </summary>
public class OfficeToPdfModule;
