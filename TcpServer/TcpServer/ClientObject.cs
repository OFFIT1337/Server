using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TcpServer.Form1;

namespace TcpServer
{
    public class ClientObject
    {
        public static Form1 Form1;
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        private string UserName { get; set; }
        private readonly TcpClient client;
        private readonly ServerObject server;
        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
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
                UserName = message;
                message = UserName + " вошел в чат";
                server.BroadcastMessage(message, this.Id);
                Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message == "")
                            throw new Exception();
                        message = String.Format("{0}: {1}", UserName, message);
                        Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", UserName);
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
            byte[] data = new byte[6297630];
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
}
