using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPS
{
    /// <summary>
    /// 모든 설비의 상태를 확인, 물체를 생성
    /// 속성: 각 설비들, 물체프리팹 변수
    /// </summary>
    public class MPSManager : MonoBehaviour
    {
        [Header("Facilities")]
        [SerializeField] List<Cylinder> cylinders = new List<Cylinder>();
        [SerializeField] List<MeshRenderer> lamps = new List<MeshRenderer>();
        [SerializeField] List<Pusher> pushers = new List<Pusher>();
        [SerializeField] List<Sensor> sensors = new List<Sensor>();
        [SerializeField] int startBtnState = 0;
        [SerializeField] int stopBtnState = 0;
        [SerializeField] int eStopBtnState = 0;

        [Space(20)]
        [Header("Etc")]
        [SerializeField] GameObject[] objPrefabs;
        [SerializeField] Transform spawnPos;
        int count;
        Color redLamp;
        Color yellowLamp;
        Color greenLamp;

        private void Start()
        {
            redLamp = lamps[0].material.GetColor("_BaseColor");
            yellowLamp = lamps[1].material.GetColor("_BaseColor");
            greenLamp = lamps[2].material.GetColor("_BaseColor");

            OnLampOnOffBtnClkEvent("Red", false);
            OnLampOnOffBtnClkEvent("Yellow", false);
            OnLampOnOffBtnClkEvent("Green", false);
        }

        private void Update()
        {
            UpdateYDevices();
            UpdateXDevices();
            UpdateDDevices();

            void UpdateYDevices()
            {
                if (MxComponent.Instance.state == MxComponent.State.DISCONNECTED)
                    return;

                if (MxComponent.Instance.yDevices.Length == 0) return;

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
                int 컨베이어STOP = MxComponent.Instance.yDevices[10] - '0';
                int 빨강램프 = MxComponent.Instance.yDevices[11] - '0';
                int 노랑램프 = MxComponent.Instance.yDevices[12] - '0';
                int 초록램프 = MxComponent.Instance.yDevices[13] - '0';

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
                }
                else if (컨베이어CCW회전 == 1)
                {
                    foreach (var pusher in pushers)
                    {
                        pusher.Move(false);
                    }
                }

                if (컨베이어STOP == 1)
                {
                    foreach (var pusher in pushers)
                    {
                        pusher.Stop();
                    }
                }

                if (빨강램프 == 1) OnLampOnOffBtnClkEvent("Red", true);
                else OnLampOnOffBtnClkEvent("Red", false);

                if (노랑램프 == 1) OnLampOnOffBtnClkEvent("Yellow", true);
                else OnLampOnOffBtnClkEvent("Yellow", false);

                if (초록램프 == 1) OnLampOnOffBtnClkEvent("Green", true);
                else OnLampOnOffBtnClkEvent("Green", false);
            }

            void UpdateXDevices()
            {
                // PLC의 x device를 업데이트
                MxComponent.Instance.xDevices = $"{startBtnState}" +                                 // 시작버튼 상태    (X0)
                                                $"{stopBtnState}" +                                 // 정지버튼         (X1)
                                                $"{eStopBtnState}" +                                 // 긴급정지버튼     (X2) 
                                                $"{(sensors[0].isEnabled == true ? 1 : 0)}" +        // 공급센서         (X3)
                                                $"{(sensors[1].isEnabled == true ? 1 : 0)}" +        // 물체확인센서     (X4)
                                                $"{(sensors[2].isEnabled == true ? 1 : 0)}" +        // 금속확인센서     (X5)
                                                $"{(cylinders[0].isForwardLSOn == true ? 1 : 0)}" + // 공급실린더 전진LS(X6)
                                                $"{(cylinders[0].isBackwardLSOn == true ? 1 : 0)}" + // 공급실린더 후진LS(X7)
                                                $"{(cylinders[1].isForwardLSOn == true ? 1 : 0)}" + // 가공실린더 전진LS(X8)
                                                $"{(cylinders[1].isBackwardLSOn == true ? 1 : 0)}" + // 가공실린더 후진LS(X9)
                                                $"{(cylinders[2].isForwardLSOn == true ? 1 : 0)}" + // 송출실린더 전진LS(X0A)
                                                $"{(cylinders[2].isBackwardLSOn == true ? 1 : 0)}" + // 송출실린더 후진LS(X0B)
                                                $"{(cylinders[3].isForwardLSOn == true ? 1 : 0)}" + // 배출실린더 전진LS(X0C)
                                                $"{(cylinders[3].isBackwardLSOn == true ? 1 : 0)}" + // 배출실린더 후진LS(X0D)
                                                "00";                                                // 사용하지 않는 나머지 2비트(X0E, X0F)
            }

            void UpdateDDevices()
            {
                if (MxComponent.Instance.state == MxComponent.State.DISCONNECTED)
                    return;

                if (MxComponent.Instance.dDevices.Length == 0) return;

                print(MxComponent.Instance.dDevices);
                print(Convert.ToInt32(MxComponent.Instance.dDevices, 2));
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