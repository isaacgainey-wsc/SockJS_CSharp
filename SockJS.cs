using Newtonsoft.Json;
using SockJS_CSharp.Transports;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using WebSocketSharp;

namespace SockJS_CSharp
{
    public class SockJS
    {
        public static string Enabled_Transports = string.Join(",", new string[] { "websocket" });
        public readonly string url;
        internal SockJSTransport _transport = null;
        protected internal int _SessionIdLength { get; set; }
        protected internal string _SessionID { get; set; }
        public bool Connected { get { return _transport != null && _transport.Connected; } }

        public event EventHandler OnOpen;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<WebSocketSharp.ErrorEventArgs> OnError;
        public event EventHandler<CloseEventArgs> OnClose;
        
        public SockJS(string SockJS_url, string transports = "default", int sessionId_length = 8)
        {
            url = SockJS_url;
            _SessionIdLength = sessionId_length;
        }
        public SockJS(string SockJS_url, string[] transports, int sessionId_length = 8)
        {
            url = SockJS_url;
            _SessionIdLength = sessionId_length;
        }
        public SockJS(string SockJS_url, string[] transports, Func<string, double> sessionId_genator)
        {
            url = SockJS_url;
        }
        public SockJS(string SockJS_url, string transports, Func<string, double> sessionId_genator)
            : this (SockJS_url, new string[] { transports }, sessionId_genator)
        {
        }
        public SockJS(string SockJS_url, Func<string, string> sessionId_genator)
        {
            url = SockJS_url;
        }

        public bool Connect()
        {
            try
            {
                string _url = (url.Contains("//"))?url: $"http://{url}";
                SockJSInfo Connection_Stats = SockJSServerInfo($"{_url}/info");
                if (Connection_Stats == null)
                    return false;

                _transport = loadSelectedTransport(Connection_Stats);

                _transport._Close += this.OnClose;
                _transport._Error += this.OnError;
                _transport._Message += this.OnMessage;
                _transport._Open += this.OnOpen;
            }catch
            {
                return false;
            }
            return _transport.Connect();
        }
        public bool Close()
        {
            return _transport.Close();
        }

        public void Send(byte[] message)
        {
            _transport.Send(message);
        }

        public void Send(string message)
        {
            _transport.Send(message);
        }

        public bool Accept()
        {
            return _transport.Accept();
        }

        public bool Ping(string message = "")
        {
            return _transport.Ping(message);
        }
        internal string ServerIdGen(int seed, int length = 3)
        {
            var d = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
            int max = 0;
            string format = "";
            for (int i = 0; i < length; i++)
            {
                max *= 10;
                max += 9;
                format += "0";
            }
            return string.Format($"{new Random(seed).Next(0, max)}", format);
        }
        internal string SessionIdGen(int seed, int length)
        {
            return Convert.ToBase64String(
                        Guid.NewGuid().ToByteArray()
                    ).Replace("=", "")
                    .Replace("+", "")
                    .Replace("/","")
                    .Substring(0, length);
        }
        internal SockJSInfo SockJSServerInfo(String url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

            // List data response.
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                return JsonConvert.DeserializeObject<SockJSInfo>(
                            response.Content.ReadAsStringAsync().Result
                       );
            }
            else
            {
                throw new IOException($"Could not connect to {url}. Error: {(int)response.StatusCode} ({response.ReasonPhrase})");
            }
        }
        internal SockJSTransport loadSelectedTransport(SockJSInfo info)
        {
            if (info.websocket)
                return new WebSocketTransport(url, ServerIdGen(info.entropy) , SessionIdGen(info.entropy, _SessionIdLength));
            return null;
        }
    }
}
