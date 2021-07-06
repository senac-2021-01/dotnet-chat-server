using System;

namespace dotnet_chat_server
{
    public class ConnectEventArgs : EventArgs
    {
        public ThreadClient Client { set; get; }
    }
}
