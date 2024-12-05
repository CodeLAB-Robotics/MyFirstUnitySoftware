using UnityEngine;

/// <summary>
/// RobotController ��ũ��Ʈ�� ���� step�������� isSuctionOn�� true�� ��, �浹�� ��ü�� Suction�� ���δ�.
/// �ʿ�Ӽ�: RobotController ��ũ��Ʈ
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

        // ���� ������ Suction ���°� Off �϶�, �浹�� ��ü�� ���� �Ӽ����� �ٲ���
        if (isAttached && !robotController.steps[robotController.currentStepNumber].isSuctionOn)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            rb.transform.SetParent(null);

            isAttached = false;
        }
    }

    // RobotController ��ũ��Ʈ�� ���� step�������� isSuctionOn�� true�� ��, �浹�� ��ü�� Suction�� ���δ�.
    private void OnTriggerEnter(Collider other)
    {
        if (robotController.steps.Count == 0) return;

        // RobotController ��ũ��Ʈ�� ���� step�������� isSuctionOn�� true�� ��
        if (robotController.steps[robotController.currentStepNumber].isSuctionOn)
        {
            if(other.tag.Contains("Metal") || other.tag.Contains("NonMetal"))
            {
                isAttached = true;

                rb = other.GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;

                // �⵹ ���� �ӵ��� ���ӵ��� �������ش�.
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;

                other.transform.SetParent(transform);
            }
        }
    }
}
