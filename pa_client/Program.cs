using System;

namespace pa_client
{
    class Program
    {
        static readonly string clientId = "pa_" + new Random().Next(100000).ToString();

        static void Main(string[] args)
        {
            Console.WriteLine("Public Address Client App\n");
            Console.WriteLine($"Client ID: {clientId}");

            var signalRConnection = new SignalRConnection();
            signalRConnection.Start("http://localhost:7777/centralHub", clientId);

            Console.ReadLine();
        }
    }

}
