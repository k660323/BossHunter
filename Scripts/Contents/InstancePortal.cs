using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InstancePortal : NetworkBehaviour
{
    [SerializeField]
    Define.InstanceBackground dungeonBackground;
    [SerializeField]
    List<Define.InstanceScene> instanceSceneList = new List<Define.InstanceScene>();

    [ClientCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (player.isOwned == false)
                return;

            if (player.GetParty.IsParty && player.GetParty.IsMaster == false)
                return;


            UI_Dungeon uI_Dungeon = Managers.UI.ShowPopupUI<UI_Dungeon>();
            uI_Dungeon.InitInstanceSceneList(this, ref instanceSceneList, dungeonBackground);
        }
    }

    [Command(requiresAuthority = false)]
    public void MoveToInstanceScene(GameObject masterObject, Define.InstanceScene scene)
    {
        if (masterObject.TryGetComponent(out Player masterPlayer) == false)
            return;

        List<GameObject> players = masterPlayer.GetParty.GetPlayerList();
     
        // 씬 정보가져와 던전을 로드시킨다.
        if (Managers.Data.InstanceSceneInfoDict.TryGetValue((int)scene, out Data.SceneInfo sceneInfo))
        {
            Vector3 startPos = sceneInfo._startPosition;
            StartCoroutine(Managers.Scene.DungeonSceneLoadAdditive(players, startPos, masterObject.scene.name, scene.ToString()));
        }
    }
}
