using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;

namespace FPVPulse.Ingest.RaceVision
{
    public class MethodPacket
    {
        public string PacketType { get; set; }
        public byte[] PacketBytes { get; set; }
    }

    public enum ApplicationType
    {
        Unknown,
        RaceBoard,
        RaceStation,
        RaceVision,
        ScoringEngine,
        Scoreboard,
        RaceRadio,
        RaceWatch,
        AxisCamera,
        RaceLine
    }

    public enum ClientOperatingSystem
    {
        Unknown,
        Windows,
        OSX,
        Linux,
        iOS,
        Android
    }

    public static class SimpleRequests
    {
        public const string Logout = "LogoutRequest";

        public const string LiveState = "LiveStateRequest";
        public const string LiveSetting = "LiveSettingRequest";
        public const string LiveRaceState = "LiveRaceStateRequest";
        public const string LiveRaceTimeSync = "LiveRaceTimeSyncRequest";
        public const string LiveRaceEntry = "LiveRaceEntryRequest";
        public const string LiveEstimatedPosition = "LiveEstimatedPositionRequest";
    }

    public class LoginRequest
    {
        public int APIVersion { get; set; }
        public string DeviceName { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public ClientOperatingSystem ClientOperatingSystem { get; set; }
        public string Password { get; set; }
        public bool IsLive { get; set; }
        public long? LiveDriverLIDFilter { get; set; }
    }

    public class PingRequest
    {
        public DateTime DateTimeUTC { get; set; }
    }

    public abstract class Response
    {
        public bool IsOK { get; set; }
        public string Error { get; set; }
    }

    public class PingResponse : Response
    {
        public DateTime DateTimeUTC { get; set; }
    }

    public class RaceVisionClient
    {
        const int API_VERSION = 10;
        const int PORT_NUMBER = 54235;
        const int CONNECTION_TIMEOUT = 5000;
        const int DISCONNECTION_TIMEOUT = 2000;

        string key;

        bool _isCompressed;
        bool _isEncrypted;

        string _url;

        HubConnection _hubConnection;

        bool _isLoggedIn;

        Channel<bool> _onMessageReceivedChannel;
        readonly ConcurrentDictionary<string, string> _messageBuffer;
        Task _messageProcessingTask;

        public bool IsConnecting
        {
            get
            {
                if (_hubConnection == null)
                    return false;
                if (_hubConnection.State == HubConnectionState.Connecting)
                    return true;
                return false;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (_hubConnection == null)
                    return false;
                if (_hubConnection.State == HubConnectionState.Connected)
                    return true;
                return false;
            }
        }

        public bool IsLoggedIn => _isLoggedIn;

        public event Action Disconnected;
        public event Action<string, string> MessageReceived;

        public RaceVisionClient(string key)
        {
            _isCompressed = true;
            _isEncrypted = true;
            _isLoggedIn = false;
            this.key = key;

            _messageBuffer = new ConcurrentDictionary<string, string>();
        }

        public void Connect(string serverAddress)
        {
            string url = $"http://{serverAddress}:{PORT_NUMBER}/signalr";
            if (_url != url && _hubConnection != null)
            {
                try
                {
                    Disconnect(disableEvents: true);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
            if (IsConnecting)
            {
                return;
            }
            if (IsConnected)
            {
                return;
            }
            try
            {
                _onMessageReceivedChannel = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

                _messageProcessingTask = Task.Run(ProccessMessageBuffer);

                _url = url;
                _hubConnection = new HubConnectionBuilder().WithUrl(url).Build();
                _hubConnection.On("Response", delegate (MethodPacket methodPacket)
                {
                    ProcessIncomingPacket(methodPacket);
                });
                if (!_hubConnection.StartAsync().Wait(CONNECTION_TIMEOUT))
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                Disconnect();
            }
        }

        public void Login(string password, bool isLive, long? liveDriverLIDFilter, string deviceName, ApplicationType applicationType, ClientOperatingSystem clientOperatingSystem)
        {
            if (IsConnected && !IsLoggedIn)
            {
                SendLoginRequest(password, isLive, liveDriverLIDFilter, deviceName, applicationType, clientOperatingSystem);
            }
        }

        public void Logout()
        {
            if (IsLoggedIn)
            {
                TransmitPacket(SimpleRequests.Logout);
                _isLoggedIn = false;
            }
        }

        public void Disconnect(bool disableEvents = false)
        {
            if (_hubConnection == null)
            {
                return;
            }
            try
            {
                if (_hubConnection.State != HubConnectionState.Disconnected)
                {
                    _hubConnection?.StopAsync().Wait(TimeSpan.FromMilliseconds(DISCONNECTION_TIMEOUT, 0L));
                }

                _onMessageReceivedChannel.Writer.Complete();
                _messageProcessingTask.Wait();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            _hubConnection = null;
            _isLoggedIn = false;
            OnDisconnected();
        }

        private void ProcessIncomingPacket(MethodPacket methodPacket)
        {
            if (_isCompressed)
                methodPacket.PacketBytes = Decompress(methodPacket.PacketBytes);
            string packetString = Encoding.UTF8.GetString(methodPacket.PacketBytes);
            if (_isEncrypted)
                packetString = DecryptString(packetString, key);
            try
            {
                switch (methodPacket.PacketType)
                {
                    case "LogoutResponse":
                        LogoutResponse();
                        break;
                    case "PingRequest":
                        PingRequest();
                        break;
                    default:
                        packetString = packetString.Substring(0, packetString.LastIndexOf('}') + 1);
                        //OnMessageReceived(methodPacket.PacketType, packetString);
                        _messageBuffer[methodPacket.PacketType] = packetString;
                        _onMessageReceivedChannel.Writer.TryWrite(true);
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                Disconnect();
            }
        }

        private async Task ProccessMessageBuffer()
        {
            try
            {
                await foreach (var _ in _onMessageReceivedChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        foreach (var (packetType, packetString) in _messageBuffer)
                        {
                            if (_messageBuffer.TryRemove(packetType, out var _))
                            {
                                OnMessageReceived(packetType, packetString);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void LogoutResponse()
        {
            Disconnect();
        }

        private void PingRequest()
        {
            PingResponse pingResponse = new PingResponse();
            pingResponse.IsOK = true;
            pingResponse.Error = string.Empty;
            pingResponse.DateTimeUTC = DateTime.UtcNow;
            TransmitRequest(pingResponse);
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke();
        }

        private void OnMessageReceived(string packetType, string packetString)
        {
            MessageReceived?.Invoke(packetType, packetString);
        }

        private void SendLoginRequest(string password, bool isLive, long? liveDriverLIDFilter, string deviceName, ApplicationType applicationType, ClientOperatingSystem clientOperatingSystem)
        {
            LoginRequest loginRequest = new LoginRequest();
            loginRequest.APIVersion = API_VERSION;
            loginRequest.DeviceName = deviceName;
            loginRequest.ApplicationType = applicationType;
            loginRequest.ClientOperatingSystem = clientOperatingSystem;
            if (string.IsNullOrWhiteSpace(password))
                loginRequest.Password = null;
            else
                loginRequest.Password = password;
            loginRequest.IsLive = isLive;
            loginRequest.LiveDriverLIDFilter = liveDriverLIDFilter;
            TransmitRequest(loginRequest);
        }

        public void TransmitPacket(string request, string json = "{}")
        {
            if (!IsConnected)
                return;
            string packetString = json;
            if (_isEncrypted)
                packetString = EncryptString(packetString, key);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            if (_isCompressed)
                packetBytes = Compress(packetBytes);
            MethodPacket methodPacket = new MethodPacket();
            methodPacket.PacketType = request;
            methodPacket.PacketBytes = packetBytes;
            try
            {
                _hubConnection.InvokeAsync("Request", methodPacket).ContinueWith(delegate (Task task)
                {
                    if (task.IsFaulted)
                        Disconnect();
                }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                Disconnect();
            }
        }

        public void TransmitRequest<T>(T request)
        {
            if (!IsConnected)
                return;
            string packetString = JsonConvert.SerializeObject(request);
            var packetType = typeof(T).Name;
            TransmitPacket(packetType, packetString);
        }

        public static byte[] Compress(byte[] data)
        {
            using MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            using MemoryStream input = new MemoryStream(data);
            using MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static string EncryptString(string input, string keyString)
        {
            byte[] key = Encoding.UTF8.GetBytes(keyString);
            using Aes aes = Aes.Create();
            aes.Padding = PaddingMode.Zeros;
            using ICryptoTransform encryptor = aes.CreateEncryptor(key, aes.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(input);
            }
            byte[] iv = aes.IV;
            byte[] decryptedContent = msEncrypt.ToArray();
            byte[] result = new byte[iv.Length + decryptedContent.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);
            return Convert.ToBase64String(result);
        }

        public static string DecryptString(string input, string keyString)
        {
            byte[] fullCipher = Convert.FromBase64String(input);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            byte[] key = Encoding.UTF8.GetBytes(keyString);
            using Aes aes = Aes.Create();
            aes.Padding = PaddingMode.Zeros;
            using ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            using MemoryStream msDecrypt = new MemoryStream(cipher);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        void HandleException(Exception ex)
        {
            Console.WriteLine($"CustomClientWorker: {ex.Message} {ex.StackTrace}");
        }
    }
}
