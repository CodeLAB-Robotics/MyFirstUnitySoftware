using UnityEngine;
using ActUtlType64Lib;
using System;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using MPS;
using System.Collections;

/// <summary>
/// MPSManager에게 데이터를 전달하고, 받는 역할
/// </summary>
public class MxComponent : MonoBehaviour
{
    public static MxComponent Instance; // 싱글턴 디자인 패턴

    public enum State
    {
        CONNECTED,
        DISCONNECTED
    }
    public State state = State.DISCONNECTED;

    ActUtlType64 mxComponent;
    [SerializeField] TMP_InputField deviceInput;
    public int xDeviceBlockSize;
    public int yDeviceBlockSize;
    public int dDeviceBlockSize;
    public string xDevices;
    public string yDevices;
    public string dDevices;
    bool isStartBtnClicked = false;
    public bool isDataRead = false;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mxComponent = new ActUtlType64();

        mxComponent.ActLogicalStationNumber = 1;

        for(int i = 0; i < xDeviceBlockSize; i++)
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

    /// <summary>
    /// 실시간으로 PLC 프로그램의 인풋 정보(X 디바이스 정보)를 가져온다.
    /// </summary>
    private void Update()
    {
/*        if (state == State.DISCONNECTED) return;

        //string xDevices = ReadDevices("X0", 1);
        string yDevices = ReadDevices("Y0", 1);

        int 공급실린더전진 = yDevices[0] - '0';
        int 공급실린더후진 = yDevices[1] - '0';

        if(공급실린더전진 == 1)
        {
            mPSManager.cylinders[0].OnForwardBtnClkEvent();
        }
        else if(공급실린더후진 == 1)
        {
            mPSManager.cylinders[0].OnBackwardBtnClkEvent();
        }*/
    }

    public IEnumerator StartReadDevice()
    {
        while(state == State.CONNECTED)
        {
            WriteDevices("X0", 2, xDevices); // Unity -> PLC : Unity 입력 디바이스(x device)들의 정보를 PLC로 전송
            yDevices = ReadDevices("Y0", 2); // PLC -> Unity : PLC의 출력 디바이스(y device)들의 정보를 PC로 전송
            //dDevices = ReadDevices("D0", 1); // 55 or 100

            yield return new WaitForSeconds(0.15f);

            isDataRead = true;
        }
    }

    public void OnSimConnectBtnClkEvent()
    {
        if (state == State.CONNECTED) return;

        int returnValue = mxComponent.Open();


        if(returnValue == 0)
        {
            state = State.CONNECTED;

            StartCoroutine(StartReadDevice());

            print("Simulator와 연결이 잘 되었습니다.");
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러코드를 확인해 주세요. 에러코드: {hexValue}");
        }
    }

    public void OnSimDisconnectBtnClkEvent()
    {
        if (state == State.DISCONNECTED) return;

        int returnValue = mxComponent.Close();

        if (returnValue == 0)
        {
            state = State.DISCONNECTED;

            print("Simulator와 연결이 해제되었습니다.");
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러코드를 확인해 주세요. 에러코드: {hexValue}");
        }

        isDataRead = false;
    }

    public void OnGetDeviceBtnClkEvent()
    {
        int value;
        int returnValue = mxComponent.GetDevice(deviceInput.text, out value);

        if (returnValue == 0)
        {
            print("디바이스 읽기가 완료되었습니다.");

            deviceInput.text = value.ToString();
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");
        }
    }

    public void OnSetDeviceBtnClkEvent()
    {
        // string "X10,1" or "D20,54"
        string[] data = deviceInput.text.Split(",");

        int returnValue = mxComponent.SetDevice(data[0], int.Parse(data[1]));

        if (returnValue == 0)
        {
            print("디바이스 쓰기가 완료되었습니다.");
        }
        else
        {
            print($"에러가 발생하였습니다. 에러코드: {returnValue}");
        }
    }

    public string ReadDevices(string deviceName, int blockSize)
    {
        int[] data = new int[blockSize];
        int returnValue = mxComponent.ReadDeviceBlock(deviceName, blockSize, out data[0]);
        string totalData = ""; // 110010111001111001111001110001111001100001111001100 -> X30 = ?

        if (returnValue == 0)
        {
            print("디바이스 블록 읽기가 완료되었습니다.");

            foreach (int d in data)
            {
                string input = Convert.ToString(d, 2); // D 디바이스: 10진수, 데이터 처리를 위해 사용
                                                       // (생산된 물건의 개수, 생산해야 할 물건의 남은 개수, 불량의 개수)
                                                       // 데이터 예시: 55, 100
                                                       // X, Y 디바이스: 2진수, 비트 단위로 사용
                                                       // 비트 단위 데이터 예시: 1011010, xDevice[0] = 1

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

            return totalData; // 110101000100000 + 110101000100000 
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");

            return hexValue;
        }
    }

    public bool WriteDevices(string deviceName, int blockSize, string data)
    {
        // data = "1101010001000000" or "110101000100000011010100010000001101010001000000" -> 555
        int[] convertedData = new int[blockSize];

        ConvertData(data);

        void ConvertData(string data)
        {
            for (int i = 0; i < blockSize; i++)
            {
                string splited = data.Substring(i*16, 16);        // 1101010001000000
                splited = Reverse(splited);                         // 0000001000101011(reversed)
                convertedData[i] = Convert.ToInt32(splited, 2);     // 555(10진수 변환)
            }
        }

        int returnValue = mxComponent.WriteDeviceBlock(deviceName, blockSize, ref convertedData[0]);

        if (returnValue == 0)
        {
            print("디바이스 블록 쓰기가 완료되었습니다.");

            return true;
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");

            return false;
        }
    }

    public void OnReadDeviceBlockBtnClkEvent()
    {
        int data;
        int returnValue = mxComponent.ReadDeviceBlock(deviceInput.text, 1, out data);

        if (returnValue == 0)
        {
            print("디바이스 블록 읽기가 완료되었습니다.");

            string input = Convert.ToString(data, 2);
            deviceInput.text = Reverse(input);
        }
        else
        {
            print($"에러가 발생하였습니다. 에러코드: {returnValue}");
        }
    }

    public void OnReadDeviceBlockBtnClkEvent(int blockCnt)
    {
        int[] data = new int[blockCnt];
        int returnValue = mxComponent.ReadDeviceBlock(deviceInput.text, blockCnt, out data[0]);
        string totalData = ""; // 110010111001111001111001110001111001100001111001100 -> X30 = ?

        if (returnValue == 0)
        {
            print("디바이스 블록 읽기가 완료되었습니다.");

            foreach(int d in data)
            {
                string input = Convert.ToString(d, 2);
                string reversed = Reverse(input);

                // x[33] = 0 -> x[3][3]
                if(16 - reversed.Length > 0) // 1101010001 -> 110101000100000 
                {
                    int countZero = 16 - reversed.Length;
                    for (int i = 0; i < countZero; i++)
                    {
                        reversed += '0';
                    }
                }

                totalData += reversed;
            }
            print(totalData); // 00011001100
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");
        }
    }

    public void OnWriteDeviceBlockBtnClkEvent()
    {
        OnWriteDeviceBlockBtnClkEvent(5, "11010100010000000010011000000000010101010000000000000010100000000000101000000000");
    }

    public void OnWriteDeviceBlockBtnClkEvent(int blockCnt, string data)
    {
        // data = "1101010001000000" or "110101000100000011010100010000001101010001000000" -> 555
        int[] convertedData = new int[blockCnt];

        ConvertData(data);

        void ConvertData(string data)
        {
            for (int i = 0; i < blockCnt; i++)
            {
                string splited = data.Substring(i * 16, 16);        // 1101010001000000
                splited = Reverse(splited);                         // 0000001000101011(reversed)
                convertedData[i] = Convert.ToInt32(splited, 2);     // 555(10진수 변환)
                print(convertedData[i]);
            }
        }

        int returnValue = mxComponent.WriteDeviceBlock(deviceInput.text, blockCnt, ref convertedData[0]);

        if (returnValue == 0)
        {
            print("디바이스 블록 쓰기가 완료되었습니다.");
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");
        }
    }

    public void OnReadDeviceRandomBtnClkEvent(int blockCnt)
    {
        int[] data = new int[blockCnt + 2];
        int returnValue = mxComponent.ReadDeviceRandom("X0\nX1​", blockCnt, out data[0]);
        string totalData = ""; // 110010111001111001111001110001111001100001111001100 -> X30 = ?

        if (returnValue == 0)
        {
            print("디바이스 블록 읽기가 완료되었습니다.");

            foreach (int d in data)
            {
                string input = Convert.ToString(d, 2);
                string reversed = Reverse(input);

                // x[33] = 0 -> x[3][3]
                if (16 - reversed.Length > 0) // 1101010001 -> 110101000100000 
                {
                    int countZero = 16 - reversed.Length;
                    for (int i = 0; i < countZero; i++)
                    {
                        reversed += '0';
                    }
                }

                totalData += reversed;
            }
            print(totalData); // 00011001100
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");
        }
    }

    public void OnWriteDeviceRandomBtnClkEvent(int blockCnt, string data)
    {
        // data = "1101010001000000" or "110101000100000011010100010000001101010001000000" -> 555
        int[] convertedData = new int[blockCnt];

        ConvertData(data);

        void ConvertData(string data)
        {
            for (int i = 0; i < blockCnt; i++)
            {
                string splited = data.Substring(i * 16, 16);        // 1101010001000000
                splited = Reverse(splited);                         // 0000001000101011(reversed)
                convertedData[i] = Convert.ToInt32(splited, 2);     // 555(10진수 변환)
                print(convertedData[i]);
            }
        }

        int returnValue = mxComponent.WriteDeviceRandom(deviceInput.text, blockCnt, ref convertedData[0]);

        if (returnValue == 0)
        {
            print("디바이스 블록 쓰기가 완료되었습니다.");
        }
        else
        {
            string hexValue = Convert.ToString(returnValue, 16);
            print($"에러가 발생하였습니다. 에러코드: {hexValue}");
        }
    }

    public static string Reverse(string input)
    {
        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    private void OnDestroy()
    {
        OnSimDisconnectBtnClkEvent();
    }
}
