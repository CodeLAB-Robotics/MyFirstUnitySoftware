using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �ð��� �ѹ� �ݺ������� ��ü�� �����Ѵ�.
/// �Ӽ� : ��ü, �ð�
/// </summary>
public class Manager : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] GameObject sphere;
    [SerializeField] List<GameObject> spawnedObj = new List<GameObject>();
    [SerializeField] int number;
    [SerializeField] float spawnTime;
    float currentTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spawn(box, number);
        Spawn(sphere, number);
    }

    private void Spawn(GameObject box, int number)
    {
        for(int i = 0; i < number; i++)
        {
            GameObject go = Instantiate(box, transform);
            spawnedObj.Add(go); // ������Ʈ Ǯ���� ���� ��ü ������ ����Ʈ�� �ֱ�

            go.transform.position = new Vector3(Random.Range(-10f, 10f),
                                                Random.Range(0, 20f), Random.Range(-10f, 10f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ������Ʈ Ǯ��: ����ȭ ��� �� �ϳ�, ���ҽ��� �����ϴ� ���
            foreach (GameObject go in spawnedObj)
            {
                go.transform.position = new Vector3(Random.Range(-10f, 10f),
                                    Random.Range(0, 20f), Random.Range(-10f, 10f));
            }
        }

       /* currentTime += Time.deltaTime;

        if (currentTime > spawnTime)
        {
            GameObject go = Instantiate(box, transform);
            go.transform.position = new Vector3(Random.Range(-10f, 10f),
                                                Random.Range(0, 20f), Random.Range(-10f, 10f));

            GameObject go2 = Instantiate(sphere, transform);
            go2.transform.position = new Vector3(Random.Range(-10f, 10f),
                                                Random.Range(0, 20f), Random.Range(-10f, 10f));

            currentTime = 0;
        }*/
    }
}
