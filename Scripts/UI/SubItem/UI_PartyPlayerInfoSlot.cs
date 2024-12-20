using UnityEngine;
using UnityEngine.UI;

public class UI_PartyPlayerInfoSlot : UI_Base
{
    enum Texts
    {
        IDText,
        NickNameText,
    }

    enum GameObjects
    {
        MasterImage
    }

    enum Buttons
    {
        ResignButton
    }

    UI_Party uI_party;

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        // ������ �̹��� �⺻ ��Ȱ��ȭ
        Get<GameObject>((int)GameObjects.MasterImage).SetActive(false);

        // ���� ��ư
        Get<Button>((int)Buttons.ResignButton).gameObject.BindEvent(data =>
        {
            if (uI_party == null)
                return;

            uint id = uint.Parse(Get<Text>((int)Texts.IDText).text);

            uI_party.ResignPlayerInfoSlot(id);
        });
    }

    public void SetPlayerInfo(UI_Party ui_party, long id, string nickName, bool isMaster)
    {
        uI_party = ui_party;
        Get<Text>((int)Texts.IDText).text = id.ToString();
        Get<Text>((int)Texts.NickNameText).text = nickName.ToString();
        Get<GameObject>((int)GameObjects.MasterImage).SetActive(isMaster);
    }
}
