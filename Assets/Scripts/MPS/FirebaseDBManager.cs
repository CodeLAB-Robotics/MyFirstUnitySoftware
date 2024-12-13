using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using Newtonsoft.Json;
using System.Text;
using static MPS.MPSManager;

namespace MPS
{
    /// <summary>
    /// MPS Scene에서 사용되는 데이터 작업을 위한 FirebaseDBManager 입니다.
    /// MPSManager의 일정 시간 간격으로 설비 데이터를 FirebaseDB에 저장하고 불러온다.
    /// </summary>
    public class FirebaseDBManager : MonoBehaviour
    {
        public string dbURL;
        public MPSManager mPSManager;
        public float updateInterval = 1f;
        string dataFormat = @"{
  ""EnergyConsumption"": [
    {
      ""mps"": 0,
      ""robots"": 0,
      ""total"": 0
    }
  ],
  ""EnvironmentData"": [
    {
      ""airQulity"": 0,
      ""humidity"": 0,
      ""temperture"": 0
    }
  ],
  ""Inventory"": [
    {
      ""expectedProducts"": 0,
      ""failedProducts"": 0,
      ""finishedProducts"": 0,
      ""rawMaterial"": 0
    }
  ],
  ""PalletizingLines"": [
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""id"": ""RobotA"",
      ""isRunning"": false,
      ""stepCnt"": 0
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""id"": ""RobotB"",
      ""isRunning"": false,
      ""stepCnt"": 0
    }
  ],
  ""ProductLines"": [
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""공급실린더"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""가공실린더"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""송출실린더"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""배출실린더"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""컨베이어"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""공급센서"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    },
    {
      ""cycleCnt"": 0,
      ""cycleTime"": 0,
      ""duration"": 0,
      ""id"": ""물체확인센서"",
      ""isRunning"": false,
      ""lsBackward"": false,
      ""lsForward"": false
    }
  ],
  ""id"": ""Smart Factory 1"",
  ""isRunning"": false,
  ""timeStamp"": ""2024-12-13 오전 10:26:54""
}";
        DatabaseReference dbRef;
        StringBuilder sb = new StringBuilder();

        private void Start()
        {
            FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(dbURL);

            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

            if (dbRef != null)
            {
                StartCoroutine(CoUploadData());
            }

            //InvokeRepeating("InitializeData", 0, 3);
            //InitializeData();
            UploadData();
        }

        void InitializeData()
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            
            if (dbRef != null)
            {
                sb.Clear();

                sb.Append($"{{\"timeStamp\":\"{mPSManager.timeStamp.ToString()}\",");
                sb.Append($"\"id\":\"{mPSManager.id}\",");
                sb.Append($"\"isRunning\":{(mPSManager.isRunning == true ? "true" : "false")},");
                sb.Append($"\"ProductLines\":[{JsonConvert.SerializeObject(mPSManager.productLines[0])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[1])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[2])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[3])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[4])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[5])}," +
                    $"{JsonConvert.SerializeObject(mPSManager.productLines[6])}],");
                sb.Append($"\"PalletizingLines\":[{JsonConvert.SerializeObject(mPSManager.palletizingLines[0])}," +
                          $"{JsonConvert.SerializeObject(mPSManager.palletizingLines[1])}],");
                sb.Append($"\"Inventory\":[{JsonConvert.SerializeObject(mPSManager.inventory)}],");
                sb.Append($"\"EnergyConsumption\":[{JsonConvert.SerializeObject(mPSManager.energyConsumption)}],");
                sb.Append($"\"EnvironmentData\":[{JsonConvert.SerializeObject(mPSManager.environmentData)}]}}");

                string json = sb.ToString();
                print(json);

                //print(dataFormat);
                dbRef.SetRawJsonValueAsync(json);
            }
        }

        public void LoadData()
        {

        }

        IEnumerator CoUploadData()
        {
            yield return new WaitUntil(() => mPSManager.isRunning);

            while (mPSManager.isRunning)
            {
                UploadData();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        public void UploadData()
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

            //string json = $@"{{
            //    ""timeStamp"":""{mPSManager.timeStamp.ToString()}"",
            //    ""id"":""{mPSManager.id}"",
            //    ""isRunning"":{mPSManager.isRunning},
            //    ""ProductLines"":[{JsonConvert.SerializeObject(mPSManager.productLines[0])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[1])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[2])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[3])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[4])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[5])},
            //                      {JsonConvert.SerializeObject(mPSManager.productLines[6])}  
            //    ],
            //    ""PalletizingLines"":[{JsonConvert.SerializeObject(mPSManager.palletizingLines[0])},
            //                         {JsonConvert.SerializeObject(mPSManager.palletizingLines[1])}
            //    ],
            //    ""Inventory"":{JsonConvert.SerializeObject(mPSManager.inventory)},
            //    ""EnergyConsumption"":{JsonConvert.SerializeObject(mPSManager.energyConsumption)},
            //    ""EnvironmentData"":{JsonConvert.SerializeObject(mPSManager.environmentData)}
            //}}";

            sb.Clear();

            sb.Append("{{");
            sb.Append($"\"timeStamp\":\"{mPSManager.timeStamp.ToString()}\",");
            sb.Append($"\"id\":\"{mPSManager.id}\",");
            sb.Append($"\"isRunning\":{(mPSManager.isRunning == true ? "true" : "false")},"); // Json 형식에는 bool 형식: 소문자(O) 대문자(X) 
            sb.Append($"\"ProductLines\":[{JsonConvert.SerializeObject(mPSManager.productLines[0])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[1])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[2])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[3])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[4])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[5])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.productLines[6])}],");
            sb.Append($"\"PalletizingLines\":[{JsonConvert.SerializeObject(mPSManager.palletizingLines[0])}," +
                      $"{JsonConvert.SerializeObject(mPSManager.palletizingLines[1])}],");
            sb.Append($"\"Inventory\":[{JsonConvert.SerializeObject(mPSManager.inventory)}],");
            sb.Append($"\"EnergyConsumption\":[{JsonConvert.SerializeObject(mPSManager.energyConsumption)}],");
            sb.Append($"\"EnvironmentData\":[{JsonConvert.SerializeObject(mPSManager.environmentData)}]");
            sb.Append("}}");

            string json = sb.ToString();
            print(json);

            dbRef.SetRawJsonValueAsync(json);
        }
    }
}
