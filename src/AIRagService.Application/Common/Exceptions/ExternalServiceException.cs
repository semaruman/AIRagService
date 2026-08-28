namespace AIRagService.Application.Common.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string serviceName, string message)
        : base($"{serviceName}: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"{serviceName}: {message}", innerException)
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}
