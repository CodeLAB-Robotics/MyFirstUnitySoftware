using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� ������ ���¸� Ȯ��, ��ü�� ����
/// �Ӽ�: �� �����, ��ü������ ����
/// </summary>
public class MPSManager : MonoBehaviour
{
    [SerializeField] List<Cylinder> cylinders = new List<Cylinder>();
    //[SerializeField] List<S> cylinders = new List<Cylinder>();
    [SerializeField] List<MeshRenderer> lamps = new List<MeshRenderer>();
    [SerializeField] Pusher pusher;
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
    }

    public void OnSpawnObjBtnClkEvent()
    {
        if (count > objPrefabs.Length - 1) count = 0;

        Instantiate(objPrefabs[count++], spawnPos.position, Quaternion.identity, transform);
        //obj.transform.position = spawnPos.position;
    }

    public void OnLampOnOffBtnClkEvent(string name)
    {
        Color color;

        switch (name)
        {
            case "Red":
                color = lamps[0].material.GetColor("_BaseColor");
                print(color);

                if(color == redLamp)
                {
                    lamps[0].material.SetColor("_BaseColor", Color.black);
                }
                else if(color == Color.black)
                {
                    lamps[0].material.SetColor("_BaseColor", redLamp);
                }
                break;

            case "Yellow":
                color = lamps[1].material.GetColor("_BaseColor");
                print(color);

                if (color == yellowLamp)
                {
                    lamps[1].material.SetColor("_BaseColor", Color.black);
                }
                else if (color == Color.black)
                {
                    lamps[1].material.SetColor("_BaseColor", yellowLamp);
                }
                break;

            case "Green":
                color = lamps[2].material.GetColor("_BaseColor");
                print(color);

                if (color == greenLamp)
                {
                    lamps[2].material.SetColor("_BaseColor", Color.black);
                }
                else if (color == Color.black)
                {
                    lamps[2].material.SetColor("_BaseColor", greenLamp);
                }
                break;
        }
    }

    public void OnConvCWBtnClkEvent()
    {
        pusher.Move(true);
    }

    public void OnConvCCWBtnClkEvent()
    {
        pusher.Move(false);
    }
}
