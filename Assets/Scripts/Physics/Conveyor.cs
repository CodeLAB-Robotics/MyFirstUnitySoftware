using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 센서1이 감지되면, 버튼을 눌렀을 때 Pusher1, 2, 3이 순차적으로 작동하고
/// 최종적으로 센서2가 작동하면 Count를 1 상승
/// 속성: 센서1, 센서2, 버튼, count, Pusher1,2,3, Pusher1,2,3의 목적지들
/// </summary>
public class Conveyor : MonoBehaviour
{
    [SerializeField] int count;
    [SerializeField] Button startButton; 
    [SerializeField] Sensor sensor1;
    [SerializeField] Sensor sensor2;
    [SerializeField] List<Transform> pushers = new List<Transform>();
    [SerializeField] List<Transform> pusherOrigins = new List<Transform>();
    [SerializeField] List<Transform> pusherDests = new List<Transform>();
    [SerializeField] float duration = 2;
    bool isExecutable = false;

    private void Start()
    {
        StartCoroutine(Sensor1Callback());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator RunConveyor()
    {
        for (int i = 0; i < pushers.Count; i++)
        {
            yield return MovePusher(pushers[i], pusherOrigins[i], pusherDests[i], duration, true);

            StartCoroutine(MovePusher(pushers[i], pusherOrigins[i], pusherDests[i], duration, false));
        }
    }

    private IEnumerator MovePusher(Transform pusher, Transform pusherOrigin, Transform pusherDest, float duration, bool isForward)
    {
        float currentTime = 0;

        if (isForward)
        {
            print(pusher.name + " 전진!");

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;

                pusher.position = Vector3.Lerp(pusherOrigin.position, pusherDest.position, currentTime / duration);

                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            print(pusher.name + " 후진!");

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;

                pusher.position = Vector3.Lerp(pusherDest.position, pusherOrigin.position, currentTime / duration);

                yield return new WaitForEndOfFrame();
            }
        }
    }

    Coroutine coroutine;
    public void OnStartBtnClkEvent()
    {
        if (isExecutable)
        {
            print("컨베이어 작동!");

            coroutine = StartCoroutine(RunConveyor());

            StartCoroutine(Sensor2Callback());

            isExecutable = false;
        }
    }

    IEnumerator Sensor1Callback()
    {
        yield return new WaitUntil(() => sensor1.IsSensored);

        isExecutable = true;
    }

    IEnumerator Sensor2Callback()
    {
        yield return new WaitUntil(() => sensor2.IsSensored);

        StartCoroutine(Sensor1Callback());

        count++;
    }
}
