using StompProtocol_CSharp;
using StompProtocol_CSharp.Commands;
using StompProtocol_CSharp.HeaderFrames;
using StompProtocol_CSharp.Headers;
using StompProtocol_CSharp.StompFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockJS_CSharp
{
    public class SimpStomp
    {
        private readonly SockJS _sock;
        private bool _sock_connected { get { return _sock != null && _sock.Connected; } }
        private bool _stomp_connected { get; set; }

        public bool Connected { get { return _sock_connected && _stomp_connected; } }

        private Dictionary<string, Action<StompFrame>> _sub_tracker = new Dictionary<string, Action<StompFrame>>();
        private Dictionary<string, Action<StompFrame>> _dest_tracker = new Dictionary<string, Action<StompFrame>>();

        private Queue<StompFrame> _subscriptions = new Queue<StompFrame>();
        private Queue<StompFrame> _toSendRequests = new Queue<StompFrame>();

        private Action<StompFrame> _OnSend = null;
        private Action<StompFrame> _OnReceived = null;
        private Action _OnHeartBeat = null;

        private int _subID = 0;

        public SimpStomp(string url)
        {
            _sock = new SockJS(url);
        }
        public void Subscribe(string destination, Action<StompFrame> CallBack)
        {
            Subscribe(destination, null , CallBack);
        }

        public void Subscribe(string destination, HeaderFrame Headers , Action<StompFrame> CallBack)
        {
            int id = _subID++;
            if (Headers == null)
                Headers = new SubscribeHeaderFrame($"sub-{id}", destination);

            _sub_tracker.Add(
                Headers.GetHeaderFromProperty(new IdHeader().Property).Setting, 
                CallBack
            );
            _dest_tracker.Add(
                Headers.GetHeaderFromProperty(new DestinationHeader().Property).Setting, 
                CallBack
            );

            if (_sock_connected)
                Send(new StompFrame(new SubscribeCommand(), Headers));
            else
                _subscriptions.Enqueue(new StompFrame(new SubscribeCommand(), Headers));
        }
        public void Send(string destination, object obj)
        {
            if(obj.GetType() == typeof(string) || obj.GetType() == typeof(String))
                SendString(destination, obj as string);
            else
                Send(destination, new SendHeaderFrame(destination, obj, ContentTypeHeader.Inputs.JSON), obj);
        }
        protected internal void SendString(string destination, string obj)
        {
            Send(destination, new SendHeaderFrame(destination, obj, ContentTypeHeader.Inputs.TEXT), obj);
        }
        public void Send(string destination, HeaderFrame Headers, object obj)
        {
            if (Headers == null)
                Headers = new SendHeaderFrame(destination, obj, ContentTypeHeader.Inputs.JSON);

            Send(new StompFrame(new SendCommand(), Headers));
        }
        public void Send(StompFrame msg)
        {
            if (Connected)
            {
                _OnSend?.Invoke(msg);
                _sock.Send(msg.ToString());
            }
            else
                _toSendRequests.Enqueue(msg);
        }
        public void Connect(Action<StompFrame> OnEstablishedConnection, Action<StompFrame> OnStompSend, Action<StompFrame> OnStompIn,Action OnHeartBeat ,Action<string> OnErrorMessage)
        {
            _OnSend = OnStompSend;
            _OnHeartBeat = OnHeartBeat;
            _OnReceived = OnStompIn;
            _sock.OnMessage += (sender, e) =>
            {
                switch (e.Data.ToCharArray()[0])
                {
                    case 'o':
                        _OpenStompConnection();
                        break;
                    case 'h':
                        _stomp_connected = true;
                        _OnHeartBeat?.Invoke();
                        break;
                    case 'a':
                        _stomp_connected = true;
                        _OnStompFrameIn(OnEstablishedConnection, e);
                        _OnStompMessageIn(new StompFrame(e.Data.Substring(1)));
                        break;
                }
            };
            _sock.OnError += (sender, e) =>
                OnErrorMessage($"{e.Exception}  \n{ e.Message }");
            _sock.OnClose += (sender, e) =>
                _stomp_connected = false;
            if ( !(_sock.Connect()) )
                OnErrorMessage($"Could Not Connect to WebSocket Endpoint");
        }
        public void Connect(Action<StompFrame> OnEstablishedConnection, Action<StompFrame> OnStompSend,Action<StompFrame> OnStompIn, Action<string> OnErrorMessage)
        {
            Connect(OnEstablishedConnection, OnStompSend, OnStompIn, null, OnErrorMessage);
        }
        public void Connect(Action<StompFrame> OnEstablishedConnection, Action<StompFrame> OnStompIn, Action<string> OnErrorMessage)
        {
            Connect(OnEstablishedConnection, null, OnStompIn, null, OnErrorMessage);
        }
        public void Connect(Action<StompFrame> OnEstablishedConnection, Action<string> OnErrorMessage)
        {
            Connect(OnEstablishedConnection, null, OnErrorMessage);
        }
        public void Close()
        {
            if(_sock_connected)
                _sock.Close();
        }
        internal virtual void _OpenStompConnection()
        {
            StompFrame cnt_msg = new ConnectFrame();
            _OnSend?.Invoke(cnt_msg);
            _sock.Send(cnt_msg.ToString());
        }
        internal virtual void _OnStompConnectionEstablished()
        {
            _sendQueueFrames(_subscriptions);
            _sendQueueFrames(_toSendRequests);
        }
        internal virtual void _OnStompFrameIn(Action<StompFrame> Successful, WebSocketSharp.MessageEventArgs e)
        {
            StompFrame message = new StompFrame(e.Data.Substring(1));

            if (message.Command.GetType() == typeof(ConnectedCommand))
            {
                Successful(message);
                _OnStompConnectionEstablished();
            }
            else
            {
                _OnStompMessageIn(message);
            }
        }
        internal virtual void _OnStompMessageIn(StompFrame message)
        {
            if (message == null)
                return;

            String Destination =
                message?.Headers.GetHeaderFromProperty(
                    new DestinationHeader().Property
                )?.Setting;
            String SubID =
                message?.Headers.GetHeaderFromProperty(
                    new SubscribeIdHeader().Property
                )?.Setting;

            if (Destination != null && _dest_tracker.ContainsKey(Destination))
                _dest_tracker[Destination](message);
            else if (SubID != null && _sub_tracker.ContainsKey(SubID))
                _sub_tracker[SubID](message);

            _OnReceived?.Invoke(message);
        }
        private void _sendQueueFrames(Queue<StompFrame> q)
        {
            if(q != null)
                while (q.Count > 0)
                    Send(q.Dequeue());
        }
    }
}
