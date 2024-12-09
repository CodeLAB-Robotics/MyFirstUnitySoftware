using System.Net;
using System.Net.Sockets;
using System.Text;
using ActUtlType64Lib;

namespace TCPServerFormEx
{
    public partial class Form1 : Form
    {
        public TcpListener server;
        public NetworkStream stream;
        public bool isRunning = false;
        string message;
        ActUtlType64 mxComponent;

        public Form1()
        {
            InitializeComponent();

            server = new TcpListener(IPAddress.Any, 7000);

            mxComponent = new ActUtlType64();
            mxComponent.ActLogicalStationNumber = 1; // Communication Setting에서 설정
        }

 
        private void connectButton_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                messageLabel.Text = "이미 서버가 실행중 입니다.";
                return;
            }

            isRunning = true;

            server.Start();

            Thread thread = new Thread(update);
            thread.Start();

            messageLabel.Text = "서버가 시작되었습니다!";
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                messageLabel.Text = "서버가 종료상태 입니다.";
                return;
            }

            isRunning = false;

            //stream.Close();
            server.Stop();

            messageLabel.Text = "서버가 종료되었습니다!";
        }

        void update()
        {

            while (isRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = 0;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string messageFromClient = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string response = "";
                    if (messageFromClient.Contains("Connect"))
                    {
                        int iRet = mxComponent.Open();
                        if(iRet == 0)
                        {
                            response = "MxComponent에 잘 연결 되었습니다.";
                        }
                        else
                        {
                            response = Convert.ToString(iRet, 16);
                        }
                    }
                    else if (messageFromClient.Contains("Disconnect"))
                    {
                        int iRet = mxComponent.Close();
                        if (iRet == 0)
                        {
                            response = "MxComponent 연결을 종료하였습니다.";
                        }
                        else
                        {
                            response = Convert.ToString(iRet, 16);
                        }
                    }
                    else if(messageFromClient.Contains("Quit"))
                    {
                        response = "서버를 종료합니다...";
                    }
                    else if(messageFromClient.Contains("GET"))
                    {   // GET,Y0,2
                        string[] splited = messageFromClient.Split(',');
                        string devicName = splited[1]; // "Y0"
                        int blockSize = int.Parse(splited[2]); // 2
                        
                        int[] output = new int[blockSize]; // { 64, 3 }
                        mxComponent.ReadDeviceBlock(devicName, blockSize, out output[0]);

                        response = string.Join(",", output); // "64,3"
                    }

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    if(messageFromClient.Contains("Quit"))
                    {
                        isRunning = false;
                    }
                    

                }

                
             }
        }
    }
}
