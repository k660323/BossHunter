using Mirror;
using UnityEngine;

public class WorldItem : InAnimate
{
    [SyncVar(hook = nameof(UpdateItemImage))]
    Item _item;

    [SerializeField]
    UI_WorldItem uI_WorldItem;
    public UI_WorldItem UI_WorldItem { get { return uI_WorldItem; } }

    // 클라만 Image 컴포넌트 캐싱
    [ClientCallback]
    public void Awake()
    {
        uI_WorldItem = GetComponentInChildren<UI_WorldItem>();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();

        // 5분뒤 삭제
        Invoke("DestroyItem", 300.0f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        uI_WorldItem.GetlookAtMainCamera.enabled = true;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        uI_WorldItem.GetlookAtMainCamera.enabled = false;
    }

    // 서버에서만 아이템 설정
    [ServerCallback]
    public void SetItem(Item item)
    {
        _item = item;
    }

    [SerializeField]
    public Item GetItem()
    {
        return _item;
    }


    [ClientCallback]
    void UpdateItemImage(Item _old, Item _new)
    {
        // 새로운 이미지가 없으면 비워준다.
        if(_new == null)
        {
            uI_WorldItem.UpdateImage(null);
            return;
        }

        // 이전 이미지가 있고, 만약 같은 이미지면 패스
        if (_old != null && _old.Data._iconSpritePath == _new.Data._iconSpritePath)
            return;

        Sprite loadImage = Managers.Resource.Load<Sprite>(_new.Data._iconSpritePath);
        uI_WorldItem.UpdateImage(loadImage);
    }

    public void DestroyItem()
    {
        CancelInvoke();
        _item = null;
        NetworkServer.UnSpawn(gameObject);
    }

}
