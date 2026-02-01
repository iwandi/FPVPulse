using System;
using System.Net;
using System.Diagnostics;

namespace FPVPulse.Ingest.RaceVision
{
    [Serializable]
    public class Config
    {
        public string? Key { get; set; }

        public string? ConnectionIPAddress { get; set; }
        public IPAddress? ConnectionIP { get; protected set; }

        public string? ApiUrlRoot { get; set; }
        public string? ApiToken { get; set; }

        public bool Validate()
        {
            bool isValid = true;

            if (Key == null)
            {
                isValid = false;
                Console.WriteLine("a valid Key is required");
            }
            else
            {
                Key = Key.Trim();
                if (string.IsNullOrWhiteSpace(Key))
                {
                    isValid = false;
                    Console.WriteLine("a valid Key is required");
                }
            }

            if (IPAddress.TryParse(ConnectionIPAddress, out var ip))
                ConnectionIP = ip;
            else
            {
                isValid = false;
                ConnectionIP = IPAddress.None;
                Console.WriteLine($"Invalid IP address: {ConnectionIPAddress}");
            }

            if (ApiUrlRoot == null)
            {
                isValid = false;
                Console.WriteLine("a valid ApiUrlRoot is required");
            }
            else
            {
                ApiUrlRoot = ApiUrlRoot.Trim();
                if (string.IsNullOrWhiteSpace(ApiUrlRoot))
                {
                    isValid = false;
                    Console.WriteLine("a valid ApiUrlRoot is required");
                }
            }

            // Allow null Token for now
            /*if (ApiToken == null)
            {
                isValid = false;
                Console.WriteLine("a valid ApiToken is required");
            }
            else
            {
                ApiToken = ApiToken.Trim();
                if (string.IsNullOrWhiteSpace(ApiToken))
                {
                    isValid = false;
                    Console.WriteLine("a valid ApiToken is required");
                }
            }*/

            return isValid;
        }
    }
}
