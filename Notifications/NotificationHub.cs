using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public async Task NotifyChange(string message)
    {
        await Clients.All.SendAsync("UpdateLayout", message);
    }
}

