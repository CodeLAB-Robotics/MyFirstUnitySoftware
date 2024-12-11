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
    bool isReceived = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

        //SendObjectByNewtonJson();

        RequesObjectByNewtonJson();
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
