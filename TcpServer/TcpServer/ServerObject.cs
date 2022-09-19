using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public class ServerObject
    {
        public static Form1 Form1;
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();
        ClientObject clientObject = null;
        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += ("Сервер запущен. Ожидание подключений..." + "\n")));
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Proccess));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += ex.Message + "\n"));
                Disconnect();
            }
        }
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }
        protected internal void Disconnect()
        {
            tcpListener.Stop();
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}
