using Mirror;
using UnityEngine;

public class WorldItem : InAnimate
{
    [SyncVar(hook = nameof(UpdateItemImage))]
    Item _item;

    [SerializeField]
    UI_WorldItem uI_WorldItem;
    public UI_WorldItem UI_WorldItem { get { return uI_WorldItem; } }

    // Ŭ�� Image ������Ʈ ĳ��
    [ClientCallback]
    public void Awake()
    {
        uI_WorldItem = GetComponentInChildren<UI_WorldItem>();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();

        // 5�е� ����
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

    // ���������� ������ ����
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
        // ���ο� �̹����� ������ ����ش�.
        if(_new == null)
        {
            uI_WorldItem.UpdateImage(null);
            return;
        }

        // ���� �̹����� �ְ�, ���� ���� �̹����� �н�
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
