namespace ConfigurationManager.Core.Models.SignalR;

public enum EventType
{
    ConfigurationCreated,
    ConfigurationUpdated,
    ConfigurationDeleted,
    ConfigurationActivated
}

public class SignalRMessage
{
    public EventType EventType { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}