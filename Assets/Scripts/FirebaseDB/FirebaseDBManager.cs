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
using TMPro;
using NUnit.Framework.Constraints;
using UnityEditor;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;


// FirebaseDB 예제입니다.
namespace FirebaseDB
{
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
        [SerializeField] TMP_Text infoTxt;
        [SerializeField] TMP_InputField infoInput;
        [SerializeField] TMP_InputField jsonPathInput;
        bool isReceived = false;
        Query query;

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

        public Button btn;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

            SetRawJsonValueAsync(studentInfoJson);

            QueryJsonFile();

            //string btnName = EventSystem.current.currentSelectedGameObject.name;

            //SendObjectByNewtonJson();

            //RequesObjectByNewtonJson();
        }

        public void QueryJsonFile()
        {
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
                                                  },
                                                  ""Array"":[
                                                        1,
                                                        2,
                                                        3
                                                  ]
                                                }
                                              }
                                            }";

            JObject info = JObject.Parse(studentInfoJson);

            string student1Code = (string)info["student"]["0000"]["code"]; // 0000
            print(student1Code);
            string student1English = (string)info["student"]["0000"]["grade"]["English"]; // 50
            print(student1English);
            string student2Array = (string)info["student"]["0001"]["Array"][0]; // 1
            print(student2Array);

            JArray stu2Array = (JArray)info["student"]["0001"]["Array"];
            IList<int> stu2List = stu2Array.Select(value => (int)value).ToList();

            print(stu2List[0]); // 1
            print(stu2List[1]); // 2
        }

        void SetRawJsonValueAsync()
        {
            DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

            // JSON 파일 포멧
            string json = @"{
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
                    print("응답 취소");
                }
                else if(task.IsFaulted)
                {
                    print("응답 실패");
                }
                else if(task.IsCompleted)
                {
                    // 레코드(데이터)를 스냅샷 형태로 가져온다(저장)
                    DataSnapshot snapshot = task.Result;

                    foreach(var data in snapshot.Children) // student의 children 0000, 0001
                    {
                        IDictionary studentData = (IDictionary)data.Value;
                        totalInfo += $"code: {studentData["code"]}\n";

                        DataSnapshot grade = data.Child("grade");
                        IDictionary gradeData = (IDictionary)grade.Value;
                        totalInfo += $"grade: Englise_{gradeData["English"]}" +
                                     $"/Korean_{gradeData["Korean"]}" +
                                     $"/Math_{gradeData["Math"]}" +
                                     $"/Science_{gradeData["Science"]}\n";

                        infoInput.text = (string)gradeData["Math"];
                        jsonPathInput.text = (string)gradeData["Korean"];

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

        IEnumerator UpdateData()
        {
            yield return new WaitUntil(() => isReceived);

            isReceived = false;

            infoTxt.text = totalInfo;
        }

        public void SelectData()
        {
            if(infoInput.text == "")
            {
                print("정보를 입력해 주세요.");
            
                return;
            }

            DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student");

            // code에 해당하는 쿼리 찾기
            query = dbRef.OrderByChild("code").EqualTo(infoInput.text);
            query = dbRef.OrderByChild("postId").EqualTo("0");

            query.ValueChanged += OnDataLoaded;
        }

        private void OnDataLoaded(object sender, ValueChangedEventArgs args)
        {
            DataSnapshot snapshot = args.Snapshot;

            totalInfo = "";

            if (snapshot.ChildrenCount == 0)
            {
                print("해당 코드를 가진 학생 데이터 없음.");
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
                print("삭제하고 싶은 학생 코드를 입력해 주세요.");
                return;
            }

            DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student").Child(infoInput.text);

            dbRef.RemoveValueAsync().ContinueWith(task =>
            {
                if(task.IsFaulted)
                {
                    print("데이터 삭제에 실패하였습니다.");
                }
                else if(task.IsCompleted)
                {
                    isReceived = true;
                }
            });

            StartCoroutine(PrintDataDeleted());
        }

        IEnumerator PrintDataDeleted()
        {
            yield return new WaitUntil(() => isReceived);

            isReceived = false;
             
            print("데이터가 삭제되었습니다.");
        }

        public void InsertData()
        {
            if (!File.Exists(infoInput.text))
            {
                print("파일이 존재하지 않습니다.");
                return;
            }

            using (FileStream fs = new FileStream(jsonPathInput.text, FileMode.Open))
            {
                string fileName = Path.GetFileNameWithoutExtension(jsonPathInput.text);

                using (StreamReader sr = new StreamReader(fs))
                {
                    string json = sr.ReadToEnd();

                    DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student");
                
                    if (dbRef == null)
                        print("올바른 DatabaseReference를 입력해 주세요.");

                    dbRef.Child(fileName).SetRawJsonValueAsync(json);
                }
            }
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
}
