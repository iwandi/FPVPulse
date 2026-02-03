using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;

namespace FPVPulse.Ingest.RaceVision
{ 
    public static class Program
    {
        const string configFilePath = "config.json";

        [STAThread]
        public static async Task Main(string[] args)
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

			if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Configuration file '{configFilePath}' not found. Please create a config.json file.");
                return;
            }

            var configText = File.ReadAllText(configFilePath);
            if (string.IsNullOrWhiteSpace(configText))
            {
                Console.WriteLine("Configuration file is empty. Please provide valid configuration.");
                return;
            }

            Config? config = null;
            try
            {
                config = JsonConvert.DeserializeObject<Config>(configText);

                if (config == null || !config.Validate())
                {
                    Console.WriteLine("Configuration validation failed. Please check the config.json file. A valid config is required to run.");
                    return;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Unexpected Exception while trying to load the config: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }

            Debug.Assert(config != null);

            var client = new RaceVisionClient(config.Key);
            var ingestClient = new IngestClient(config.ConnectionIPAddress, config.ApiUrlRoot, config.ApiToken);
            var handler = new RaceVisionHandler(client);
            var fpvPulse = new FPVPulseReportProcessor(client, ingestClient);

            handler.OnMessageReceived += (type, json) =>
            {
#if DEBUG
                LogAndDumpProcessor.OnMessageReceived(type, json);
#endif
                fpvPulse.ProcessLiveTimeMessage(type, json);

            };

            var keepAlive = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                while (!client.IsConnected)
                {
                    try
                    {
                        await handler.Connect(config.ConnectionIP);
                        await Task.Delay(2000);
                        await handler.RequestData();
                        await Task.Delay(1000);
                        _ = handler.KeepAlive(keepAlive);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception during connection: {ex.Message}");
                        Console.WriteLine("Retrying connection in 1 seconds...");
                        await Task.Delay(1000);
                    }
                }
            });

            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "q":
                    case "quit":
                    case "exit":
                        keepAlive.Cancel();
                        await handler.Disconnect();
                        return;
                }
            }
        }
    }
}