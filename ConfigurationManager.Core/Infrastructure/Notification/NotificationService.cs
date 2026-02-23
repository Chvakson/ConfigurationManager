using Microsoft.AspNetCore.SignalR;
using ConfigurationManager.Core.Infrastructure.Hubs;
using ConfigurationManager.Core.Models.Dto.Mappings;
using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Infrastructure.Notification;

public class NotificationService : INotificationService
{
    private readonly IHubContext<ConfigurationHub> _hubContext;

    public NotificationService(IHubContext<ConfigurationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyConfigurationCreated(Configuration config)
    {
        var configDto = config.ToResponse();

        await _hubContext.Clients.All.SendAsync("ConfigurationEvent", new
        {
            EventType = "ConfigurationCreated",
            Message = "Создана новая конфигурация",
            Data = configDto,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyConfigurationUpdated(Configuration config)
    {
        var configDto = config.ToResponse();

        await _hubContext.Clients.All.SendAsync("ConfigurationEvent", new
        {
            EventType = "ConfigurationUpdated",
            Message = "Конфигурация обновлена",
            Data = configDto,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyConfigurationDeleted(Guid configId)
    {
        await _hubContext.Clients.All.SendAsync("ConfigurationEvent", new
        {
            EventType = "ConfigurationDeleted",
            Message = "Конфигурация удалена",
            Data = new { Id = configId },
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyConfigurationActivated(Configuration config)
    {
        var configDto = config.ToResponse();

        await _hubContext.Clients.All.SendAsync("ConfigurationEvent", new
        {
            EventType = "ConfigurationActivated",
            Message = "Конфигурация активирована",
            Data = configDto,
            Timestamp = DateTime.UtcNow
        });
    }
}