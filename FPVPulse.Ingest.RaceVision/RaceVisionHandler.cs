using System;
using System.Diagnostics;
using System.Net;
using FPVPulse.Ingest;

namespace FPVPulse.Ingest.RaceVision
{
    public class RaceVisionHandler
    {
        RaceVisionClient client;

        TaskCompletionSource<bool> onLogin;
        CancellationTokenSource onDisconnect;

        public event Action<string, string> OnMessageReceived;

        public RaceVisionHandler(RaceVisionClient client) 
        {
            this.client = client;

            Bind(client);
        }

        void Bind(RaceVisionClient client)
        {
            client.Disconnected += OnClientDisconnected;
            client.MessageReceived += OnClientMessageReceived;
        }

        public async Task Connect(IPAddress ipAddress)
        {
            Console.Write("Connecting to LiveTime ...");
            client.Connect(ipAddress.ToString());

            if (!client.IsConnected)
                throw new Exception($"Failed to connect to {ipAddress}");

            Console.WriteLine(" Connected");

            Task.Delay(1000).Wait();

            if (onLogin != null && !onLogin.Task.IsCompleted)
            {
                onLogin.SetCanceled();
            }
            onLogin = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            Console.Write("Logging in to LiveTime ...");
            client.Login("", true, null, Environment.MachineName, ApplicationType.RaceVision, ClientOperatingSystem.Windows);
            Console.WriteLine(" Logged in");

            if (onDisconnect != null)
            {
                onDisconnect.Dispose();
            }
            onDisconnect = new CancellationTokenSource();

            client.Disconnected += OnClientDisconnected;
            client.MessageReceived += OnClientMessageReceived;

            await onLogin.Task;
        }

        public Task Disconnect()
        {
            if (client == null || !client.IsConnected)
                return Task.CompletedTask;

            Console.Write("Loggin out from LiveTime ...");
            client.Logout();
            Console.WriteLine(" Logged out");

            Console.Write("Disconnecting from LiveTime ...");
            client.Disconnect();
            Console.WriteLine(" Disconnected");

            return Task.CompletedTask;
        }

        void OnClientDisconnected()
        {
            if(onDisconnect != null)
                onDisconnect.Cancel();
        }

        void OnClientMessageReceived(string type, string json)
        {
            switch (type)
            {
                case "LoginResponse":
                    var isValid = IsValidLogin(json);
                    onLogin.SetResult(isValid);
                    if (!isValid)
                        Console.WriteLine("Failed to login with json response: " + json);
                    break;
                case "PingResponse":
                    break;
                default:
                    OnMessageReceived?.Invoke(type, json);
                    break;
            }
        }

        bool IsValidLogin(string json)
        {
            try
            {
                var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                if (jObject.TryGetValue("IsLoginAccepted", out var isLoginAccepted) &&
                    isLoginAccepted.Type == Newtonsoft.Json.Linq.JTokenType.Boolean)
                {
                    return (bool)isLoginAccepted;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse login response: {ex.Message}");
                return false;
            }
        }

        public Task RequestData()
        {
            if (client == null)
                throw new Exception("Client is not initialized. Please connect before requesting data.");

            if (!client.IsConnected)
                throw new Exception("Client is not connected. Please connect before requesting data.");

            Console.WriteLine(SimpleRequests.LiveState);
            client.TransmitPacket(SimpleRequests.LiveState);
            Console.WriteLine(SimpleRequests.LiveSetting);
            client.TransmitPacket(SimpleRequests.LiveSetting);
            Console.WriteLine(SimpleRequests.LiveRaceState);
            client.TransmitPacket(SimpleRequests.LiveRaceState);
            Console.WriteLine(SimpleRequests.LiveRaceTimeSync);
            client.TransmitPacket(SimpleRequests.LiveRaceTimeSync);
            Console.WriteLine(SimpleRequests.LiveRaceEntry);
            client.TransmitPacket(SimpleRequests.LiveRaceEntry);
            Console.WriteLine(SimpleRequests.LiveEstimatedPosition);
            client.TransmitPacket(SimpleRequests.LiveEstimatedPosition);

            return Task.CompletedTask;
        }

        public async Task KeepAlive(CancellationTokenSource runSource)
        {
            if (client == null || !client.IsConnected)
                return;

            bool run = true;
            while (run && !runSource.IsCancellationRequested && client.IsConnected)
            {
                try
                {
                    if (onDisconnect != null && onDisconnect.IsCancellationRequested)
                    {
                        run = false;
                        Console.WriteLine("Disconnected from LiveTime, stopping keep alive loop.");
                        continue;
                    }

                    await Task.Delay(1000);
                    SendKeepAlive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    run = false; // stop the loop on exception
                }
            }
        }

        void SendKeepAlive()
        {
            try
            {
                client.TransmitRequest(new PingRequest());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
