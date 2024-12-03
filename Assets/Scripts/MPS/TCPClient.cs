using System.Net.Sockets;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using TMPro;
using System.Collections;
using UnityEngine.Windows;
using System.Threading.Tasks;
using System.Threading;

public class TCPClient : MonoBehaviour
{
    public static TCPClient Instance;

    [SerializeField] TMP_InputField dataInput;
    public bool isConnected;
    public float interval;
    public string xDevices = "0000000000000000";
    public string yDevices = "0000000000000000";
    public string dDevices = "0000000000000000";

    TcpClient client;
    NetworkStream stream;
    string msg;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            // 1. 서버에 연결
            client = new TcpClient("127.0.0.1", 12345);
            print("서버에 연결됨");

            // 2. 네트워크 스트림 얻기
            stream = client.GetStream();
        }
        catch(Exception ex)
        {
            print(ex);
            print("서버를 먼저 작동시켜 주세요.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    CancellationTokenSource cts;
    Task t;
    public void OnConnectBtnClkEvent()
    {
        msg = Request("Connect"); // CONNECTED 일 경우 OK

        if(msg == "CONNECTED")
        {
            isConnected = true;

            //cts = new CancellationTokenSource();
            //CancellationToken token = cts.Token;

            //if (isConnected)
            //    t = Task.Run(() => RequestAsync(), token);

            StartCoroutine(CoRequest());
        }
    }

    public void OnDisconnectBtnClkEvent()
    {
        msg = Request("Disconnect"); // DISCONNECTED 일 경우 OK

        //if( msg == "DISCONNECTED" )
        {
            isConnected = false;

            if (!isConnected)
            {
                if(t != null && cts != null)
                    cts.Cancel();
            }
        }
    }

    int i = 0;
    IEnumerator CoRequest()
    {
        while(isConnected)
        {
            // 1. MPS의 X 디바이스 정보를 정수형으로 전달한다.

            // 2. PLC의 Y 디바이스 정보를 2진수 형태로 받는다.

            //string data = Request("temp"); // GET,X0,1 / SET,X0,128

            yield return new WaitForSeconds(interval);

            string returnValue = WriteDevices("X0", 1, xDevices);
        }
    }

    string totalMsg;
    public string WriteDevices(string deviceName, int blockSize, string data)
    {
        totalMsg = "";

        // data = "1101010001000000" or "110101000100000011010100010000001101010001000000" -> 555
        int[] convertedData = new int[blockSize];

        ConvertData(data);

        void ConvertData(string data)
        {
            for (int i = 0; i < blockSize; i++)
            {
                string splited = data.Substring(i * 16, 16);        // 1101010001000000
                splited = Reverse(splited);                         // 0000001000101011(reversed)
                convertedData[i] = Convert.ToInt32(splited, 2);     // 555(10진수 변환)
            }
        }

        // 128,64,266
        foreach(var d in convertedData)
        {
            totalMsg += "," + d;
        }

        // Server로 데이터 전송
        print($"SET,{deviceName},{blockSize}{totalMsg}");
        string returnValue = Request($"SET,{deviceName},{blockSize}{totalMsg}"); // SET,X0,3,128,64,266
        return returnValue;
    }


    public static string Reverse(string input)
    {
        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public void Request()
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(dataInput.text);
        stream.Write(dataBytes, 0, dataBytes.Length);

        // 서버로부터 데이터 읽기
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("서버: " + response);
    }

    public string Request(string message)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(message);
        stream.Write(dataBytes, 0, dataBytes.Length);

        // 서버로부터 데이터 읽기
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        print("서버: " + response);

        return response;
    }

    private async Task RequestAsync()
    {
        while (isConnected)
        {
            try
            {
                WriteDevices("X0", 1, xDevices);

                byte[] buffer = Encoding.UTF8.GetBytes(xDevices);

                // NetworkStream에 데이터 쓰기
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // 데이터 수신(i.g GET,Y0,5)
                byte[] buffer2 = new byte[1024];
                int nBytes = await stream.ReadAsync(buffer2, 0, buffer2.Length);
                string msg = Encoding.UTF8.GetString(buffer2, 0, nBytes);
                print(msg);
            }
            catch (Exception e)
            {
                print(e.ToString());
            }
        }
    }


    private void OnDestroy()
    {
        Request("Disconnect&Quit");
        
        if (isConnected)
        {
            isConnected = false;
        }
    }
}
