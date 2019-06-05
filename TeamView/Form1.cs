using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using client;

namespace TeamView
{
    public partial class TeamViewer : Form
    {
        public TeamViewer()
        {
            InitializeComponent();
            Listening = new Thread(StartListening);
            Listening.Start();
        }

        private TcpModel Client;
        private int status = -1;
        private readonly Thread Listening;
        public TcpListener tcpserver;
        Thread threadRe;
        Thread thread;
        string dataReceive;
        char sig;
        Watch_Request watch;

        private void StartListening()
        {
            Client = new TcpModel("127.0.0.1", 8080);
            //*************************/
            tcpserver = new TcpListener(IPAddress.Parse("127.0.0.1"), 9090);
            tcpserver.Start();
            Client.tcpclnt2 = tcpserver.AcceptTcpClient();
            Console.WriteLine("accept");
            Client.stmL = Client.tcpclnt2.GetStream();
            threadRe = new Thread(AlReceive);
            threadRe.Start();
        }

        private void StopListening()
        {
            tcpserver.Stop();
        }
//----------------------------------------------------------------------
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Stop watching")
            {
                btnConnect.Text = "Connect";
                if (Client.tcpclnt1.Connected)
                    Client.tcpclnt1.Close();
                watch.Close();
                status = -1;
            }
            else
            {
                ConnectToServer();
            }
        }
//----------------------------------------------------------------------
        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Client.CloseConnection();
                this.Close();
            }
            catch (Exception)
            {
                this.Close();
            }
        }

//----------------------------------------------------------------------
        private void AlReceive()
        {
            try
            {
                while (true)
                {
                    dataReceive = Client.ReadData(ref Client.stmL);
                    sig = dataReceive[0];
                    switch (sig)
                    {
                        case 'r':
                            Console.WriteLine("receive r");
                            DialogResult result = MessageBox.Show(dataReceive.Substring(1) + " wants to connect to your desktop.\n " +
                                            "Do you want to accept?", "TeamViewer notice",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result == DialogResult.Yes)
                            {
                                Client.SendData("1", ref Client.stmL);
                                Console.WriteLine("start send");
                                Share.Enabled = true;
                                threadRe.Abort();
                            }
                            else
                                Client.SendData("0", ref Client.stmL);
                            break;
                       
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                //Client.tcpclnt2.Close();
            }
        }
//----------------------------------------------------------------------
        void ConnectToServer()
        {
            try
            {
                if (status == -1)
                {
                    //Client = new TcpModel(IpHost.Text, int.Parse(PortHost.Text));
                    status = Client.ConnectToServer(); 
                    Client.SendData("r ", ref Client.stmC);
                    string dataReceive = Client.ReadData(ref Client.stmC);

                    if (dataReceive[0] == '1')
                    {
                        btnConnect.Text = "Stop watching";
                        watch = new Watch_Request(Client);
                        watch.Show();
                    }
                    else
                    {
                        MessageBox.Show("Request unsuccessful!!", "TeamViewer notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Client.tcpclnt1.Close();
                        btnConnect.Enabled = true;
                        //threadRe.Start();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //throw;
            }
        }
//----------------------------------------------------------------------
        private Image GrabDesktop()
        {
            Rectangle bounds = System.Windows.Forms.Screen.AllScreens[0].Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            return screenshot;
        }
//----------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("send");
            Client.SendDesktopImage(GrabDesktop(), ref Client.stmL);
        }

//----------------------------------------------------------------------
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            btnClose_Click(null, null);
        }

        private void Share_Click(object sender, EventArgs e)
        {
            if (Share.Text == "Share")
            {
                timer1.Start();
                Share.Text = "Stop Sharing";
                
            }
            else
            {
                timer1.Stop();
                Share.Text = "Share";

            }
        }
//------------------------------------------------------------------------

    }
}
