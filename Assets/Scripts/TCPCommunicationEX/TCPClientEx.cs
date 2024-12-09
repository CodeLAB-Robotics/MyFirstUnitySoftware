using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class TCPClientEx : MonoBehaviour
{
    public TMP_InputField input;
    public float waitTime = 0.5f;
    public Transform obj;
    public Transform pointA;
    public Transform pointB;
    public float speed;
    public bool isLeft;
    TcpClient client;
    NetworkStream stream;
    bool isConnected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 7000);

            stream = client.GetStream();

            input.text = "GET,Y0,2";
        }
        catch (Exception e)
        {
            print(e);
            print("������ ���� �۵����� �ּ���.");
        }
    }

    Vector3 direction;
    // Update is called once per frame
    void Update()
    {
        if (isLeft)
        {
            direction = pointA.position - obj.position;
            float distance = direction.magnitude;

            if (distance < 0.1f)
            {
                isLeft = false;
            }
        }
        else
        {
            direction = pointB.position - obj.position;
            float distance = direction.magnitude;

            if (distance < 0.1f)
            {
                isLeft = true;
            }
        }

        obj.position += direction.normalized * speed * Time.deltaTime;
    }

    public void OnConnectBtnClkEvent()
    {
        if(!isConnected)
        {
            isConnected = true;

            string ret = Request("Connect");

            print(ret);
        }
        else
        {
            print("�̹� ����� �����Դϴ�.");
        }
    }

    private string Request(string message)
    {
        // ������ �۽�
        byte[] requestData = Encoding.UTF8.GetBytes(message);
        stream.Write(requestData, 0, requestData.Length);

        // ������ ����
        byte[] buffer = new byte[1024];
        int nBytes = stream.Read(buffer, 0, buffer.Length);
        string responseData = Encoding.UTF8.GetString(buffer, 0, nBytes);

        return responseData;
    }

    public void OnDisconnectBtnClkEvent()
    {
        if (isConnected)
        {
            isConnected = false;

            string ret = Request("Disconnect");

            print(ret);
        }
        else
        {
            print("�̹� �������� �����Դϴ�.");
        }
    }

    // �ڷ�ƾ ��� ����ȭ ��û ��ư
    public void OnSyncRequestBtnClkEvent()
    {
        if(isConnected)
        {
            print("SyncRequest �޼��尡 ����Ǿ����ϴ�.");
            StartCoroutine(CoSyncRequest());
        }
        else
        {
            print("������ ������ ���� ���ּ���.");
        }
    }

    IEnumerator CoSyncRequest()
    {
        while(isConnected)
        {
            // GET,Y0,2
            string ret = Request(input.text);
            print(ret);

            if (ret.Contains("Quit"))
                yield break;

            yield return new WaitForSeconds(waitTime);
        }
    }

    // Task ��� �񵿱� ��û ��ư
    public void OnAsyncRequestBtnClkEvent()
    {
        if(isConnected)
        {
            print("AsyncRequest�� ���۵Ǿ����ϴ�.");

            Task.Run(() => RequestAsync());
        }
        else
        {
            print("������ ������ ���� ���ּ���.");
        }
    }

    // async, await
    private async Task RequestAsync()
    {
        while(isConnected)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input.text);

            await stream.WriteAsync(buffer, 0, buffer.Length);

            byte[] buffer2 = new byte[1024];
            int nBytes = await stream.ReadAsync(buffer2, 0, buffer2.Length);
            string message = Encoding.UTF8.GetString(buffer2, 0, nBytes);
            print(message);

            if (message.Contains("Quit"))
                break;
        }
    }
}
