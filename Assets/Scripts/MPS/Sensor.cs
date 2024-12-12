using System;
using UnityEngine;

namespace MPS
{
    /// <summary>
    /// 센서의 콜라이더에 금속 또는 비금속 물체가 닿았을 경우 센서를 활성화시킨다.
    /// 속성: 센서타입, 센서 활성화 유무
    /// </summary>
    public class Sensor : MonoBehaviour
    {
        public enum SensorType
        {
            근접센서,
            유도형센서,
            용량형센서
        }
        public SensorType sensorType = SensorType.근접센서;
        public bool isEnabled = false;
        public int cycleCnt;
        public float cycleTime;
        public DateTime lastMaintenanceTime;
        public DateTime nextMaintenanceTime;

        private void OnTriggerStay(Collider other)
        {
            if(sensorType == SensorType.근접센서)
            {
                isEnabled = true;
                print("물체 감지");

                cycleTime += Time.deltaTime;
            }
            else if(sensorType == SensorType.유도형센서)
            {
                if(other.tag == "Metal")
                {
                    isEnabled = true;
                    print("금속 감지");
                }

                cycleTime += Time.deltaTime;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isEnabled)
                isEnabled = false;

            cycleCnt++;
        }
    }
}

