using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseInstanceScene : BaseScene, IInstanceScene
{
    public override Define.Scene SceneType { get => base.SceneType; protected set => sceneType = value; }

    [SerializeField]
    protected Define.InstanceScene instanceSceneType;
    public Define.InstanceScene InstanceSceneType { get { return instanceSceneType; } set { instanceSceneType = value; } }

    [SerializeField]
    public MonsterSpawner[] monsterSpawners;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.InstanceScene;
    }

    public override void RemovePlayer(uint id)
    {
        base.RemovePlayer(id);

        // ������ ȣ��Ʈ�� ȣ��
        S_SceneObjectRemove();
    }

    [ServerCallback]
    public virtual void S_SceneObjectRemove()
    {
        if (networkPlayerObjects.Count == 0)
        {
            // �� ��ųʸ����� ����
            Managers.Scene.RemoveBaseScene(gameObject.scene);

            // �� ������ ��� ����
            foreach (MonsterSpawner monsterSpawner in monsterSpawners)
            {
                monsterSpawner.StopAllCoroutines();
            }

            // �� ����
            foreach (GameObject monster in networkMonsterObjects.Values)
            {
                NetworkServer.UnSpawn(monster);
            }

            // ��Ÿ ������Ʈ ����
            foreach (GameObject item in networkInAnimateObjects.Values)
            {
                NetworkServer.UnSpawn(item);
            }

            Invoke("DelayUnloadScene", 3.0f);
        }
    }

    [ServerCallback]
    void DelayUnloadScene()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
        Resources.UnloadUnusedAssets();
    }

}
