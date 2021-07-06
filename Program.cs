using System;
using System.Text;
using System.Collections.Generic;
namespace dotnet_chat_server
{
    class Program
    {
        private enum Action
        {
            NEW_USER,
            NEW_USER_SUCCESS,
            NEW_USER_FAIL,
            LOGIN,
            LOGIN_SUCCESS,
            LOGIN_FAIL,
            NEW_TEXT
        };

        private static List<ThreadClient> threadClients;

        private static event EventHandler OnClientConnect;
        private static event EventHandler OnClientReceiveMessage;

        static void Main(string[] args)
        {
            threadClients = new List<ThreadClient>();

            OnClientConnect += HandleOnClientConnect;
            OnClientReceiveMessage += HandleOnClientReceiveMessage;

            Server server = new Server("127.0.0.1", 5000);

            server.Start(OnClientConnect, OnClientReceiveMessage);
        }

        private static void ActionNewUser(ThreadClient threadClient, Message message)
        {
            String login = message.GetString("login");
            String name = message.GetString("name");
            String password = message.GetString("password");

            UserDAL userDAL = new UserDAL();

            UserDAL.InsertResult insertResult = userDAL.Insert(new User()
            {
                Login = login,
                Name = name,
                Password = password,
            });

            switch (insertResult)
            {
                case UserDAL.InsertResult.SUCCESS:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.NEW_USER_SUCCESS,
                        ClientNumber = threadClient.GetNumber(),
                        Text = "Usuário cadastrado com sucesso"
                    });
                    break;
                case UserDAL.InsertResult.LOGIN_UNIQUE_KEY_ERROR:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.NEW_USER_FAIL,
                        Text = "Usuário já cadastrado"
                    });
                    break;
                default:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.NEW_USER_FAIL,
                        Text = "Falha ao tentar cadastrar o usuário"
                    });
                    break;
            }
        }

        private static void ActionLogin(ThreadClient threadClient, Message message)
        {
            String login = message.GetString("login");
            String password = message.GetString("password");

            UserDAL userDAL = new UserDAL();

            StringBuilder userName = new StringBuilder();

            UserDAL.LoginResult loginResult = userDAL.GetByLoginPassword(new User()
            {
                Login = login,
                Password = password,
            }, userName);

            switch (loginResult)
            {
                case UserDAL.LoginResult.SUCCESS:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.LOGIN_SUCCESS,
                        ClientNumber = threadClient.GetNumber(),
                        Name = userName.ToString()
                    });
                    break;
                case UserDAL.LoginResult.USER_NOT_FOUND:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.LOGIN_FAIL,
                        Text = "Usuário ou senha inválidos"
                    });
                    break;
                default:
                    threadClient.SendMessage(new
                    {
                        Type = (Int32)Action.LOGIN_FAIL,
                        Text = "Falha ao tentar fazer o login"
                    });
                    break;
            }
        }

        private static void ActionNewText(ThreadClient threadClient, Message message)
        {
            Int32 clientNumber = message.GetInt32("ClientNumber");

            var clientsIterator = threadClients.GetEnumerator();

            ThreadClient threadClientIterator;

            while (clientsIterator.MoveNext())
            {
                threadClientIterator = clientsIterator.Current;

                if (threadClientIterator.GetNumber() != clientNumber)
                {
                    threadClientIterator.SendMessage(new
                    {
                        Type = (Int32)Action.NEW_TEXT,
                        UserName = message.GetString("UserName"),
                        Text = message.GetString("Text")
                    });
                }
            }
        }

        private static void HandleOnClientConnect(object sender, EventArgs e)
        {
            ConnectEventArgs connectEventArgs = e as ConnectEventArgs;

            if (connectEventArgs != null)
            {
                Console.WriteLine("Cliente conectado");

                ThreadClient threadClient = connectEventArgs.Client;

                threadClients.Add(threadClient);
            }
        }

        private static void HandleOnClientReceiveMessage(object sender, EventArgs e)
        {
            ThreadClient threadClient = sender as ThreadClient;
            MessageEventArgs messageEventArgs = e as MessageEventArgs;

            if (threadClient != null && messageEventArgs != null)
            {
                Message message = messageEventArgs.Message;

                switch (message.GetInt32("Type"))
                {
                    case (Int32)Action.NEW_USER:
                        ActionNewUser(threadClient, message);
                        break;
                    case (Int32)Action.LOGIN:
                        ActionLogin(threadClient, message);
                        break;
                    default:
                        ActionNewText(threadClient, message);
                        break;
                }
            }
        }
    }
}
