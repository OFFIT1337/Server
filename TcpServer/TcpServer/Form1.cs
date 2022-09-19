using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpServer
{
    public partial class Form1 : Form
    {
        static ServerObject server;
        static Thread listenThread;
        public Form1()
        {
            InitializeComponent();
            ClientObject.Form1 = this;
            ServerObject.Form1 = this;
            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                richTextBoxChat.Text = ex.Message + "\n";
            }
        }

        public class ClientObject
        {
            public static Form1 Form1;
            protected internal string Id { get; private set; }
            protected internal NetworkStream Stream { get; private set; }
            string userName;
            TcpClient client;
            ServerObject server;
            public ClientObject (TcpClient tcpClient, ServerObject serverObject)
            {
                Id = Guid.NewGuid().ToString ();
                client = tcpClient;
                server = serverObject;
                serverObject.AddConnection(this);
            }
            public void Proccess()
            {
                try
                {
                    Stream = client.GetStream();
                    string message = GetMessage();
                    userName = message;
                    message = userName + " вошел в чат";
                    server.BroadcastMessage(message, this.Id);
                    Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                    while (true)
                    {
                        try
                        {
                            message = GetMessage();
                            message = String.Format("{0}: {1}", userName, message);
                            Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                            server.BroadcastMessage(message, this.Id); 
                        }
                        catch
                        {
                            message = String.Format("{0}: покинул чат", userName);
                            Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                            server.BroadcastMessage(message, this.Id);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += e.Message + '\n'));
                }
                finally
                {
                    server.RemoveConnection(this.Id);
                    Close();
                }
            }
            private string GetMessage()
            {
                byte[] data = new byte[64]; 
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = Stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (Stream.DataAvailable);
                return builder.ToString();
            }
            protected internal void Close()
            {
                if (Stream != null)
                    Stream.Close();
                if (client != null)
                    client.Close();
            }

        }
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
                    while(true)
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
                byte [] data = Encoding.Unicode.GetBytes(message);
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
                


        private void Form1_Load(object sender, EventArgs e)
        {

        }


    }
}
