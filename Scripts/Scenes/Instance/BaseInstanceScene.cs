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

        // 서버나 호스트만 호출
        S_SceneObjectRemove();
    }

    [ServerCallback]
    public virtual void S_SceneObjectRemove()
    {
        if (networkPlayerObjects.Count == 0)
        {
            // 씬 딕셔너리에서 제거
            Managers.Scene.RemoveBaseScene(gameObject.scene);

            // 몹 스포너 기능 중지
            foreach (MonsterSpawner monsterSpawner in monsterSpawners)
            {
                monsterSpawner.StopAllCoroutines();
            }

            // 몹 수거
            foreach (GameObject monster in networkMonsterObjects.Values)
            {
                NetworkServer.UnSpawn(monster);
            }

            // 기타 오브젝트 수거
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
