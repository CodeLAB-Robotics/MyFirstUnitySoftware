using UnityEngine;

/// <summary>
/// RobotController 스크립트의 현재 step정보에서 isSuctionOn이 true일 때, 충돌된 물체를 Suction에 붙인다.
/// 필요속성: RobotController 스크립트
/// </summary>
public class Suction : MonoBehaviour
{
    [SerializeField] RobotController robotController;
    [SerializeField] bool isAttached = false;
    Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
        if (robotController.steps.Count == 0) return;

        // 현재 스탭의 Suction 상태가 Off 일때, 충돌한 물체를 원래 속성으로 바꿔줌
        if (isAttached && !robotController.steps[robotController.currentStepNumber].isSuctionOn)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            rb.transform.SetParent(null);

            isAttached = false;
        }
    }

    // RobotController 스크립트의 현재 step정보에서 isSuctionOn이 true일 때, 충돌된 물체를 Suction에 붙인다.
    private void OnTriggerEnter(Collider other)
    {
        if (robotController.steps.Count == 0) return;

        // RobotController 스크립트의 현재 step정보에서 isSuctionOn이 true일 때
        if (robotController.steps[robotController.currentStepNumber].isSuctionOn)
        {
            if(other.tag.Contains("Metal") || other.tag.Contains("NonMetal"))
            {
                isAttached = true;

                rb = other.GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;

                // 출돌 후의 속도와 각속도를 제거해준다.
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;

                other.transform.SetParent(transform);
            }
        }
    }
}
