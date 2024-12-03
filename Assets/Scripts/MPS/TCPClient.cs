using System.Net.Sockets;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using TMPro;

public class TCPClient : MonoBehaviour
{
    [SerializeField] TMP_InputField dataInput;
    TcpClient client;
    NetworkStream stream;

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
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void Request(string message)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(message);
        stream.Write(dataBytes, 0, dataBytes.Length);

        // �����κ��� ������ �б�
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("����: " + response);
    }
}
