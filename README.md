# SockJS_CSharp
SockJs in C#

An Easy to use SockJS client side interface for using sockjs.

Currently, only the websocket option is avaible through this project.

SockJS(string url, @Optional string transport_preference, @Optional int id_length);

SockJS(string url, string[] transport_preferences, @Optional int id_length);

SockJS(string url, Func<string, string> id_gen);

SockJS(string url, string transport_preference, Func<string, string> id_gen);

SockJS();

SockJS();


bool Connect();

bool Close();

void Send(byte[] message);

void Send(string message)

bool Accept();

bool Ping( @Optional string message);

