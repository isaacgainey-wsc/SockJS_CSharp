using StompProtocol_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SockJS_CSharp.Transports
{
    class WebSocketTransport : SockJSTransport
    {
        protected internal readonly WebSocket ws;
        public bool _connected { get { return ws.ReadyState == WebSocketState.Open; } }

        public WebSocketTransport(string url, string server_id = "0", string session_id = "000")
            : base ($"ws://{url}/{server_id}/{session_id}/websocket")
        {
            setEvents();
            ws = new WebSocket(this.url);
        }
        public WebSocketTransport(string url, string server_id, string session_id, string[] protocols)
           : base($"ws://{url}/{server_id}/{session_id}/websocket")
        {
            setEvents();
            ws = new WebSocket(this.url, protocols);
        }
        private void setEvents()
        {
            this._Message += (sender, e) =>
            {
                switch (e.Data.ToCharArray()[0])
                {
                    case 'o':
                    case 'a':
                    case 'h':
                        _hard_connected = true;
                        break;
                    case '1':
                    default:
                        _hard_connected = false;
                        break;
                }
            };
            this._Open += (sender, e) =>
            {
                _soft_connected = true;
            };
            this._Error += (sender, e) =>
            {
                _soft_connected = false;
                _hard_connected = false;
            };
            this._Close += (sender, e) =>
            {
                _soft_connected = false;
                _hard_connected = false;
            };
        }
        public override bool Accept()
        {
            return _wsCmd(ws.Accept);
        }

        public override bool Close()
        {
            return _wsCmd(ws.Close);
        }

        public override bool Connect()
        {
            ws.OnClose += this.OnClose;
            ws.OnError += this.OnError;
            ws.OnOpen += this.OnOpen;
            ws.OnMessage += this.OnMessage;
            _wsCmd(ws.Connect);
            return Connected;
        }

        public override bool Ping(string message = "")
        {
            return _wsCmd(ws.Ping, message);
        }

        public override void Send(string message)
        {
            _wsCmdVoid(ws.Send, message);
        }

        public override void Send(byte[] message)
        {
            _wsCmdVoid(ws.Send, message);
        }

        private bool _wsCmd(Action ws_cmd)
        {
            try
            {
                ws_cmd();
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool _wsCmdVoid<T>(Action<T> ws_cmd, T arg)
        {
            try
            {
                ws_cmd(arg);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool _wsCmd<T>(Func<T, bool> ws_cmd, T arg)
        {
            try
            {
                return ws_cmd(arg);
            }
            catch
            {
                return false;
            }
        }
    }
}
