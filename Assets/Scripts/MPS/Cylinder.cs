using System;
using System.Collections;
using UnityEngine;

namespace MPS
{
    /// <summary>
    /// 공압이 공급되면 실린더 로드가 일정 거리만큼, 일정 속도로 전진 또는 후진한다.
    /// 전진 또는 후진 시, 전후진 Limit Switch(LS)가 작동한다.
    /// 속성: 실린더로드, Min-Max Range, Duration, 전후방 Limit Switch
    /// </summary>
    public class Cylinder : MonoBehaviour
    {
        [SerializeField] Transform cylinderRod;
        [SerializeField] float maxRange;
        [SerializeField] float minRange;
        [SerializeField] float duration;
        [SerializeField] Transform forwardLS;
        [SerializeField] Transform backwardLS;
        public bool isForward = false;
        public bool isRodMoving = false;
        public bool isForwardLSOn = false;
        public bool isBackwardLSOn = false;
        public int cycleCnt;
        public float cycleTime;
        public string lastMaintenanceTime; // 2024-12-12
        public string nextMaintenanceTime; // 2025-12-12
        MeshRenderer forwardMR;
        MeshRenderer backwardMR;
        Color lsOffColor; // red

        private void Start()
        {
            // 캐싱 & 래퍼런싱
            forwardMR = forwardLS.GetComponent<MeshRenderer>();
            backwardMR = backwardLS.GetComponent<MeshRenderer>();

            lsOffColor = forwardMR.material.color;

            // LS 초기화
            isBackwardLSOn = true;
            backwardMR.material.SetColor("_BaseColor", Color.green);
        }

        public void OnForwardBtnClkEvent()
        {
            if (isForward || isRodMoving) return;

            cycleCnt++;

            StartCoroutine(MoveCylinder(cylinderRod, minRange, maxRange, duration));
        }

        public void OnBackwardBtnClkEvent()
        {
            if (!isForward || isRodMoving) return;

            StartCoroutine(MoveCylinder(cylinderRod, maxRange, minRange, duration));
        }

        IEnumerator MoveCylinder(Transform rod, float min, float max, float duration)
        {
            isRodMoving = true;

            Vector3 minPos = new Vector3(rod.transform.localPosition.x, min, rod.transform.localPosition.z);
            Vector3 maxPos = new Vector3(rod.transform.localPosition.x, max, rod.transform.localPosition.z);

            float currentTime = 0;

            while (currentTime <= duration)
            {
                currentTime += Time.deltaTime;

                rod.localPosition = Vector3.Lerp(minPos, maxPos, currentTime / duration);

                cycleTime += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            isRodMoving = false;
            isForward = !isForward;

            UpdateLimitSwitch(isForward);
        }

        /// <summary>
        /// 실린더 윗쪽의 LS 색상 변경
        /// </summary>
        /// <param name="isForward">전진 또는 후진인지 설정하는 bool 변수</param>
        private void UpdateLimitSwitch(bool isForward)
        {
            if(isForward)
            {
                isForwardLSOn = true;
                isBackwardLSOn = false;

                // 실린더가 전진 상태일 때, 전진 LS의 색상을 녹색으로 변경
                forwardMR.material.SetColor("_BaseColor", Color.green);
                backwardMR.material.SetColor("_BaseColor", lsOffColor);
            }
            else
            {
                isForwardLSOn = false;
                isBackwardLSOn = true;

                // 실린더가 후진 상태일 때, 전진 LS의 색상을 녹색으로 변경
                backwardMR.material.SetColor("_BaseColor", Color.green);
                forwardMR.material.SetColor("_BaseColor", lsOffColor);
            }
        }
    }

}