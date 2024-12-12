using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using Unity.Cinemachine;
using System.IO;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using static FirebaseDBManager;
using TMPro;
using NUnit.Framework.Constraints;
using UnityEditor;

/// <summary>
/// Firebase Realtime DB�� �����ؼ� �����͸� ������ �޴´�.
/// �ʿ�Ӽ�: dbURL
/// </summary>
public class FirebaseDBManager : MonoBehaviour
{
    [Serializable]
    public class Library
    {
        public Dictionary<string, Book> library = new Dictionary<string, Book>();
    }

    [Serializable]
    public class Book
    {
        public string name;
        public string bookNumber;
    }

    [SerializeField] string dbURL;
    [SerializeField] List<Book> ������;
    [SerializeField] List<Library> ��������;
    [SerializeField] TMP_Text infoTxt;
    [SerializeField] TMP_InputField infoInput;
    bool isReceived = false;

    string studentInfoJson = @"{
  ""student"" : {
    ""0000"" : {
      ""code"" : ""0000"",
      ""grade"" : {
        ""English"" : 50,
        ""Korean"" : 70,
        ""Math"" : 80,
        ""Science"" : 90
      },
      ""info"" : {
        ""age"" : 10,
        ""gender"" : ""female"",
        ""name"" : ""Ojui_1""
      }
    },
    ""0001"" : {
      ""code"" : ""0001"",
      ""grade"" : {
        ""English"" : 90,
        ""Korean"" : 100,
        ""Math"" : 50,
        ""Science"" : 70
      },
      ""info"" : {
        ""age"" : 11,
        ""gender"" : ""male"",
        ""name"" : ""Ojui_2""
      }
    }
  }
}";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

        SetRawJsonValueAsync(studentInfoJson);

        //SendObjectByNewtonJson();

        //RequesObjectByNewtonJson();
    }

    void SetRawJsonValueAsync()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // JSON ���� ����
        string json = 
        @"{
            ""array"":[
                1,
                2,
                3
            ],
            ""boolean"":true,
            ""color"":""gold"",
            ""null"":null,
            ""number"":123,
            ""object"":{
                ""a"":""b"",
                ""c"":""d""
            },
            ""string"":""Hello World""
        }";

        //dbRef.SetValueAsync("�ȳ��ϼ���.");
        dbRef.SetRawJsonValueAsync(json);
    }

    void SetRawJsonValueAsync(string jsonFile)
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.SetRawJsonValueAsync(jsonFile);
    }
    
    string totalInfo = "";
    public void RequestStudentData()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student");

        totalInfo = "";

        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCanceled)
            {
                print("���� ���");
            }
            else if(task.IsFaulted)
            {
                print("���� ����");
            }
            else if(task.IsCompleted)
            {
                // ���ڵ�(������)�� ������ ���·� �����´�(����)
                DataSnapshot snapshot = task.Result;

                foreach(var data in snapshot.Children) // student�� children 0000, 0001
                {
                    IDictionary studentData = (IDictionary)data.Value;
                    totalInfo += $"code: {studentData["code"]}\n";

                    DataSnapshot grade = data.Child("grade");
                    IDictionary gradeData = (IDictionary)grade.Value;
                    totalInfo += $"grade: Englise_{gradeData["English"]}" +
                                 $"/Korean_{gradeData["Korean"]}" +
                                 $"/Math_{gradeData["Math"]}" +
                                 $"/Science_{gradeData["Science"]}\n";

                    DataSnapshot info = data.Child("info");
                    IDictionary infoData = (IDictionary)info.Value;
                    totalInfo += $"info: Age_{infoData["age"]}" +
                                 $"/Gender_{infoData["gender"]}" +
                                 $"/Name_{infoData["name"]}\n";

                    totalInfo += "--------------------------\n\n";
                }

                print(totalInfo);

                isReceived = true;
            }
        });

        StartCoroutine(UpdateData());
    }

    Query query;
    public void SelectData()
    {
        if(infoInput.text == "")
        {
            print("������ �Է��� �ּ���.");
            
            return;
        }

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student");

        // code�� �ش��ϴ� ���� ã��
        query = dbRef.OrderByChild("code").EqualTo(infoInput.text);

        query.ValueChanged += OnDataLoaded;
    }

    private void OnDataLoaded(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;

        if (snapshot.ChildrenCount == 0)
        {
            print("�ش� �ڵ带 ���� �л� ������ ����.");
        }
        else
        {
            // code: 0000, 0001
            foreach (var data in snapshot.Children)
            {
                IDictionary studentData = (IDictionary)data.Value;
                totalInfo += $"code: {studentData["code"]}\n";

                DataSnapshot grade = data.Child("grade");
                IDictionary gradeData = (IDictionary)grade.Value;
                totalInfo += $"grade: Englise_{gradeData["English"]}" +
                             $"/Korean_{gradeData["Korean"]}" +
                             $"/Math_{gradeData["Math"]}" +
                             $"/Science_{gradeData["Science"]}\n";

                DataSnapshot info = data.Child("info");
                IDictionary infoData = (IDictionary)info.Value;
                totalInfo += $"info: Age_{infoData["age"]}" +
                             $"/Gender_{infoData["gender"]}" +
                             $"/Name_{infoData["name"]}\n";
            }

            infoTxt.text = totalInfo;
        }

        query.ValueChanged -= OnDataLoaded;
    }

    public void DeleteData()
    {
        if(infoTxt.text == "")
        {
            print("�����ϰ� ���� �л� �ڵ带 �Է��� �ּ���.");
            return;
        }

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student").Child(infoInput.text);

        dbRef.RemoveValueAsync().ContinueWith(task =>
        {
            if(task.IsFaulted)
            {
                print("������ ������ �����Ͽ����ϴ�.");
            }
            else if(task.IsCompleted)
            {
                isReceived = true;

                StartCoroutine(PrintDataDeleted());
            }
        });
    }

    IEnumerator PrintDataDeleted()
    {
        yield return new WaitUntil(() => isReceived);

        isReceived = false;
             
        print("�����Ͱ� �����Ǿ����ϴ�.");
    }

    IEnumerator UpdateData()
    {
        yield return new WaitUntil(() => isReceived);

        isReceived = false;

        infoTxt.text = totalInfo;
    }

    void RequestData()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.GetValueAsync().ContinueWith(LoadFunc); // ������ ��û ��, ������ ������ LoadFunc�� ����

        void LoadFunc(Task<DataSnapshot> task)
        {
            if (task.IsCanceled)
            {
                print("DB ��û ���");
            }
            else if (task.IsFaulted)
            {
                print("DB ��û ����");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot item in snapshot.Children)
                {
                    string json = item.GetRawJsonValue();
                    print("DB ���� ������: " + json);
                }

                print("DB ��û �Ϸ�");

                isReceived = true;
            }
        }
    }

    void SendJsonFile(string filePath, string refName)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();

                DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

                dbRef.Child(refName).Child(refName).Child(refName).SetRawJsonValueAsync(json);
            }
        }
    }

    /// <summary>
    /// ����ȭ(Serialization) ����: Book class -> JSON
    /// </summary>
    void SendObjectToChild(string childName)
    {
        Book book = new Book();
        book.name = "���ǿ���";
        book.bookNumber = "1";

        Book book2 = new Book();
        book2.name = "ǻó����";
        book2.bookNumber = "2";

        string jsonBook = JsonUtility.ToJson(book);
        string jsonBook2 = JsonUtility.ToJson(book2);

        print(jsonBook);
        print(jsonBook2);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child(childName).Child("���ǿ���").SetRawJsonValueAsync(jsonBook);
        dbRef.Child(childName).Child("ǻó����").SetRawJsonValueAsync(jsonBook2);
    }

    /// <summary>
    /// ������ȭ(Deserialization) ����: JSON -> Object
    /// </summary>
    void RequestJsonToObject()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        ������ = new List<Book>();

        dbRef.Child("Library").GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var item in snapshot.Children)
                {
                    // JSON -> Object
                    string json = item.GetRawJsonValue();

                    Book book = JsonUtility.FromJson<Book>(json);

                    ������.Add(book);
                }
            }
        });
    }

    /// Object(Library)�� ����(Json)�� ����ȭ �� Firebase DB�� ����
    void SendObjectByNewtonJson()
    {
        Library ��̵����� = new Library();

        Book book = new Book();
        book.name = "�ﱹ��";
        book.bookNumber = "1";

        Book book1 = new Book();
        book1.name = "��ġ��";
        book1.bookNumber = "2";

        ��̵�����.library.Add("�ﱹ��", book);
        ��̵�����.library.Add("��ġ��", book1);

        Library ��ȸ������ = new Library();

        Book book2 = new Book();
        book2.name = "���������";
        book2.bookNumber = "3";

        Book book3 = new Book();
        book3.name = "��������";
        book3.bookNumber = "4";

        ��ȸ������.library.Add("���������", book2);
        ��ȸ������.library.Add("��������", book3);

        // Nested class�� JsonUtility�� ����ȭ �� �� ����. -> Json.Net ��� �ʿ�
        //string json1 = JsonUtility.ToJson(��̵�����); 
        //string json2 = JsonUtility.ToJson(��ȸ������);

        // Json.Net
        string json1 = JsonConvert.SerializeObject(��̵�����);
        string json2 = JsonConvert.SerializeObject(��ȸ������);

        print(json1);
        print(json2);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("����������Ʈ").Child("��̵�����").SetRawJsonValueAsync(json1);
        dbRef.Child("����������Ʈ").Child("��ȸ������").SetRawJsonValueAsync(json2);
    }

    /// <summary>
    /// Firebase DB�� ������(Json)�� Object(Library)�� ������ȭ
    /// </summary>
    void RequesObjectByNewtonJson()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("����������Ʈ").Child("��ȸ������").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var library in snapshot.Children)
                {
                    string json;
                    Library lib = new Library();

                    foreach (var item in library.Children)
                    {
                        json = item.GetRawJsonValue();
                        print(json);
                        Book book = JsonConvert.DeserializeObject<Book>(json);
                        lib.library.Add(item.Key, book);
                        //print($"{item.Key}, {item.Value}");
                        ������.Add(book);
                    }

                    // Jaon -> Object
                    ��������.Add(lib);
                }
            }
        });

        dbRef.Child("����������Ʈ").Child("��̵�����").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var library in snapshot.Children)
                {
                    string json;
                    Library lib = new Library();

                    foreach (var item in library.Children)
                    {
                        json = item.GetRawJsonValue();
                        print(json);
                        Book book = JsonConvert.DeserializeObject<Book>(json);
                        lib.library.Add(item.Key, book);

                        //print($"{item.Key}, {item.Value}");
                        ������.Add(book);
                    }

                    // Jaon -> Object
                    ��������.Add(lib);
                }
            }
        });
    }

    void RequestJsonToDictionary(string refName)
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference(refName);

        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCanceled)
            {
                print("task ��ҵ�");
            }
            else if(task.IsFaulted)
            {
                print("task ������");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach(DataSnapshot item in snapshot.Children)
                {
                    print(item.Key);
                    IDictionary value = (IDictionary)item.Value;
                    string info = $"å ��ȣ: {value["bookNumber"]}, å �̸�: {value["name"]}";
                    print(info);
                }
            }
        });
        
    }
}
