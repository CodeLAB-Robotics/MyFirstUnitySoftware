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
            print("서버를 먼저 작동시켜 주세요.");
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
            print("이미 연결된 상태입니다.");
        }
    }

    private string Request(string message)
    {
        // 데이터 송신
        byte[] requestData = Encoding.UTF8.GetBytes(message);
        stream.Write(requestData, 0, requestData.Length);

        // 데이터 수신
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
            print("이미 연결해지 상태입니다.");
        }
    }

    // 코루틴 사용 동기화 요청 버튼
    public void OnSyncRequestBtnClkEvent()
    {
        if(isConnected)
        {
            print("SyncRequest 메서드가 실행되었습니다.");
            StartCoroutine(CoSyncRequest());
        }
        else
        {
            print("서버와 연결을 먼저 해주세요.");
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

    // Task 사용 비동기 요청 버튼
    public void OnAsyncRequestBtnClkEvent()
    {
        if(isConnected)
        {
            print("AsyncRequest가 시작되었습니다.");

            Task.Run(() => RequestAsync());
        }
        else
        {
            print("서버와 연결을 먼저 해주세요.");
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
