using System;

namespace dotnet_chat_server
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { set; get; }
    }
}
