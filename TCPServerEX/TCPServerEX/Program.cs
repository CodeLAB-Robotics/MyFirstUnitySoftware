using System;
using System.Linq;
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
                string message = "";

                try
                {
                    // 3. 클라이언트로부터 데이터 읽기
                    while ((nByte = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // 4. UTF8 형식으로 인코딩
                        message = Encoding.UTF8.GetString(buffer, 0, nByte); // "Connect\0\0\0\0... 1024개
                        Console.WriteLine($"{DateTime.Now} 클라이언트: " + message);
                        string msgToClient = "";
                        string retMsg = ""; // 0,0,0,170,
                        if (message.Contains("Disconnect&Quit"))
                        {
                            msgToClient = Disconnect();
                            Console.WriteLine("서버를 종료합니다.");
                            break;
                        }
                        else if(message.Contains("Connect"))
                        {
                            msgToClient = Connect();
                        }
                        else if(message.Contains("Disconnect"))
                        {
                            msgToClient = Disconnect();
                        }
                        else if (message.Contains("SET") && message.Contains("GET"))
                        {
                            // msg: GET,Y0,4,SET,X0,2,0,170
                            // Read Device -> Write Device
                            string[] dataFromUnity = message.Split(',');
                            string yDeviceName = dataFromUnity[1]; // Y0
                            int xBlockSize = int.Parse(dataFromUnity[2]); // 4
                            string xDeviceName = dataFromUnity[4]; // X0
                            int yBlockSize = int.Parse(dataFromUnity[5]); // 2
                            string sensorData = dataFromUnity[6] + "," + dataFromUnity[7]; // Sensor Data(0) + Limit Switch Data(170)
                            // 1. PLC의 데이터 읽기(PLC의 실린더 신호를 MPS에 보내기 위해서)
                            // 신호 ex) int[] = {32,22} : 32(실린더 전후진 신호, 컨베이어 정회전 역회전 신호), 22(램프신호)
                            devices = new int[xBlockSize];
                            
                            ReadDeviceBlock(yDeviceName, xBlockSize, out msgToClient);
                            Console.WriteLine(msgToClient);


                            // 2. PLC에 데이터 쓰기(센서 데이터 + Limit Switch 데이터)
                            // 2-2: WriteDeviceBlock: 클라이언트로 부터 받은 데이터(dataToServer)에서 MPS의 센서데이터(sensorData)를 PLC에게 전달,
                            // 2-3: retMsg: retMsg(위에서 받은 PLC 신호) + 현재 센서 데이터(sensorData)를 Client로 전달(dataFromServer)
                            // dataToServer: GET,Y0,4,SET,X0,0,170
                            WriteDeviceBlock(xDeviceName, yBlockSize, sensorData);
                            Console.WriteLine(retMsg); // Y0,0,0
                        }

                        // 4. 클라이언트에 데이터 보내기
                        byte[] dataBytes = new byte[1024]; // 새로운 버퍼를 사용해서 다시전송
                        dataBytes = Encoding.UTF8.GetBytes(msgToClient);
                        stream.Write(dataBytes, 0, dataBytes.Length);
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

        static int[] devices;
        // "SET,sensorNum,lsNum"  SET,Y0,0,170
        public static string WriteDeviceBlock(string deviceName, int blockNum, string dataFromClient)
        {
            string[] dataSplited = dataFromClient.Split(",");

            int[] data = new int[blockNum]; // 2,3
            //data[0] = devices[0]; // xDevice 첫 번재 블록
            //data[1] = devices[1]; // xDevice 두 번째 블록
            data[0] = int.Parse(dataSplited[0]); // yDevice 첫 번재 블록
            data[1] = int.Parse(dataSplited[1]); // yDevice 두 번째 블록

            // 0,170
            int ret = mxComponent.WriteDeviceBlock(deviceName, blockNum, ref data[0]);

            if (ret == 0)
            {
                //return $"X0,{data[0]},{data[1]},Y0,{data[2]},{data[3]}";
                return $"{data[0]},{data[1]}";
            }
            else
            {
                return "ERROR " + Convert.ToString(ret, 16);
            }
        }

        /// <summary>
        /// 디바이스를 받기위해서 사용
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static int[] ReadDeviceBlock(string deviceName, int blockSize, out string retMsg)
        {
            retMsg = "";

            int ret = mxComponent.ReadDeviceBlock(deviceName, blockSize, out devices[0]);

            if (ret == 0)
            {
                for(int i = 0; i < devices.Length; i++)
                {
                    if(i < devices.Length - 1)
                        retMsg += devices[i].ToString() + ",";
                    else 
                        retMsg += devices[i].ToString();
                }

                return devices;
            }
            else
            {
                retMsg = "ERROR " + Convert.ToString(ret, 16);
                return null;
            }
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
                    string convertedData = String.Join(",", data); // "32,22", "128"

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
            // SET,X0,3,128,64,266
            string[] strArray = message.Split(',');

            if (strArray.Length < 3) return $"문자열 이상";

            string deviceName = strArray[1];        // X0
            int blockSize = int.Parse(strArray[2]); // 3
            int[] data = new int[blockSize];        // 128,64,266

            int j = 0;
            for(int i = 3; i < strArray.Length; i++)
            {
                int value;
                bool isCorrect = int.TryParse(strArray[i], out value);

                if(!isCorrect) return $"문자열 이상";

                data[j] = value;
                j++;
            }

            int iRet = mxComponent.WriteDeviceBlock(deviceName, blockSize, data[0]);

            if (iRet == 0)
            {
                return "데이터 전송 완료(Server -> MxComponent)";
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

                return "CONNECTED___________________________________";
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