using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    public class TcpModel
    {
        public TcpClient tcpclnt1;
        public TcpClient tcpclnt2;
        public Stream stmC;
        public Stream stmL;
        private byte[] byteSend;
        private byte[] byteReceive;
        private string IPofServer;
        private int port;

        public TcpModel(string ip, int p)
        {
            IPofServer = ip;
            port = p;
            tcpclnt1 = new TcpClient();
            byteReceive = new byte[100];
        }
        //show information of client to update UI
        public string UpdateInformation()
        {
            string str = "";
            try
            {
                Socket s = tcpclnt1.Client;
                str = str + s.LocalEndPoint;
                Console.WriteLine(str);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                throw;
            }
            return str;
        }
        //Set up a connection to server and get stream for communication
        public int ConnectToServer()
        {
            try
            {
                tcpclnt1.Connect(IPofServer, port);
                stmC = tcpclnt1.GetStream();
                Console.WriteLine("OK!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                throw;
                return -1;
            }
            return 1;
        }
        //Send data to server after connection is set up
        public int SendData(string str, ref Stream stm)
        {
            try
            {
                ASCIIEncoding asen = new ASCIIEncoding();
                byteSend = asen.GetBytes(str);
                stm.Write(byteSend, 0, byteSend.Length);
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return -1;
                throw;
            }
        }
        
        //Send desktop image to server 
        public void SendDesktopImage(Image screenshot,ref Stream stm)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stm, screenshot);
            }
            catch (Exception)
            {
                //stm.Close();
            }
        }

        //Receive screenshot from server
        public Image ReceiveImage(ref Stream stm)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                Image tmp = (Image)bf.Deserialize(stm);
                return tmp;
            }
            catch (Exception)
            {
                Console.WriteLine("catch");
                //stm.Close();
                throw;
            }
            return null;
        }

        //Read data from server after connection is set up
        public string ReadData(ref Stream stm)
        {   
            string str = "";
            try
            {
                //count the length of data received
                int k = stm.Read(byteReceive, 0, 100);
                Console.WriteLine("Length of data received: " + k.ToString());
                Console.WriteLine("From server: ");
                //conver the byte recevied into string
                char[] c = new char[k];
                for (int i = 0; i < k; i++)
                {
                    Console.Write(Convert.ToChar(byteReceive[i]));
                    c[i] = Convert.ToChar(byteReceive[i]);
                }
                str = new string(c);
            }
            catch (Exception e)
            {
                str = "Error..... " + e.StackTrace;
                Console.WriteLine(str);
                throw;
            }
            return str;
        }
        //close connection
        public void CloseConnection()
        {
            try
            {
                if (tcpclnt1.Connected)
                    tcpclnt1.Close();
            }
            catch (Exception)
            {
                
            }
        }
    }
}
