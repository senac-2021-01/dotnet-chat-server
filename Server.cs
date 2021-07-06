using System;
using System.Net;
using System.Net.Sockets;

namespace dotnet_chat_server
{
    public class Server
    {
        private String ip;
        private Int32 port;

        public Server(String ip, Int32 port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Start(EventHandler OnClientConnect, EventHandler OnClientReceiveMessage)
        {
            Int32 clientNumber = 1;

            Boolean isRunning = true;

            TcpListener tcpListener = new TcpListener(IPAddress.Parse(this.ip), this.port);

            tcpListener.Start();

            Socket socket;
            ThreadClient threadClient;

            while (isRunning == true)
            {
                socket = tcpListener.AcceptSocket();

                threadClient = new ThreadClient(clientNumber, socket);

                threadClient.OnReceiveMessage += OnClientReceiveMessage;

                OnClientConnect.Invoke(this, new ConnectEventArgs()
                {
                    Client = threadClient
                });

                threadClient.Start();

                clientNumber++;
            }
        }

        public void SendMessage(ThreadClient client, dynamic message)
        {
            client.SendMessage(message);
        }
    }
}
