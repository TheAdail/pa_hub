using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using NeoSmart.Caching.Sqlite;

namespace pa_hub.Hubs
{
    public class CentralHub : Hub
    {
        const string managerGroup = "Manager";
        IDistributedCache cache;

        public CentralHub(IDistributedCache _cache) : base()
        {
            cache = _cache;
        }

        public async Task SendMessageToDevice(string deviceId, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", deviceId, message);
        }

        public async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync($"{groupName}Message", message);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync($"{groupName}Message", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync($"{groupName}Message", $"{Context.ConnectionId} has left the group {groupName}.");
        }


        public DateTime Now()
        {
            return DateTime.UtcNow;
        }

        public async Task WakeUpMessage(string clientAppInstance)
        {
            cache.SetString(Context.ConnectionId, clientAppInstance);
            await SendMessageToGroup(managerGroup, $"WU|{Context.ConnectionId}:{clientAppInstance}|{Now():u}");
        }

        public async Task Heartbeat(string clientAppInstance)
        {
            await SendMessageToGroup(managerGroup, $"HB|{Context.ConnectionId}:{clientAppInstance}|{Now():u}");
        }

        public override async Task OnConnectedAsync()
        {
            await SendMessageToGroup(managerGroup, $"CO|{Context.ConnectionId}|{Now():u}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if(exception == null)
            {
                string clientAppInstance = cache.GetString(Context.ConnectionId);
                cache.Remove(Context.ConnectionId);
                await SendMessageToGroup(managerGroup, $"DC|{Context.ConnectionId}:{clientAppInstance}|{Now():u}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
