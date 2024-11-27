using UnityEngine;

namespace MPS
{
    /// <summary>
    /// ������ �ݶ��̴��� �ݼ� �Ǵ� ��ݼ� ��ü�� ����� ��� ������ Ȱ��ȭ��Ų��.
    /// �Ӽ�: ����Ÿ��, ���� Ȱ��ȭ ����
    /// </summary>
    public class Sensor : MonoBehaviour
    {
        public enum SensorType
        {
            ��������,
            ����������,
            �뷮������
        }
        public SensorType sensorType = SensorType.��������;
        public bool isEnabled = false;

        private void OnTriggerStay(Collider other)
        {
            if(sensorType == SensorType.��������)
            {
                isEnabled = true;
                print("��ü ����");
            }
            else if(sensorType == SensorType.����������)
            {
                if(other.tag == "Metal")
                {
                    isEnabled = true;
                    print("�ݼ� ����");
                }
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (isEnabled)
                isEnabled = false;
        }
    }
}

