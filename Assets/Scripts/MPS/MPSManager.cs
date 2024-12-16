// Master에서만 사용하는 전처리기
#define TCPServerVersion // MasterMode(MxComponentVersion or TCPServerVersion) or SlaveMode

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FirebaseDBManager = MPS.FirebaseDBManager;

namespace MPS
{
    /// <summary>
    /// 모든 설비의 상태를 확인, 물체를 생성
    /// 속성: 각 설비들, 물체프리팹 변수
    /// </summary>
    public class MPSManager : MonoBehaviour
    {
        public class ProductLine
        {
            public string id;
            public bool isRunning;
            public float duration;
            public bool lsForward;
            public bool lsBackward;
            public int cycleCnt;
            public float cycleTime;
            public string lastMaintenanceTime;
            public string nextMaintenanceTime;

            public ProductLine(string id)
            {
                this.id = id;
            }
        }

        public class PalletizingLine
        {
            public string id;
            public bool isRunning;
            public int stepCnt;
            public int cycleCnt;
            public float cycleTime;
            public string lastMaintenanceTime;
            public string nextMaintenanceTime;

            public PalletizingLine(string id)
            {
                this.id = id;
            }
        }

        public class Inventory
        {
            public int expectedProducts;
            public int finishedProducts;
            public int failedProducts;
            public int rawMaterial;
        }

        public class EnergyConsumption
        {
            public float mps;
            public float robots;
            public float total;
        }

        public class EnvironmentData
        {
            public float temperture;
            public float humidity;
            public float airQulity;
        }


        [Header("Facilities")]
        [SerializeField] List<Cylinder> cylinders = new List<Cylinder>();
        [SerializeField] List<MeshRenderer> lamps = new List<MeshRenderer>();
        [SerializeField] List<Pusher> pushers = new List<Pusher>();
        [SerializeField] List<Sensor> sensors = new List<Sensor>();
        [SerializeField] List<RobotController> robotController = new List<RobotController>();
        [SerializeField] int startBtnState = 0;
        [SerializeField] int stopBtnState = 0;
        [SerializeField] int eStopBtnState = 0;
        public bool isConveyorRunning;
        public int convyorCycleCnt;
        DateTime convyorLastMaintenanceTime;
        DateTime convyorNextMaintenanceTime;

        [Space(20)]
        [Header("Etc")]
        [SerializeField] GameObject[] objPrefabs;
        [SerializeField] Transform spawnPos;

        [Space(20)]
        [Header("DB Data")]
        // 본 데이터
        public DateTime timeStamp;
        public string id = "Smart Factory 1";
        public bool isRunning;
        public List<ProductLine> productLines = new List<ProductLine>();
        public List<PalletizingLine> palletizingLines = new List<PalletizingLine>();
        public Inventory inventory = new Inventory();
        public EnergyConsumption energyConsumption = new EnergyConsumption();
        public EnvironmentData environmentData = new EnvironmentData();
        
        // 임시 데이터
        public ProductLine 컨베이어Data = new ProductLine("컨베이어");
        public ProductLine 공급센서Data = new ProductLine("공급센서");
        public ProductLine 물체확인센서Data = new ProductLine("물체확인센서");
        public ProductLine 금속확인센서Data = new ProductLine("금속확인센서");
        public ProductLine 공급실린더Data = new ProductLine("공급실린더");
        public ProductLine 가공실린더Data = new ProductLine("가공실린더");
        public ProductLine 송출실린더Data = new ProductLine("송출실린더");
        public ProductLine 배출실린더Data = new ProductLine("배출실린더");
        public PalletizingLine 로봇AData = new PalletizingLine("RobotA");
        public PalletizingLine 로봇BData = new PalletizingLine("RobotB");

        int count;
        Color redLamp;
        Color yellowLamp;
        Color greenLamp;

        public GameObject 게시판main;
        public GameObject 게시글;

        private void Awake()
        {
            redLamp = lamps[0].material.GetColor("_BaseColor");
            yellowLamp = lamps[1].material.GetColor("_BaseColor");
            greenLamp = lamps[2].material.GetColor("_BaseColor");

            OnLampOnOffBtnClkEvent("Red", false);
            OnLampOnOffBtnClkEvent("Yellow", false);
            OnLampOnOffBtnClkEvent("Green", false);


            timeStamp = DateTime.Now;
            productLines.Add(공급실린더Data);
            productLines.Add(가공실린더Data);
            productLines.Add(송출실린더Data);
            productLines.Add(배출실린더Data);
            productLines.Add(컨베이어Data);
            productLines.Add(공급센서Data);
            productLines.Add(물체확인센서Data);
            productLines.Add(금속확인센서Data);
            palletizingLines.Add(로봇AData);
            palletizingLines.Add(로봇BData);
        }

        private void Update()
        {
            UpdateYDevices();
            UpdateXDevices();
            UpdateDDevices();

#if MxComponentVersion || TCPServerVersion
            UpdateDBData(); // MPS 설비의 정보를 최신화(DB에 들어갈 내용을 업데이트)
#endif
            void UpdateYDevices()
            {
#if MxComponentVersion
                if (MxComponent.Instance.state == MxComponent.State.DISCONNECTED)
                    return;

                if (MxComponent.Instance.yDevices.Length == 0 || !MxComponent.Instance.isDataRead) return;

                int 공급실린더전진 = MxComponent.Instance.yDevices[0] - '0';
                int 공급실린더후진 = MxComponent.Instance.yDevices[1] - '0';
                int 가공실린더전진 = MxComponent.Instance.yDevices[2] - '0';
                int 가공실린더후진 = MxComponent.Instance.yDevices[3] - '0';
                int 송출실린더전진 = MxComponent.Instance.yDevices[4] - '0';
                int 송출실린더후진 = MxComponent.Instance.yDevices[5] - '0';
                int 배출실린더전진 = MxComponent.Instance.yDevices[6] - '0';
                int 배출실린더후진 = MxComponent.Instance.yDevices[7] - '0';
                int 컨베이어CW회전 = MxComponent.Instance.yDevices[8] - '0';
                int 컨베이어CCW회전 = MxComponent.Instance.yDevices[9] - '0';
                int 컨베이어STOP = MxComponent.Instance.yDevices[10] - '0';      // Y0A
                int 빨강램프 = MxComponent.Instance.yDevices[11] - '0';          // Y0B
                int 노랑램프 = MxComponent.Instance.yDevices[12] - '0';          // Y0C
                int 초록램프 = MxComponent.Instance.yDevices[13] - '0';          // Y0D
                int 로봇A싱글사이클 = MxComponent.Instance.yDevices[14] - '0';   // Y0E
                int 로봇A오리진 = MxComponent.Instance.yDevices[15] - '0';       // Y0F
                int 로봇B싱글사이클 = MxComponent.Instance.yDevices[16] - '0';   // Y10
                int 로봇B오리진 = MxComponent.Instance.yDevices[17] - '0';       // Y11
#elif TCPServerVersion
                if (TCPClient.Instance.isConnected == false)
                    return;

                if (TCPClient.Instance.yDevices.Length == 0) return;

                int 공급실린더전진  = TCPClient.Instance.yDevices[0] - '0';
                int 공급실린더후진  = TCPClient.Instance.yDevices[1] - '0';
                int 가공실린더전진  = TCPClient.Instance.yDevices[2] - '0';
                int 가공실린더후진  = TCPClient.Instance.yDevices[3] - '0';
                int 송출실린더전진  = TCPClient.Instance.yDevices[4] - '0';
                int 송출실린더후진  = TCPClient.Instance.yDevices[5] - '0';
                int 배출실린더전진  = TCPClient.Instance.yDevices[6] - '0';
                int 배출실린더후진  = TCPClient.Instance.yDevices[7] - '0';
                int 컨베이어CW회전  = TCPClient.Instance.yDevices[8] - '0';
                int 컨베이어CCW회전 = TCPClient.Instance.yDevices[9] - '0';
                int 컨베이어STOP    = TCPClient.Instance.yDevices[10] - '0';
                int 빨강램프        = TCPClient.Instance.yDevices[11] - '0';
                int 노랑램프        = TCPClient.Instance.yDevices[12] - '0';
                int 초록램프        = TCPClient.Instance.yDevices[13] - '0';
                int 로봇A싱글사이클 = TCPClient.Instance.yDevices[14] - '0';   // Y0E
                int 로봇A오리진     = TCPClient.Instance.yDevices[15] - '0';       // Y0F
                int 로봇B싱글사이클 = TCPClient.Instance.yDevices[16] - '0';   // Y10
                int 로봇B오리진     = TCPClient.Instance.yDevices[17] - '0';       // Y11

#elif SlaveMode
                if (isRunning == false)
                    return;

                int 공급실린더전진  = FirebaseDBManager.Instance.yDevices[0]  - '0';
                int 공급실린더후진  = FirebaseDBManager.Instance.yDevices[1]  - '0';
                int 가공실린더전진  = FirebaseDBManager.Instance.yDevices[2]  - '0';
                int 가공실린더후진  = FirebaseDBManager.Instance.yDevices[3]  - '0';
                int 송출실린더전진  = FirebaseDBManager.Instance.yDevices[4]  - '0';
                int 송출실린더후진  = FirebaseDBManager.Instance.yDevices[5]  - '0';
                int 배출실린더전진  = FirebaseDBManager.Instance.yDevices[6]  - '0';
                int 배출실린더후진  = FirebaseDBManager.Instance.yDevices[7]  - '0';
                int 컨베이어CW회전  = FirebaseDBManager.Instance.yDevices[8]  - '0';
                int 컨베이어CCW회전 = FirebaseDBManager.Instance.yDevices[9]  - '0';
                int 컨베이어STOP    = FirebaseDBManager.Instance.yDevices[10] - '0';
                int 빨강램프        = FirebaseDBManager.Instance.yDevices[11] - '0';
                int 노랑램프        = FirebaseDBManager.Instance.yDevices[12] - '0';
                int 초록램프        = FirebaseDBManager.Instance.yDevices[13] - '0';
                int 로봇A싱글사이클 = FirebaseDBManager.Instance.yDevices[14] - '0';   // Y0E
                int 로봇A오리진     = FirebaseDBManager.Instance.yDevices[15] - '0';   // Y0F
                int 로봇B싱글사이클 = FirebaseDBManager.Instance.yDevices[16] - '0';   // Y10
                int 로봇B오리진     = FirebaseDBManager.Instance.yDevices[17] - '0';   // Y11
#endif

                if (공급실린더전진 == 1) cylinders[0].OnForwardBtnClkEvent();
                else if (공급실린더후진 == 1) cylinders[0].OnBackwardBtnClkEvent();

                if (가공실린더전진 == 1) cylinders[1].OnForwardBtnClkEvent();
                else if (가공실린더후진 == 1) cylinders[1].OnBackwardBtnClkEvent();

                if (송출실린더전진 == 1) cylinders[2].OnForwardBtnClkEvent();
                else if (송출실린더후진 == 1) cylinders[2].OnBackwardBtnClkEvent();

                if (배출실린더전진 == 1) cylinders[3].OnForwardBtnClkEvent();
                else if (배출실린더후진 == 1) cylinders[3].OnBackwardBtnClkEvent();

                if (컨베이어CW회전 == 1)
                {
                    foreach (var pusher in pushers)
                    {
                        pusher.Move(true);
                    }

                    UpdateDBDataCW();

                    void UpdateDBDataCW()
                    {
                        if (!isConveyorRunning)
                        {
                            isConveyorRunning = true;
                            컨베이어Data.isRunning = isConveyorRunning;
                            컨베이어Data.lsForward = true;
                            convyorCycleCnt++;
                        }

                        컨베이어Data.cycleTime += Time.deltaTime;
                    }
                }
                else if (컨베이어CCW회전 == 1)
                {
                    foreach (var pusher in pushers)
                    {
                        pusher.Move(false);
                    }

                    UpdateDBDataCCW();

                    void UpdateDBDataCCW()
                    {
                        if (!isConveyorRunning)
                        {
                            isConveyorRunning = true;
                            컨베이어Data.isRunning = isConveyorRunning;
                            컨베이어Data.lsBackward = true;
                            convyorCycleCnt++;
                        }

                        컨베이어Data.cycleTime += Time.deltaTime;
                    }
                }

                if (컨베이어STOP == 1)
                {
                    foreach (var pusher in pushers)
                    {
                        pusher.Stop();
                    }

                    UpdateDBDateStop();

                    void UpdateDBDateStop()
                    {
                        if (isConveyorRunning)
                        {
                            isConveyorRunning = false;
                            컨베이어Data.isRunning = isConveyorRunning;
                            컨베이어Data.lsForward = false;
                            컨베이어Data.lsBackward = false;

                            convyorCycleCnt++;
                        }
                    }
                }

                if (빨강램프 == 1) OnLampOnOffBtnClkEvent("Red", true);
                else OnLampOnOffBtnClkEvent("Red", false);

                if (노랑램프 == 1) OnLampOnOffBtnClkEvent("Yellow", true);
                else OnLampOnOffBtnClkEvent("Yellow", false);

                if (초록램프 == 1) OnLampOnOffBtnClkEvent("Green", true);
                else OnLampOnOffBtnClkEvent("Green", false);

                if(로봇A싱글사이클 == 1) // 동시에 3번 작동
                    robotController[0].OnSingleCycleBtnClkEvent();

                if(로봇A오리진 == 1) robotController[0].OnOriginBtnClkEvent();

                if (로봇B싱글사이클 == 1) robotController[1].OnSingleCycleBtnClkEvent();

                if (로봇B오리진 == 1) robotController[1].OnOriginBtnClkEvent();
            }
            void UpdateXDevices()
            {
#if MxComponentVersion
                if (MxComponent.Instance.state == MxComponent.State.DISCONNECTED)
                    return;

                if (MxComponent.Instance.yDevices.Length == 0 || !MxComponent.Instance.isDataRead) return;

                // PLC의 x device를 업데이트
                // 만약 xDevice의 블록 수가 두 번째 블록부터는 0이 16개 들어가야 함.
                string xDeviceValue = $"{startBtnState}" +                                    // 시작버튼 상태    (X0)
                                      $"{stopBtnState}" +                                     // 정지버튼         (X1)
                                      $"{eStopBtnState}" +                                    // 긴급정지버튼     (X2) 
                                      $"{(sensors[0].isEnabled == true ? 1 : 0)}" +           // 공급센서         (X3)
                                      $"{(sensors[1].isEnabled == true ? 1 : 0)}" +           // 물체확인센서     (X4)
                                      $"{(sensors[2].isEnabled == true ? 1 : 0)}" +           // 금속확인센서     (X5)
                                      $"{(cylinders[0].isForwardLSOn == true ? 1 : 0)}" +     // 공급실린더 전진LS(X6)
                                      $"{(cylinders[0].isBackwardLSOn == true ? 1 : 0)}" +    // 공급실린더 후진LS(X7)
                                      $"{(cylinders[1].isForwardLSOn == true ? 1 : 0)}" +     // 가공실린더 전진LS(X8)
                                      $"{(cylinders[1].isBackwardLSOn == true ? 1 : 0)}" +    // 가공실린더 후진LS(X9)
                                      $"{(cylinders[2].isForwardLSOn == true ? 1 : 0)}" +     // 송출실린더 전진LS(X0A)
                                      $"{(cylinders[2].isBackwardLSOn == true ? 1 : 0)}" +    // 송출실린더 후진LS(X0B)
                                      $"{(cylinders[3].isForwardLSOn == true ? 1 : 0)}" +     // 배출실린더 전진LS(X0C)
                                      $"{(cylinders[3].isBackwardLSOn == true ? 1 : 0)}" +    // 배출실린더 후진LS(X0D)
                                      $"{(robotController[0].isRunning == true ? 1 : 0)}" +   // 현재 로봇의 작동여부(X0E)
                                      $"{(robotController[1].isRunning == true ? 1 : 0)}";    // 현재 로봇의 작동여부(X0F) 
                
                for (int i = 1; i < MxComponent.Instance.xDeviceBlockSize; i++)
                {
                    xDeviceValue += "0000000000000000";
                }

                MxComponent.Instance.xDevices = xDeviceValue;
#elif TCPServerVersion

                // PLC의 x device를 업데이트
                // 만약 xDevice의 블록 수가 두 번째 블록부터는 0이 16개 들어가야 함.
                string xDeviceValue = $"{startBtnState}" +                                    // 시작버튼 상태    (X0)
                                      $"{stopBtnState}" +                                     // 정지버튼         (X1)
                                      $"{eStopBtnState}" +                                    // 긴급정지버튼     (X2) 
                                      $"{(sensors[0].isEnabled == true ? 1 : 0)}" +           // 공급센서         (X3)
                                      $"{(sensors[1].isEnabled == true ? 1 : 0)}" +           // 물체확인센서     (X4)
                                      $"{(sensors[2].isEnabled == true ? 1 : 0)}" +           // 금속확인센서     (X5)
                                      $"{(cylinders[0].isForwardLSOn == true ? 1 : 0)}" +     // 공급실린더 전진LS(X6)
                                      $"{(cylinders[0].isBackwardLSOn == true ? 1 : 0)}" +    // 공급실린더 후진LS(X7)
                                      $"{(cylinders[1].isForwardLSOn == true ? 1 : 0)}" +     // 가공실린더 전진LS(X8)
                                      $"{(cylinders[1].isBackwardLSOn == true ? 1 : 0)}" +    // 가공실린더 후진LS(X9)
                                      $"{(cylinders[2].isForwardLSOn == true ? 1 : 0)}" +     // 송출실린더 전진LS(X0A)
                                      $"{(cylinders[2].isBackwardLSOn == true ? 1 : 0)}" +    // 송출실린더 후진LS(X0B)
                                      $"{(cylinders[3].isForwardLSOn == true ? 1 : 0)}" +     // 배출실린더 전진LS(X0C)
                                      $"{(cylinders[3].isBackwardLSOn == true ? 1 : 0)}" +    // 배출실린더 후진LS(X0D)
                                      $"{(robotController[0].isRunning == true ? 1 : 0)}" +   // 현재 로봇의 작동여부(X0E)
                                      $"{(robotController[1].isRunning == true ? 1 : 0)}";    // 현재 로봇의 작동여부(X0F) 


                for (int i = 1; i < TCPClient.Instance.xDeviceBlockSize; i++)
                {
                    xDeviceValue += "0000000000000000";
                }

                TCPClient.Instance.xDevices = xDeviceValue;

                isRunning = startBtnState == 1 ? true : false;

                공급센서Data.isRunning     = sensors[0].isEnabled;
                물체확인센서Data.isRunning = sensors[1].isEnabled;
                금속확인센서Data.isRunning = sensors[2].isEnabled;

                공급실린더Data.isRunning   = cylinders[0].isRodMoving;
                공급실린더Data.lsForward   = cylinders[0].isForwardLSOn;
                공급실린더Data.lsBackward  = cylinders[0].isBackwardLSOn;
                공급실린더Data.cycleCnt    = cylinders[0].cycleCnt;
                공급실린더Data.cycleTime   = cylinders[0].cycleTime;

                가공실린더Data.isRunning   = cylinders[1].isRodMoving;
                가공실린더Data.lsForward   = cylinders[1].isForwardLSOn;
                가공실린더Data.lsBackward  = cylinders[1].isBackwardLSOn;
                가공실린더Data.cycleCnt    = cylinders[1].cycleCnt;
                가공실린더Data.cycleTime   = cylinders[1].cycleTime;

                송출실린더Data.isRunning   = cylinders[2].isRodMoving;
                송출실린더Data.lsForward   = cylinders[2].isForwardLSOn;
                송출실린더Data.lsBackward  = cylinders[2].isBackwardLSOn;
                송출실린더Data.cycleCnt    = cylinders[2].cycleCnt;
                송출실린더Data.cycleTime   = cylinders[2].cycleTime;

                배출실린더Data.isRunning   = cylinders[3].isRodMoving;
                배출실린더Data.lsForward   = cylinders[3].isForwardLSOn;
                배출실린더Data.lsBackward  = cylinders[3].isBackwardLSOn;
                배출실린더Data.cycleCnt    = cylinders[3].cycleCnt;
                배출실린더Data.cycleTime   = cylinders[3].cycleTime;

                로봇AData.isRunning        = robotController[0].isRunning;
                로봇BData.isRunning        = robotController[1].isRunning;
#endif
            }
            void UpdateDDevices()
            {
#if MxComponentVersion
                if (MxComponent.Instance.state == MxComponent.State.DISCONNECTED)
                    return;

                if (MxComponent.Instance.dDevices.Length == 0) return;

                print(MxComponent.Instance.dDevices);
                print(Convert.ToInt32(MxComponent.Instance.dDevices, 2));
#elif TCPServerVersion
                if (TCPClient.Instance.isConnected == false)
                    return;

                if (TCPClient.Instance.dDevices.Length == 0) return;

                print(TCPClient.Instance.dDevices);
                //print(Convert.ToInt32(TCPClient.Instance.dDevices, 2));
#endif
            }
            void UpdateDBData()
            { 
                isRunning = (startBtnState == 1) ? true : false;

                공급센서Data.isRunning = sensors[0].isEnabled;
                물체확인센서Data.isRunning = sensors[1].isEnabled;
                금속확인센서Data.isRunning = sensors[2].isEnabled;

                공급실린더Data.isRunning = cylinders[0].isRodMoving;
                공급실린더Data.lsForward = cylinders[0].isForwardLSOn;
                공급실린더Data.lsBackward = cylinders[0].isBackwardLSOn;
                공급실린더Data.cycleCnt = cylinders[0].cycleCnt;
                공급실린더Data.cycleTime = cylinders[0].cycleTime;

                가공실린더Data.isRunning = cylinders[1].isRodMoving;
                가공실린더Data.lsForward = cylinders[1].isForwardLSOn;
                가공실린더Data.lsBackward = cylinders[1].isBackwardLSOn;
                가공실린더Data.cycleCnt = cylinders[1].cycleCnt;
                가공실린더Data.cycleTime = cylinders[1].cycleTime;

                송출실린더Data.isRunning = cylinders[2].isRodMoving;
                송출실린더Data.lsForward = cylinders[2].isForwardLSOn;
                송출실린더Data.lsBackward = cylinders[2].isBackwardLSOn;
                송출실린더Data.cycleCnt = cylinders[2].cycleCnt;
                송출실린더Data.cycleTime = cylinders[2].cycleTime;

                배출실린더Data.isRunning = cylinders[3].isRodMoving;
                배출실린더Data.lsForward = cylinders[3].isForwardLSOn;
                배출실린더Data.lsBackward = cylinders[3].isBackwardLSOn;
                배출실린더Data.cycleCnt = cylinders[3].cycleCnt;
                배출실린더Data.cycleTime = cylinders[3].cycleTime;

                로봇AData.isRunning = robotController[0].isRunning;
                로봇AData.cycleCnt = robotController[0].cycleCnt;
                로봇AData.cycleTime = robotController[0].cycleTime;
                로봇BData.isRunning = robotController[1].isRunning;
                로봇BData.cycleCnt = robotController[1].cycleCnt;
                로봇BData.cycleTime = robotController[1].cycleTime;
            }
        }

        public void OnSpawnObjBtnClkEvent()
        {
            if (count > objPrefabs.Length - 1) count = 0;

            Instantiate(objPrefabs[count++], spawnPos.position, Quaternion.identity, transform);
            //obj.transform.position = spawnPos.position;
        }

        public void OnLampOnOffBtnClkEvent(string name, bool isActive)
        {
            Color color;

            switch (name)
            {
                case "Red":
                    color = lamps[0].material.GetColor("_BaseColor");

                    if (color == redLamp && !isActive)
                    {
                        lamps[0].material.SetColor("_BaseColor", Color.black);
                    }
                    else if (color == Color.black && isActive)
                    {
                        lamps[0].material.SetColor("_BaseColor", redLamp);
                    }
                    break;

                case "Yellow":
                    color = lamps[1].material.GetColor("_BaseColor");

                    if (color == yellowLamp && !isActive)
                    {
                        lamps[1].material.SetColor("_BaseColor", Color.black);
                    }
                    else if (color == Color.black && isActive)
                    {
                        lamps[1].material.SetColor("_BaseColor", yellowLamp);
                    }
                    break;

                case "Green":
                    color = lamps[2].material.GetColor("_BaseColor");

                    if (color == greenLamp && !isActive)
                    {
                        lamps[2].material.SetColor("_BaseColor", Color.black);
                    }
                    else if (color == Color.black && isActive)
                    {
                        lamps[2].material.SetColor("_BaseColor", greenLamp);
                    }
                    break;
            }
        }

        public void OnConvCWBtnClkEvent()
        {
            foreach (var pusher in pushers)
                pusher.Move(true);
        }

        public void OnConvCCWBtnClkEvent()
        {
            foreach (var pusher in pushers)
                pusher.Move(false);
        }

        public void OnConvStopBtnClkEvent()
        {
            foreach (var pusher in pushers)
                pusher.Stop();
        }

        public void OnStartBtnClkEvent()
        {
            startBtnState = 1;
            stopBtnState = 0;
        }

        public void OnStopBtnClkEvent()
        {
            stopBtnState = 1;
            startBtnState = 0;
        }

        public void OnEStopBtnClkEvent()
        {
            eStopBtnState = (eStopBtnState == 1) ? 0 : 1;
            startBtnState = 0;
            stopBtnState = 0;
        }
    }

}