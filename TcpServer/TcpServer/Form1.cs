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
    }
}
