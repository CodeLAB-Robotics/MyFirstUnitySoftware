using System.Collections;
using UnityEngine;

/// <summary>
/// ������ ���޵Ǹ� �Ǹ��� �ε尡 ���� �Ÿ���ŭ, ���� �ӵ��� ���� �Ǵ� �����Ѵ�.
/// ���� �Ǵ� ���� ��, ������ Limit Switch(LS)�� �۵��Ѵ�.
/// �Ӽ�: �Ǹ����ε�, Min-Max Range, Duration, ���Ĺ� Limit Switch
/// </summary>
public class Cylinder : MonoBehaviour
{
    [SerializeField] Transform cylinderRod;
    [SerializeField] float maxRange;
    [SerializeField] float minRange;
    [SerializeField] float duration;
    [SerializeField] Transform forwardLS;
    [SerializeField] Transform backwardLS;
    [SerializeField] bool isForward = false;
    [SerializeField] bool isRodMoving = false;

    public void OnForwardBtnClkEvent()
    {
        if (isForward || isRodMoving) return;

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

            yield return new WaitForEndOfFrame();
        }

        isRodMoving = false;
        isForward = !isForward;
    }

}
