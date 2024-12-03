using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ActUtlType64Lib;

namespace Server
{
    class Program
    {
        enum State
        {
            CONNECTED,
            DISCONNECTED
        }
        static ActUtlType64 mxComponent;
        static State state = State.DISCONNECTED;

        static void Main(string[] args)
        {
            mxComponent = new ActUtlType64();
            mxComponent.ActLogicalStationNumber = 1;


            // 서버 소켓 생성 및 바인딩
            TcpListener server = new TcpListener(IPAddress.Any, 12345);
            server.Start();

            Console.WriteLine("서버 시작");

            TcpClient client;
            NetworkStream stream;
            byte[] buffer = new byte[1024];

            while (true)
            {
                // 1. 클라이언트 연결 대기
                client = server.AcceptTcpClient();
                Console.WriteLine("클라이언트 연결됨");

                // 2. 클라이언트의 네트워크 스트림 얻기
                stream = client.GetStream();

                int nByte;
                string messge = "";

                try
                {
                    // 3. 클라이언트로부터 데이터 읽기
                    while ((nByte = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // 4. UTF8 형식으로 인코딩
                        messge = Encoding.UTF8.GetString(buffer, 0, nByte);
                        Console.WriteLine("클라이언트: " + messge);

                        // 클라이언트에 데이터 보내기
                        buffer = new byte[1024]; // 버퍼 초기화
                        buffer = Encoding.UTF8.GetBytes("SERVER TEST");
                        stream.Write(buffer, 0, buffer.Length);

                        if (messge.Contains("Quit"))
                        {
                            Console.WriteLine("서버를 종료합니다.");
                            break;
                        }
                    }

                    if (messge.Contains("Quit"))
                    {
                        Console.WriteLine("서버를 종료합니다.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            // 연결 종료
            stream.Close();
            client.Close();
        }

        static public void Connect()
        {
            if (state == State.CONNECTED) return;

            int returnValue = mxComponent.Open();


            if (returnValue == 0)
            {
                state = State.CONNECTED;

                Console.WriteLine("Simulator와 연결이 잘 되었습니다.");
            }
            else
            {
                string hexValue = Convert.ToString(returnValue, 16);
                Console.WriteLine($"에러코드를 확인해 주세요. 에러코드: {hexValue}");
            }
        }

        static public void DisConnect()
        {
            if (state == State.DISCONNECTED) return;

            int returnValue = mxComponent.Close();


            if (returnValue == 0)
            {
                state = State.DISCONNECTED;

                Console.WriteLine("Simulator와 연결이 해제되었습니다.");
            }
            else
            {
                string hexValue = Convert.ToString(returnValue, 16);
                Console.WriteLine($"에러코드를 확인해 주세요. 에러코드: {hexValue}");
            }
        }
    }
}