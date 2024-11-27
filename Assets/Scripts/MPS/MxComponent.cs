using UnityEngine;
using ActUtlType64Lib;
using System;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class MxComponent : MonoBehaviour
{
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
        int returnValue = mxComponent.Open();

        string hexValue = Convert.ToString(returnValue, 16);

        if(hexValue == "0")
        {
            print("Simulator�� ������ �� �Ǿ����ϴ�.");
        }
        else
        {
            print($"�����ڵ带 Ȯ���� �ּ���. �����ڵ�: {hexValue}");
        }
    }

    public void OnSimDisconnectBtnClkEvent()
    {
        int returnValue = mxComponent.Close();

        string hexValue = Convert.ToString(returnValue, 16);

        if (hexValue == "0")
        {
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
