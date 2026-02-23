using Microsoft.AspNetCore.SignalR;
using ConfigurationManager.Core.Models.SignalR;
using ConfigurationManager.Core.Services;

namespace ConfigurationManager.Core.Infrastructure.Hubs;

public class ConfigurationHub : Hub
{
    private readonly IConfigurationService _configurationService;
    private static readonly Dictionary<string, HashSet<EventType>> _userSubscriptions = new();

    public ConfigurationHub(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public override async Task OnConnectedAsync()
    {
        await SendAllConfigurations();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _userSubscriptions.Remove(connectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToEvent(EventType eventType)
    {
        try
        {
            var connectionId = Context.ConnectionId;

            if (!_userSubscriptions.ContainsKey(connectionId))
                _userSubscriptions[connectionId] = new HashSet<EventType>();

            _userSubscriptions[connectionId].Add(eventType);

            await Clients.Caller.SendAsync("Subscribed", new
            {
                eventType = eventType.ToString(),
                message = $"Successfully subscribed to {eventType}"
            });

            await SendAllConfigurations();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SubscribeToEvent: {ex.Message}");
            throw;
        }
    }

    public async Task UnsubscribeFromEvent(EventType eventType)
    {
        var connectionId = Context.ConnectionId;

        if (_userSubscriptions.ContainsKey(connectionId))
        {
            _userSubscriptions[connectionId].Remove(eventType);
        }

        await Clients.Caller.SendAsync("Unsubscribed", new
        {
            eventType = eventType.ToString(),
            message = $"Successfully unsubscribed from {eventType}"
        });
    }

    private async Task SendAllConfigurations()
    {
        var configs = await _configurationService.GetAllConfigurationsAsync();
        await Clients.Caller.SendAsync("AllConfigurations", new
        {
            configurations = configs,
            timestamp = DateTime.UtcNow
        });
    }
}