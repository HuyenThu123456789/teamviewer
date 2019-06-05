using client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeamView
{
    public partial class Watch_Request : Form
    {
        public Watch_Request(TcpModel Client)
        {
            this.Client = Client;
            InitializeComponent();
            Thread thread = new Thread(Upscreen);
            thread.Start();
        }

        TcpModel Client;

        private void Upscreen()
        {
            try
            {
                while (true)
                {
                    Screen.Image = Client.ReceiveImage(ref Client.stmC);
                }
            }catch (Exception)
            {
                //Client.stmC.Close();
                //this.Close();
            }
        }
    }
}
