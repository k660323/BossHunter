using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerInfo
{
    public string monsterName;
    public int curCnt;
    public int maxCnt;
}


public class MonsterSpawner : MonoBehaviour
{
    // �� ����
    [SerializeField]
    List<GameObject> monsters = new List<GameObject>();

    // ������
    [SerializeField]
    List<int> spawnCounts = new List<int>();

    // ���� ���� ����
    Dictionary<Define.MonsterType, MonsterSpawnerInfo> monsterDic = new Dictionary<Define.MonsterType, MonsterSpawnerInfo>();

    [SerializeField]
    float spawnRadius = 10.0f;

    [SerializeField]
    bool isSpawnContinue = false;

    [ServerCallback]
    private void Start()
    {
        if (monsters.Count != spawnCounts.Count)
        {
            Debug.LogWarning("�� ������ �� ���� ������ ���� �ʽ��ϴ�.");
            this.enabled = false;
            return;
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            int cnt = spawnCounts[i];
            Define.MonsterType monsterType = Define.MonsterType.Max;

            for (int j = 0; j < spawnCounts[i]; j++)
            {
                GameObject monsterObject = Managers.Resource.NetInstantiate(monsters[i].name, gameObject.scene, true);
                monsterObject.transform.position = GetSpawnPos();
                monsterObject.transform.rotation = Quaternion.identity;

                if (isSpawnContinue && monsterObject.TryGetComponent(out Monster m))
                {
                    monsterType = m.GetMonsterStat.MonsterType;
                    // �ݹ��Լ� ���
                    m.destoryAction -= GenCountDown;
                    m.destoryAction += GenCountDown;
                }

                NetworkServer.Spawn(monsterObject);
            }

            if (isSpawnContinue)
            {
                if (monsterDic.ContainsKey(monsterType) == false)
                    monsterDic.Add(monsterType, new MonsterSpawnerInfo() { monsterName = monsters[i].name, curCnt = cnt, maxCnt = cnt });
                else
                {
                    Debug.LogWarning("�ߺ� Ű �߻�");
                    continue;
                }
            }
        }

        if (isSpawnContinue)
            StartCoroutine(MonsterSpawnCor(new WaitForSeconds(60.0f)));
    }

    public void GenCountDown(Define.MonsterType monsterType)
    {
        if(monsterDic.ContainsKey(monsterType))
        {
            monsterDic[monsterType].curCnt--;
        }
    }

    [ServerCallback]
    Vector3 GetSpawnPos()
    {
        Vector3 rayStartPos = transform.position + new Vector3(Random.Range(-spawnRadius, spawnRadius), 100.0f, Random.Range(-spawnRadius, spawnRadius));
        PhysicsScene physicsScene = gameObject.scene.GetPhysicsScene();
        if(physicsScene.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 200.0f, Managers.LayerManager.Ground))
        {
            return hit.point;
        }

        return transform.position;
    }

    IEnumerator MonsterSpawnCor(WaitForSeconds waitForSeconds)
    {
        while(true)
        {
            yield return waitForSeconds;
            foreach (var pair in monsterDic)
            {
                int spawnCnt = pair.Value.maxCnt - pair.Value.curCnt;
                for (int i = 0; i < spawnCnt; i++)
                {
                    GameObject monsterObject = Managers.Resource.NetInstantiate(pair.Value.monsterName, gameObject.scene, true);
                    monsterObject.transform.position = GetSpawnPos();
                    monsterObject.transform.rotation = Quaternion.identity;
                    if (monsterObject.TryGetComponent(out Monster m))
                    {
                        // �ݹ��Լ� ���
                        m.destoryAction -= GenCountDown;
                        m.destoryAction += GenCountDown;
                    }

                    NetworkServer.Spawn(monsterObject);
                }
                pair.Value.curCnt += spawnCnt;
            }
        }
    }

    [ServerCallback]

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
