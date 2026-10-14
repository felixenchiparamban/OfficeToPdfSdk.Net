namespace FelixELabs.OfficeToPdfSdk.Exceptions;

public class OfficeConversionException : Exception
{
    public OfficeConversionException(string message) : base(message) {}

    public OfficeConversionException(string message, Exception innerException) : base(message, innerException) {}
}
