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
using System.Collections.Generic;
using Unity.VisualScripting;

public class TCPClient : MonoBehaviour
{
    public static TCPClient Instance;

    [SerializeField] TMP_InputField dataInput;
    [SerializeField] string dataToServer;
    [SerializeField] string dataFromServer;
    public bool isConnected;
    public float interval;
    public int xDeviceBlockSize;
    public int yDeviceBlockSize;
    public int dDeviceBlockSize;
    public string xDevices;
    public string yDevices;
    public string dDevices;

    TcpClient client;
    NetworkStream stream;
    string msg;
    string totalMsg;

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
            // 1. ������ ����
            client = new TcpClient("127.0.0.1", 12345);
            print("������ �����");

            // 2. ��Ʈ��ũ ��Ʈ�� ���
            stream = client.GetStream();
        }
        catch(Exception ex)
        {
            print(ex);
            print("������ ���� �۵����� �ּ���.");
        }


        for (int i = 0; i < xDeviceBlockSize; i++)
        {
            xDevices += "0000000000000000";
        }

        for (int i = 0; i < yDeviceBlockSize; i++)
        {
            yDevices += "0000000000000000";
        }

        for (int i = 0; i < dDeviceBlockSize; i++)
        {
            dDevices += "0000000000000000";
        }
    }

    CancellationTokenSource cts;
    Task t;
    public void OnConnectBtnClkEvent()
    {
        msg = Request("Connect"); // CONNECTED �� ��� OK

        if(msg == "CONNECTED")
        {
            isConnected = true;

            StartCoroutine(CoRequest());
        }
    }

    public void OnDisconnectBtnClkEvent()
    {
        msg = Request("Disconnect"); // DISCONNECTED �� ��� OK

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

    public bool isDataCorrect;
    IEnumerator CoRequest()
    {
        yield return new WaitUntil(() => isConnected);

        Task.Run(() => RequestAsync());

        while (isConnected)
        {
            //string data = Request("temp"); // GET,X0,1 / SET,X0,128


            // SET,X0,128,GET,Y0,2,GET,D0,3 -> ������ ���� -> WriteDeviceBlock 1��, ReadDeviceBlock 1��
            
            // 1. MPS�� X ����̽� ������ ���������� �����Ѵ�.
            WriteDevices("X0", 2, xDevices); // SET,X0,3,12,68,44

            // 2. PLC�� Y, D ����̽� ������ 2���� ���·� �޴´�.

            yDevices = ReadDevices("Y0", 2); //  GET,Y0,2

            //dDevices = ReadDevices("D0", 1); //  GET,D0,1


            // 3. ����: �������� �����͸� �ְ� ���� �� ���ϴ� �����͸� �ޱ�
            // (Unity to Server ������ ����: SET,X0,3,128,24,1/GET,Y0,2/GET,D0,3)
            // (Server to Unity ������ ����: X0,123,24/D0,23
            //Request($"SET,X0,1,{xDevices}/GET,Y0,2/GET,D0,1");

            yield return new WaitForSeconds(interval);
        }
    }

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
                convertedData[i] = Convert.ToInt32(splited, 2);     // 555(10���� ��ȯ)
            }
        }

        // 128,64,266
        foreach(var d in convertedData)
        {
            totalMsg += "," + d;
        }

        // Server�� ������ ����
        dataToServer = $"SET,{deviceName},{blockSize}{totalMsg},GET,X0,2";
        //dataToServer = Request($"SET,{deviceName},{blockSize}{totalMsg}"); // SET,X0,3,128,64,266
        return dataToServer;
    }

    public string ReadDevices(string deviceName, int blockSize)
    {
        // "33,22" or "128"
        string returnValue = dataFromServer; // GET,X0,3
        
        int[] data = new int[blockSize];
        string totalData = "";

        if (returnValue != "�����ڵ�")
        {
            print("����̽� ��� �бⰡ �Ϸ�Ǿ����ϴ�.");

            // Correct: 8193,0
            //data = returnValue.ToIntArray(); // { 33, 22 } 
            string[] strArray = returnValue.Split(','); // { 33, 22 } 
            for(int i = 0; i < data.Length; i++)
            {
                isDataCorrect = int.TryParse(strArray[i], out data[i]);
                if (!isDataCorrect) return "Error";
            }

            foreach (int d in data)
            {
                string input = Convert.ToString(d, 2); // D ����̽�: 10����, ������ ó���� ���� ���
                                                       // (����� ������ ����, �����ؾ� �� ������ ���� ����, �ҷ��� ����)
                                                       // ������ ����: 55, 100
                                                       // X, Y ����̽�: 2����, ��Ʈ ������ ���
                                                       // ��Ʈ ���� ������ ����: 1011010, xDevice[0] = 1

                if (!deviceName.Contains("D")) // deviceName = "Y0", "X0"
                {
                    input = Reverse(input);

                    // x[33] = 0 -> x[3][3]
                    if (16 - input.Length > 0) // 1101010001 -> 110101000100000 
                    {
                        int countZero = 16 - input.Length;
                        for (int i = 0; i < countZero; i++)
                        {
                            input += '0';
                        }
                    }
                }

                totalData += input;
            }

            return totalData; // 00011001100
        }
        else
        {
            return returnValue;
        }
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

        // �����κ��� ������ �б�
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("����: " + response);
    }

    public string Request(string message)
    {
        try
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(dataBytes, 0, dataBytes.Length);

            // �����κ��� ������ �б�
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            print("����: " + response);

            return response;
        }
        catch (Exception ex)
        {
            print(ex);

            return ex.ToString();
        }
    }

    private async Task RequestAsync()
    {
        while (true)
        {
            try
            {
                // dataToServer�� ���� GET,Y0,4,SET,Y0,0,170;
                byte[] buffer = Encoding.UTF8.GetBytes(dataToServer);

                // NetworkStream�� ������ ����
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // ������ ����(i.g GET,Y0,5)
                byte[] buffer2 = new byte[1024];
                int nBytes = await stream.ReadAsync(buffer2, 0, buffer2.Length);

                // dataFromServer = "0,0,0,170"
                dataFromServer = Encoding.UTF8.GetString(buffer2, 0, nBytes);

                print("RequestAsync: " + dataFromServer);

                if (!isConnected) break;
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
