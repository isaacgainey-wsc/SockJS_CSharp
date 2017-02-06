using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SockJS_CSharp
{
    abstract class SockJSTransport
    {
        public readonly string url;
        protected internal virtual bool _soft_connected { get; set; }
        protected internal virtual bool _hard_connected { get; set; }
        public bool Connected { get { return _soft_connected && _hard_connected; } }

        public SockJSTransport(string url = "")
        {
            this.url = url;
        }
        public abstract bool Connect();
        public abstract bool Close();
        public abstract void Send(byte[] message);
        public abstract void Send(string message);
        public abstract bool Accept();
        public abstract bool Ping(string message = "");

        protected internal event EventHandler _Open;
        protected internal event EventHandler<MessageEventArgs> _Message;
        protected internal event EventHandler<WebSocketSharp.ErrorEventArgs> _Error;
        protected internal event EventHandler<CloseEventArgs> _Close;
        protected internal virtual void OnOpen(object sender, EventArgs e)
        {
            _Open?.Invoke(this, e);
        }
        protected internal virtual void OnMessage(object sender, MessageEventArgs e)
        {
            _Message?.Invoke(this, e);
        }
        protected internal virtual void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            _Error?.Invoke(this, e);
        }
        protected internal virtual void OnClose(object sender, CloseEventArgs e)
        {
            _Close?.Invoke(this, e);
        }
    }
}
