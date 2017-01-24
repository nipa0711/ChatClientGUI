using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientGUI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        int PORT;
        string IP;
        bool command = false;

        NetworkStream NS = null;
        StreamReader SR = null;
        StreamWriter SW = null;
        TcpClient client = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            PORT = Convert.ToInt32(portBox.Text);
            IP = ipBox.Text;

            try
            {
                if (command != true)
                {
                    command = true;
                    btnConnect.Content = "접속종료";

                    client = new TcpClient(IP, PORT); //client 연결

                    Thread listen_thread = new Thread(listening); // 리스닝 쓰레드
                    listen_thread.Start();

                    NS = client.GetStream(); // 소켓에서 메시지를 가져오는 스트림
                    SR = new StreamReader(NS, Encoding.UTF8); // Get message
                    SW = new StreamWriter(NS, Encoding.UTF8); // Send message

                    SW.WriteLine("my-id:" + idBox.Text); // 메시지 보내기
                    SW.Flush();

                    UpdateChatBox(IP + ":" + PORT + "에 접속하였습니다.");
                }
                else
                {
                    disconnect();
                    UpdateChatBox("서버와의 접속이 종료되었습니다.");
                    command = false;
                    btnConnect.Content = "접속";
                }
            }
            catch (Exception A)
            {
                UpdateChatBox("ERR : " + A.Message);
            }
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            
        }

        public void listening()
        {
            string GetMessage = string.Empty;

            try
            {
                while (true)
                {
                    GetMessage = SR.ReadLine();
                    UpdateChatBox(GetMessage);
                }
            }
            catch (Exception A)
            {
                disconnect();
            }

        }

        public void UpdateChatBox(string data)
        {
            // 해당 쓰레드가 UI쓰레드인가?
            if (chatBox.Dispatcher.CheckAccess())
            {
                //UI 쓰레드인 경우
                chatBox.AppendText(data + Environment.NewLine);
                chatBox.ScrollToLine(chatBox.LineCount - 1); // 로그창 스크롤 아래로
            }
            else
            {
                // 작업쓰레드인 경우
                chatBox.Dispatcher.BeginInvoke((Action)(() => { chatBox.AppendText(data + Environment.NewLine); chatBox.ScrollToLine(chatBox.LineCount - 1); }));
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string msg = msgBox.Text;
                SW.WriteLine(msg); // 메시지 보내기
                SW.Flush();

                UpdateChatBox("나 : " + msg);
                msgBox.Text = "";
            }
            catch (Exception A)
            {
                UpdateChatBox("ERR : " + A.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disconnect();
        }

        void disconnect()
        {
            try
            {
                SW.WriteLine("G00D-BY2"); // 메시지 보내기
                SW.Flush();
            }
            catch (Exception a)
            {

            }

            if (SW != null)
                SW.Close();
            if (SR != null)
                SR.Close();
            if (client != null)
                client.Close();
        }
    }
}
