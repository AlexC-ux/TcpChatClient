using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Media;


namespace Ghost
{
    public partial class Form1 : Form
    {
        bool logged = false;
        List<string> usersOnline = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        public string userName;

        static IPAddress host = IPAddress.Parse("127.0.0.1");

        private const int port = 23;
        static TcpClient client ;
        static NetworkStream stream;

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

            
         
         
        public void Connect()
        {
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }
            finally
            {

                Disconnect();
            }
        }
        // отправка сообщений
        public void SendMessage()
        {

            if (!logged)
            {
                string message = txtUserName.Text;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            else
            {
                string message = txtMessage.Text;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
                
            
        }
        // получение сообщений
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    
                        ShowMessage(message);
                    if (!message.Contains(userName))
                    {
                        PlaySound();
                    }


                }
                catch
                {
                    ShowMessage("Подключение прервано!"); //соединение было прервано
                    Disconnect();


                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        public void btnConnect_Click(object sender, EventArgs e)
        {
            

            client = new TcpClient();
            client.NoDelay = true;
            if (!client.Connected)
            {
                userName = txtUserName.Text;
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток
            }
            if (client.Connected)
                {
                txtUserName.Visible = false;
                btnConnect.Visible = false;
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока

                ShowMessage($"Добро пожаловать, {userName}");
                txtMessage.Visible = true;
                btnSend.Visible = true;
                SendMessage();
                logged = true;
            }
                else { MessageBox.Show("Ошибка подлючения к севреру"); }


            
            

        }

        public  void ShowMessage(string message)
        {
            txtChat.Text += $"\n {message} \n";

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.Connected)
            {
                try
                {
                    client.Client.Close();
                    stream.Close();
                    client.Close();
                }
                catch { this.Close(); }
            }

            

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
                SendMessage();
            txtMessage.Text = "";


        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (Control.ModifierKeys == Keys.Enter)
            {
                if (client.Connected) 
                {
                    SendMessage();
                    txtMessage.Text = "";
                }

            }

        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            if (txtUserName.TextLength > 15)
            { 
                MessageBox.Show("Имя не может быть такой длинны");
                btnConnect.Visible = false;
            }
            else { btnConnect.Visible = true; }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                txtMessage.Text = "";
            }
        }
        public void PlaySound()
        {
            SoundPlayer sp = new SoundPlayer();
            sp.SoundLocation = "C://Windows//Media//Windows Unlock.wav";
            sp.Load();
            sp.Play();
        }

}
}