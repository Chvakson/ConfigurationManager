using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Infrastructure.Notification;

public interface INotificationService
{
    Task NotifyConfigurationCreated(Configuration config);
    Task NotifyConfigurationUpdated(Configuration config);
    Task NotifyConfigurationDeleted(Guid configId);
    Task NotifyConfigurationActivated(Configuration config);
}