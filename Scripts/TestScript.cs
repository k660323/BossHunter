using Mirror;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void HpPortionCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Countable, Define.ItemSubType.Consumption, 0);
    }

    public void MpPortionCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Countable, Define.ItemSubType.Consumption, 1);
    }

    public void HelemtCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Helmet, 0);
    }

    public void ShoulderCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Shoulder, 0);
    }

    public void TopCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Top, 0);
    }

    public void PantsCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Pants, 0);
    }

    public void BeltCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Belt, 0);
    }

    public void ShoesCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Shoes, 0);
    }

    public void RingCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Ring, 0);
    }

    public void NecklaceCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Necklace, 0);
    }

    public void BraceletCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.Bracelet, 0);
    }

    public void WeaponCreateItem()
    {
        Player player = Player.Instance;
        if (player == null)
            return;
        BaseScene baseScene = Managers.Scene.GetBaseScene(player.gameObject.scene);
        baseScene.GetSceneNetwork<BaseSceneNetwork>().CTS_TestCreateItemRPC(NetworkClient.localPlayer.gameObject, Define.ItemType.Equip, Define.ItemSubType.MeleeWeapon, 0);
    }

}
