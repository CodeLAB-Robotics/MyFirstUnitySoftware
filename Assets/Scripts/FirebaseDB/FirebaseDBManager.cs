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
/// Firebase Realtime DB에 접속해서 데이터를 보내고 받는다.
/// 필요속성: dbURL
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
    [SerializeField] List<Book> 도서관;
    [SerializeField] List<Library> 도서관들;
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

        // JSON 파일 포멧
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

        //dbRef.SetValueAsync("안녕하세요.");
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
    /// 직렬화(Serialization) 예제: Book class -> JSON
    /// </summary>
    void SendObjectToChild(string childName)
    {
        Book book = new Book();
        book.name = "사피엔스";
        book.bookNumber = "1";

        Book book2 = new Book();
        book2.name = "퓨처셀프";
        book2.bookNumber = "2";

        string jsonBook = JsonUtility.ToJson(book);
        string jsonBook2 = JsonUtility.ToJson(book2);

        print(jsonBook);
        print(jsonBook2);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child(childName).Child("사피엔스").SetRawJsonValueAsync(jsonBook);
        dbRef.Child(childName).Child("퓨처셀프").SetRawJsonValueAsync(jsonBook2);
    }

    /// <summary>
    /// 역직렬화(Deserialization) 예제: JSON -> Object
    /// </summary>
    void RequestJsonToObject()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        도서관 = new List<Book>();

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

                    도서관.Add(book);
                }
            }
        });
    }

    /// Object(Library)를 정보(Json)로 직렬화 후 Firebase DB로 전송
    void SendObjectByNewtonJson()
    {
        Library 어린이도서관 = new Library();

        Book book = new Book();
        book.name = "삼국지";
        book.bookNumber = "1";

        Book book1 = new Book();
        book1.name = "린치핀";
        book1.bookNumber = "2";

        어린이도서관.library.Add("삼국지", book);
        어린이도서관.library.Add("린치핀", book1);

        Library 국회도서관 = new Library();

        Book book2 = new Book();
        book2.name = "헌법개정안";
        book2.bookNumber = "3";

        Book book3 = new Book();
        book3.name = "민주주의";
        book3.bookNumber = "4";

        국회도서관.library.Add("헌법개정안", book2);
        국회도서관.library.Add("민주주의", book3);

        // Nested class는 JsonUtility로 직렬화 할 수 없음. -> Json.Net 사용 필요
        //string json1 = JsonUtility.ToJson(어린이도서관); 
        //string json2 = JsonUtility.ToJson(국회도서관);

        // Json.Net
        string json1 = JsonConvert.SerializeObject(어린이도서관);
        string json2 = JsonConvert.SerializeObject(국회도서관);

        print(json1);
        print(json2);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("도서관리스트").Child("어린이도서관").SetRawJsonValueAsync(json1);
        dbRef.Child("도서관리스트").Child("국회도서관").SetRawJsonValueAsync(json2);
    }

    /// <summary>
    /// Firebase DB의 정보를(Json)를 Object(Library)로 역직렬화
    /// </summary>
    void RequesObjectByNewtonJson()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("도서관리스트").Child("국회도서관").GetValueAsync().ContinueWith(task =>
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
                        도서관.Add(book);
                    }

                    // Jaon -> Object
                    도서관들.Add(lib);
                }
            }
        });

        dbRef.Child("도서관리스트").Child("어린이도서관").GetValueAsync().ContinueWith(task =>
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
                        도서관.Add(book);
                    }

                    // Jaon -> Object
                    도서관들.Add(lib);
                }
            }
        });
    }

    void RequestData()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.GetValueAsync().ContinueWith(LoadFunc); // 데이터 요청 후, 응답을 받으면 LoadFunc를 실행

        void LoadFunc(Task<DataSnapshot> task)
        {
            if (task.IsCanceled)
            {
                print("DB 요청 취소");
            }
            else if (task.IsFaulted)
            {
                print("DB 요청 실패");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot item in snapshot.Children)
                {
                    string json = item.GetRawJsonValue();
                    print("DB 응답 데이터: " + json);
                }

                print("DB 요청 완료");

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
                print("task 취소됨");
            }
            else if(task.IsFaulted)
            {
                print("task 실패함");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach(DataSnapshot item in snapshot.Children)
                {
                    print(item.Key);
                    IDictionary value = (IDictionary)item.Value;
                    string info = $"책 번호: {value["bookNumber"]}, 책 이름: {value["name"]}";
                    print(info);
                }
            }
        });
        
    }
}
