using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace dotnet_chat_server
{
    public class ThreadClient
    {
        private Int32 number;
        private Boolean isRunning;
        private Thread thread;
        private Socket socket;
        public event EventHandler OnReceiveMessage;

        public ThreadClient(int number, Socket socket)
        {
            this.number = number;
            this.socket = socket;

            try
            {
                this.thread = new Thread(new ThreadStart(Run));

                this.isRunning = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        protected void Run()
        {
            NetworkStream networkStream = new NetworkStream(this.socket);

            while (this.isRunning)
            {
                try
                {
                    this.OnReceiveMessage.Invoke(this, new MessageEventArgs()
                    {
                        Message = Message.Deserialize(networkStream)
                    });
                }
                catch (Exception ex)
                {
                    this.isRunning = false;

                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public void Start()
        {
            try
            {
                this.thread.Start();
            }
            catch (Exception ex)
            {
                this.isRunning = false;

                Console.WriteLine(ex.StackTrace);
            }
        }

        public void SendMessage(dynamic message)
        {
            try
            {
                NetworkStream networkStream = new NetworkStream(this.socket);
                BinaryWriter binaryWriter = new BinaryWriter(networkStream);

                binaryWriter.Write(Message.Serialize(message));
            }
            catch
            {
                this.isRunning = false;
            }
        }

        public int GetNumber()
        {
            return this.number;
        }
    }
}
