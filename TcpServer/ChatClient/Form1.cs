using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        string message = "";
        private const string host = "127.0.0.1";
        private const int port = 8888;
        private string clientName = "";
        static TcpClient client;
        static NetworkStream stream;
        bool flag = true; // что это
        public Form1()
        {
            InitializeComponent();
            client = new TcpClient();
            client.Connect(host, port);
            richTextBoxChat.Text += "Введите свое имя: " + '\n';
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (flag)
            {
                this.message = richTextBoxMessage.Text;
                this.clientName = this.message;
                timer1.Enabled = true;
            }
            else
            {
                this.message = richTextBoxMessage.Text;
                timer1.Enabled = true;
                richTextBoxChat.Text += this.clientName + ':' + this.message + '\n';

            }
        }
        void SendMessage(string message)
        {
            byte [] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte [64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read (data, 0, data.Length);
                        builder.Append (Encoding.Unicode.GetString (data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = builder.ToString ();
                    richTextBoxChat.Invoke(new Action(() => richTextBoxChat.Text += message + '\n'));
                }
                catch
                {
                    richTextBoxChat.Invoke(new Action(() => richTextBoxChat.Text += "Подключение прервано!" + "\n"));
                    Disconnect();
                }
            }
        }
        static void Disconnect ()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit (0);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (flag)
                {
                    stream = client.GetStream();
                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    receiveThread.Start();
                    SendMessage(message);
                    richTextBoxChat.Text += "Добро пожаловать, " + message + "\n";
                    flag = false;
                    timer1.Enabled = false;
                }
                else
                {
                    stream = client.GetStream();
                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    SendMessage(message); 
                    timer1.Enabled=false;
                }
            }
            catch (Exception)
            {

            }
        } 

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
