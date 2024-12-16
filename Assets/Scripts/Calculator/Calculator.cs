using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System;

public class Calculator : MonoBehaviour
{
    [SerializeField] TMP_Text input1;
    [SerializeField] TMP_Text input2;
    [SerializeField] List<Button> numbers = new List<Button>();
    [SerializeField] List<Button> basicOperations = new List<Button>();
    [SerializeField] Button clearBtn;
    [SerializeField] string savedStr;
    [SerializeField] string inputStr;
    [SerializeField] bool isOperEnabled = false;
    [SerializeField] int operClickCount;
    [SerializeField] double result;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input1.text = "";
        input2.text = "";

        foreach (var button in numbers)
        {
            TMP_Text numTxt = button.GetComponentInChildren<TMP_Text>();
            button.onClick.AddListener(() => AddString(numTxt.text));
            button.onClick.AddListener(ShowInputNumber);
        }

        foreach (var operation in basicOperations)
        {
            TMP_Text numTxt = operation.GetComponentInChildren<TMP_Text>();
            operation.onClick.AddListener(() => AddString(numTxt.text));
            operation.onClick.AddListener(ShowInputWithOperators);
        }

        clearBtn.onClick.AddListener(ClearInputs);
    }

    public void AddString(string num)
    {
        if (isOperEnabled)
        {
            inputStr = "";
            isOperEnabled = false;
        }

        inputStr += num.ToString();
    }

    public void ClearInputs()
    {
        input1.text = "";
        input2.text = "";
        savedStr = "";
        inputStr = "";
    }

    public void ShowInputNumber()
    {
        input2.text = inputStr;
    }

    public void ShowInputWithOperators()
    {
        savedStr = inputStr;

        input1.text += savedStr;

        isOperEnabled = true;

        operClickCount++;
    }

    public double Add(double numA, double numB)
    {
        return numA + numB;
    }

    public double Subtract(double numA, double numB)
    {
        return numA - numB;
    }

    public double Multiply(double numA, double numB)
    {
        return numA * numB;
    }

    public double Divide(double numA, double numB)
    {
        return numA / numB;
    }

    public void OnCalBtnClkEvent()
    {
        if (input2.text.Length > 0)
        {
            input1.text += input2.text;
        }

        string[] splited;
        char[] newOpers;

        ParseTotalText(out splited, out newOpers);

        result = 0;

        for (int i = 0; i < newOpers.Length; i++)
        {
            switch (newOpers[i])
            {
                case '+':
                    // 3, 6, 3
                    //   +, +
                    // 3+6, 6+3 = 9 + 9 -> 3+6, 3 = 9 + 3
                    result = Add(double.Parse(splited[i]), double.Parse(splited[i + 1])); 
                    print(result);
                    break;
                case '-':
                    result += Subtract(double.Parse(splited[i]), double.Parse(splited[i + 1]));
                    print(result);
                    break;
                case 'x':
                    result += Multiply(double.Parse(splited[i]), double.Parse(splited[i + 1]));
                    print(result);
                    break;
                case '/':
                    result += Divide(double.Parse(splited[i]), double.Parse(splited[i + 1]));
                    print(result);
                    break;

            }
        }

        input2.text = result.ToString();
    }

    private void ParseTotalText(out string[] splited, out char[] newOpers)
    {
        char[] opers = { '+', '-', 'x', '/' };       // 123+45x32
        splited = input1.text.Split(opers);          // {123, 45, 32}
        newOpers = new char[splited.Length - 1];     // {+, x}
        int indexCnt = 0;
        for (int i = 0; i < splited.Length; i++)
        {
            if (i == 0)
            {
                indexCnt += splited[i].Length;
            }
            else
            {
                indexCnt += splited[i].Length + 1;
            }

            if (indexCnt < input1.text.Length)
                newOpers[i] = input1.text[indexCnt];
        }

        foreach (var c in splited)
        {
            print("splited: " + c);
        }

        foreach (var c in newOpers)
        {
            print("oper: " + c);
        }
    }
}
