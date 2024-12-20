public class UI_PlayerUtil : UI_Scene, IChatable_UI, IPlayerUI
{
    UI_Chat chat;

    UI_PlayerUI playerInfo;

    public UI_Chat Chat
    {
        get { return chat; }
        set { chat = value; }
    }

    UI_PlayerUI IPlayerUI.GetPlayerUI { get { return playerInfo; } }

    enum Buttons
    {
        OptionButton,
        InventoryButton,
        EquipmentButton,
        StatButton
    }
    
    public override void Init()
    {
        base.Init();
        Chat = GetComponentInChildren<UI_Chat>();
        playerInfo = GetComponentInChildren<UI_PlayerUI>();
    }
}
