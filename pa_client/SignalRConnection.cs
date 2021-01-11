using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Timers;

namespace pa_client
{
    public class SignalRConnection
    {
        const int heartbeatInterval = 30 * 1000; // in milliseconds
        private HubConnection connection;
        private string clientId;
        private Timer heartbeatTimer;

        public async void Start(string url, string clientId)
        {
            this.clientId = clientId;
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            // receive a message from the hub
            connection.On<string, string>("ReceiveMessage", (deviceId, message) => OnReceiveMessage(deviceId, message));

            var t = connection.StartAsync();

            t.Wait();

            // send a message to the hub
            await connection.InvokeAsync("WakeUpMessage", clientId);

            SetTimer();
        }

        private void OnReceiveMessage(string deviceId, string message)
        {
            Console.WriteLine($"{deviceId}: {message}");
        }

        private void SetTimer()
        {
            heartbeatTimer = new System.Timers.Timer(heartbeatInterval);
            heartbeatTimer.Elapsed +=  async (sender, e) => await SendHeartbeat();
            heartbeatTimer.AutoReset = true;
            heartbeatTimer.Enabled = true;
        }

        private async Task SendHeartbeat()
        {
            Console.WriteLine("Sending Heartbeat...");
            await connection.InvokeAsync("Heartbeat", clientId);
        }

    }
}
