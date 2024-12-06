using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ActUtlType64Lib;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                string message = "";
                string msgToClient = "";

                try
                {
                    // 3. 클라이언트로부터 데이터 읽기
                    while ((nByte = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // 4. UTF8 형식으로 인코딩
                        message = Encoding.UTF8.GetString(buffer, 0, nByte);
                        Console.WriteLine($"{DateTime.Now} 클라이언트: " + message);

                        if (message == "Disconnect&Quit")
                        {
                            msgToClient = Disconnect();
                            Console.WriteLine("서버를 종료합니다.");
                            break;
                        }
                        else if(message == "Connect")
                        {
                            msgToClient = Connect();
                        }
                        else if(message == "Disconnect")
                        {
                            msgToClient = Disconnect();
                        }
                        else if(message.Contains("SET") && message.Contains("GET"))
                        {
                            // SET,X0,3,128,64,266,GET,X0,2
                            msgToClient = WriteDevices(message);
                        }/*
                        else if(message.Contains("GET"))
                        {
                            // GET,X0,3
                            // ReadDeviceBlock
                            // 33,22
                            msgToClient = ReadDevices(message);
                        }*/

                        // 4. 클라이언트에 데이터 보내기
                        buffer = new byte[1024]; // 버퍼 초기화
                        buffer = Encoding.UTF8.GetBytes(msgToClient);
                        stream.Write(buffer, 0, buffer.Length);
                    }

                    if (message.Contains("Quit"))
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

        private static string ReadDevices(string message)
        {
            // GET,X0,1;
            string[] strArray = message.Split(',');

            if(strArray.Length == 3)
            {
                string deviceName = strArray[1];        // X0
                int blockSize = int.Parse(strArray[2]); // 3

                int[] data = new int[blockSize];
                int iRet = mxComponent.ReadDeviceBlock(deviceName, blockSize, out data[0]);

                // int[] 35,22 -> Byte 01110010
                if (iRet == 0)
                {
                    Console.WriteLine("데이터 전송 완료(Server -> MxComponent)");

                    // int[] data = {32, 22} -> string d = "3222"
                    string convertedData = string.Join(",", data); // "32,22", "128"

                    return convertedData;
                }
                else
                {
                    string hexValue = Convert.ToString(iRet, 16);
                    return $"에러가 발생하였습니다. 에러코드: {hexValue}";
                }
            }
            else
            {
                return "데이터 이상";
            }
            
        }

        private static string WriteDevices(string message)
        {
            // SET,Y0,3,128,64,266,GET,X0,2
            string[] strArray = message.Split(',');

            if (strArray.Length < 3) return $"문자열 이상";

            string yDeviceName = strArray[1];        // X0
            int yDeviceBlockSize = int.Parse(strArray[2]); // 3
            int[] data = new int[yDeviceBlockSize];        // 128,64,266
            string xDeviceName = strArray[3 + yDeviceBlockSize + 1];
            string xDeviceBlockSize = strArray[3 + yDeviceBlockSize + 2];

            int j = 0;
            for(int i = 3; i < yDeviceBlockSize + 3; i++)
            {
                int value;
                bool isCorrect = int.TryParse(strArray[i], out value);

                if(!isCorrect) return $"문자열 이상";

                data[j] = value;
                j++;
            }

            int iRet = mxComponent.WriteDeviceBlock(yDeviceName, yDeviceBlockSize, data[0]);
            int[] xData = new int[int.Parse(xDeviceBlockSize)];
            int iRet2 = mxComponent.ReadDeviceBlock(xDeviceName, int.Parse(xDeviceBlockSize), out xData[0]);

            string result = string.Join(",", xData);

            if (iRet == 0 && iRet2 == 0)
            {
                Console.WriteLine("데이터 읽기 & 쓰기 완료");
                return result;
            }
            else
            {
                string hexValue = Convert.ToString(iRet, 16);
                return $"에러가 발생하였습니다. 에러코드: {hexValue}";
            }
        }

        // 통합: 서버에서 데이터를 주고 받은 후 원하는 데이터만 받기
        // (Unity to Server 데이터 형식: SET,X0,3,128,24,1/GET,Y0,2/GET,D0,3)
        // (Server to Unity 데이터 형식: X0,123,24/D0,23
        static string ReadAndWriteDevices(string message)
        {
            List<string[]> commands = SplitCommands(message);

            ReadXDevice(commands[0]);

            return "result";

            List<string[]> SplitCommands(string input)
            {
                string[] commands = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                List<string[]> commandList = new List<string[]>();
                foreach (string command in commands)
                {
                    string[] commands2nd = command.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    commandList.Add(commands2nd);
                }

                return commandList;
            }
        }

        private static void ReadXDevice(string[] strings)
        {
            throw new NotImplementedException();
        }

        static public string Connect()
        {
            if (state == State.CONNECTED) 
                return "이미 연결되어 있습니다.";

            int returnValue = mxComponent.Open();


            if (returnValue == 0)
            {
                state = State.CONNECTED;

                Console.WriteLine("Simulator와 연결이 잘 되었습니다.");

                return "CONNECTED";
            }
            else
            {
                string hexValue = Convert.ToString(returnValue, 16);
                Console.WriteLine($"에러코드를 확인해 주세요. 에러코드: {hexValue}");

                return $"에러코드를 확인해 주세요. 에러코드: {hexValue}";
            }
        }

        static public string Disconnect()
        {
            if (state == State.DISCONNECTED) 
                return "연결해제 상태입니다.";

            int returnValue = mxComponent.Close();


            if (returnValue == 0)
            {
                state = State.DISCONNECTED;

                Console.WriteLine("Simulator와 연결이 해제되었습니다.");

                return "DISCONNECTED";
            }
            else
            {
                string hexValue = Convert.ToString(returnValue, 16);
                Console.WriteLine($"에러코드를 확인해 주세요. 에러코드: {hexValue}");

                return $"에러코드를 확인해 주세요. 에러코드: {hexValue}";
            }
        }
    }
}