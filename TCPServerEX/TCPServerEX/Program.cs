using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // 서버 소켓 생성 및 바인딩
            TcpListener server = new TcpListener(IPAddress.Any, 12345);
            server.Start();

            Console.WriteLine("서버 시작");

            while (true)
            {
                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("클라이언트 연결됨");

                // 네트워크 스트림 얻기
                NetworkStream stream = client.GetStream();

                // 클라이언트로부터 데이터 읽기
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("클라이언트: " + data);

                // 클라이언트에 데이터 보내기
                string response = "Hello from server!";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);

                // 연결 종료
                client.Close();
            }
        }
    }
}