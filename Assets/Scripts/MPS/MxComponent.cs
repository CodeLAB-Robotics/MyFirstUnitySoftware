using UnityEngine;
using ActUtlType64Lib;
using System;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class MxComponent : MonoBehaviour
{
    public enum State
    {
        CONNECTED,
        DISCONNECTED
    }
    State state = State.DISCONNECTED;

    ActUtlType64 mxComponent;
    [SerializeField] TMP_InputField deviceInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mxComponent = new ActUtlType64();

        mxComponent.ActLogicalStationNumber = 1;
    }

    public void OnSimConnectBtnClkEvent()
    {
        if (state == State.CONNECTED) return;

        int returnValue = mxComponent.Open();

        string hexValue = Convert.ToString(returnValue, 16);

        if(hexValue == "0")
        {
            state = State.CONNECTED;

            print("Simulator�� ������ �� �Ǿ����ϴ�.");
        }
        else
        {
            print($"�����ڵ带 Ȯ���� �ּ���. �����ڵ�: {hexValue}");
        }
    }

    public void OnSimDisconnectBtnClkEvent()
    {
        if (state == State.DISCONNECTED) return;

        int returnValue = mxComponent.Close();

        string hexValue = Convert.ToString(returnValue, 16);

        if (hexValue == "0")
        {
            state = State.DISCONNECTED;

            print("Simulator�� ������ �����Ǿ����ϴ�.");
        }
        else
        {
            print($"�����ڵ带 Ȯ���� �ּ���. �����ڵ�: {hexValue}");
        }
    }

    public void OnGetDeviceBtnClkEvent()
    {
        int value;
        int returnValue = mxComponent.GetDevice(deviceInput.text, out value);

        if (returnValue == 0)
        {
            print("����̽� �бⰡ �Ϸ�Ǿ����ϴ�.");

            deviceInput.text = value.ToString();
        }
        else
        {
            print($"������ �߻��Ͽ����ϴ�. �����ڵ�: {returnValue}");
        }
    }

    public void OnSetDeviceBtnClkEvent()
    {
        // string "X10,1" or "D20,54"
        string[] data = deviceInput.text.Split(",");

        int returnValue = mxComponent.SetDevice(data[0], int.Parse(data[1]));

        if (returnValue == 0)
        {
            print("����̽� ���Ⱑ �Ϸ�Ǿ����ϴ�.");
        }
        else
        {
            print($"������ �߻��Ͽ����ϴ�. �����ڵ�: {returnValue}");
        }
    }

    public void OnReadDeviceBlockBtnClkEvent()
    {
        int data;
        int returnValue = mxComponent.ReadDeviceBlock(deviceInput.text, 1, out data);

        if (returnValue == 0)
        {
            print("����̽� ��� �бⰡ �Ϸ�Ǿ����ϴ�.");

            string input = Convert.ToString(data, 2);
            deviceInput.text = Reverse(input);
        }
        else
        {
            print($"������ �߻��Ͽ����ϴ�. �����ڵ�: {returnValue}");
        }
    }

    public void OnReadDeviceBlockBtnClkEvent(int blockCnt)
    {
        int[] data = new int[blockCnt];
        int returnValue = mxComponent.ReadDeviceBlock(deviceInput.text, blockCnt, out data[0]);
        string totalData = ""; // 110010111001111001111001110001111001100001111001100 -> X30 = ?

        if (returnValue == 0)
        {
            print("����̽� ��� �бⰡ �Ϸ�Ǿ����ϴ�.");

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
            print(totalData[38]); // 1
        }
        else
        {
            print($"������ �߻��Ͽ����ϴ�. �����ڵ�: {returnValue}");
        }
    }

    public void OnWriteDeviceBlockBtnClkEvent(int blockCnt, string data)
    {
        // data = "1000100010011101" or "100010001001110110001000100111011000100010011101"
        int[] convertedData = new int[blockCnt];

        int returnValue = mxComponent.WriteDeviceBlock(deviceInput.text, blockCnt, ref convertedData[0]);

        if (returnValue == 0)
        {
            print("����̽� ��� ���Ⱑ �Ϸ�Ǿ����ϴ�.");
        }
        else
        {
            print($"������ �߻��Ͽ����ϴ�. �����ڵ�: {returnValue}");
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
