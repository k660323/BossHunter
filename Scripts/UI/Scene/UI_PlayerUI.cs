using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerUI : UI_Base
{
    enum Buttons
    {
        SettingButton,
        InventoryButton,
        EquipmentButton,
        StatButton,
        PartySystemButton,
    }

    enum GameObjects
    {
        Characters,
        UI_Inventory,
        UI_EquipmentManager,
        UI_Stat,
        UI_Party,
        UI_Interaction
    }

    enum Sliders
    {
        HpSlider,
        MpSlider,
        ExpSlider
    }

    enum Texts
    {
        HpText,
        MpText,
        ExpText,
        LevelText
    }

    UI_Inventory ui_inventory;
    public UI_Inventory UI_Inventory => ui_inventory;

    UI_EquipmentManager ui_EquipmentManager;
    public UI_EquipmentManager UI_EquipmentManager => ui_EquipmentManager;

    UI_Party ui_party;

    public UI_Party UI_Party => ui_party;

    UI_Stat ui_stat;
    public UI_Stat UI_Stat => ui_stat;

    UI_Interaction ui_interaction;
    public UI_Interaction UI_Interaction => ui_interaction;

    PlayerStat stat;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Slider>(typeof(Sliders));
        Bind<Text>(typeof(Texts));


        Get<GameObject>((int)GameObjects.UI_Inventory).TryGetComponent(out ui_inventory);
        Get<GameObject>((int)GameObjects.UI_EquipmentManager).TryGetComponent(out ui_EquipmentManager);
        Get<GameObject>((int)GameObjects.UI_Stat).TryGetComponent(out ui_stat);
        Get<GameObject>((int)GameObjects.UI_Party).TryGetComponent(out ui_party);
        Get<GameObject>((int)GameObjects.UI_Interaction).TryGetComponent(out ui_interaction);

        // 세팅 창 띄우기
        Get<Button>((int)Buttons.SettingButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_Setting>();
        });

        // 인벤토리 창 띄우기
        Get<Button>((int)Buttons.InventoryButton).gameObject.BindEvent(data => {
            ShowInventory();
        });

        // 장비 창 띄우기
        Get<Button>((int)Buttons.EquipmentButton).gameObject.BindEvent(data => {
            ShowEquipment();
        });

        // 스탯 창 띄우기
        Get<Button>((int)Buttons.StatButton).gameObject.BindEvent(data => {
            ShowStat();
        });

        // 파티 시스템 창 띄우기
        Get<Button>((int)Buttons.PartySystemButton).gameObject.BindEvent(data => {
            ShowParty();
        });

        // 플레이어 이미지 초기화
        GameObject character = Get<GameObject>((int)GameObjects.Characters);
        for (int i = 0; i < character.transform.childCount; i++)
        {
            character.transform.GetChild(i).gameObject.SetActive(false);
        }

    }

    // 플레이어 UI 세팅
    public void PlayerUIInfoInit(PlayerStat _stat)
    {
        stat = _stat;
        SetPlayerImage(_stat.PlayerType);

        _stat.OnHpAction -= UpdateHpSlider;
        _stat.OnHpAction += UpdateHpSlider;
        UpdateHpSlider(_stat.Hp, _stat.TotalMaxHp);

        _stat.OnMpAction -= UpdateMpSlider;
        _stat.OnMpAction += UpdateMpSlider;
        UpdateHpSlider(_stat.Mp, _stat.TotalMaxMp);

        _stat.OnExpAction -= UpdateExpSlider;
        _stat.OnExpAction += UpdateExpSlider;
        UpdateExpSlider(_stat.Exp, _stat.MaxExp);

        _stat.OnHpAction -= UpdateHpText;
        _stat.OnHpAction += UpdateHpText;
        UpdateHpText(_stat.Hp, _stat.TotalMaxHp);

        _stat.OnMpAction -= UpdateMpText;
        _stat.OnMpAction += UpdateMpText;
        UpdateMpText(_stat.Mp, _stat.TotalMaxMp);

        _stat.OnExpAction -= UpdateExpText;
        _stat.OnExpAction += UpdateExpText;
        UpdateExpText(_stat.Exp, _stat.MaxExp);

        _stat.OnLevelAction -= UpdateLevelText;
        _stat.OnLevelAction += UpdateLevelText;
        UpdateLevelText(_stat.Level);
    }
    
    #region 플레이어 이미지

    public void SetPlayerImage(Define.PlayerType type)
    {
        // 해당 타입의 플레이어 활성화
        GameObject character = Get<GameObject>((int)GameObjects.Characters);
        character.transform.GetChild((int)type).gameObject.SetActive(true);
    }

    #endregion

    #region 체력, 마력, 경험치, 레벨 UI 업데이트
    public void UpdateHpSlider(int curHp, int maxHp)
    {
        Get<Slider>((int)Sliders.HpSlider).value = (float)curHp / maxHp;
    }

    public void UpdateMpSlider(int curMp, int maxMp)
    {
        Get<Slider>((int)Sliders.MpSlider).value = (float)curMp / maxMp;
    }

    public void UpdateExpSlider(float curExp, float maxExp)
    {
        Get<Slider>((int)Sliders.ExpSlider).value = (float)curExp / maxExp;
    }

    public void UpdateHpText(int curHp, int maxHp)
    {
        Get<Text>((int)Texts.HpText).text = $"{curHp} / {maxHp}";
    }

    public void UpdateMpText(int curMp, int maxMp)
    {
        Get<Text>((int)Texts.MpText).text = $"{curMp} / {maxMp}";
    }

    public void UpdateExpText(float curExp, float maxExp)
    {
        Get<Text>((int)Texts.ExpText).text = $"{curExp.ToString("F1")} / {maxExp.ToString("F1")}";
    }

    public void UpdateLevelText(int curLevelText)
    {
        Get<Text>((int)Texts.LevelText).text = $"Lv : {curLevelText}";
    }
    #endregion

    public void OpenPlayerInteraction(Vector2 startPos, Player targetPlayer)
    {
        ui_interaction.SetPlayerInfo(startPos, targetPlayer);
    }

    #region 유저 UI
    public void ShowInventory()
    {
        Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
        GameObject uI_Inventory = Get<GameObject>((int)GameObjects.UI_Inventory);
        uI_Inventory.SetActive(!uI_Inventory.activeSelf);
        if (uI_Inventory.activeSelf)
            uI_Inventory.transform.SetAsLastSibling();
    }

    public void ShowParty()
    {
        Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
        GameObject uI_Party = Get<GameObject>((int)GameObjects.UI_Party);
        uI_Party.SetActive(!uI_Party.activeSelf);
        if (uI_Party.activeSelf)
            uI_Party.transform.SetAsLastSibling();
    }

    public void ShowEquipment()
    {
        Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
        GameObject uI_Equipment = Get<GameObject>((int)GameObjects.UI_EquipmentManager);
        uI_Equipment.SetActive(!uI_Equipment.activeSelf);
        if (uI_Equipment.activeSelf)
            uI_Equipment.transform.SetAsLastSibling();
    }

    public void ShowStat()
    {
        Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
        GameObject uI_Stat = Get<GameObject>((int)GameObjects.UI_Stat);
        uI_Stat.SetActive(!uI_Stat.activeSelf);
        if (uI_Stat.activeSelf)
            uI_Stat.transform.SetAsLastSibling();
    }
    #endregion

    private void OnDestroy()
    {
        if (stat == null)
            return;

        stat.OnHpAction -= UpdateHpSlider;

        stat.OnMpAction -= UpdateMpSlider;

        stat.OnExpAction -= UpdateExpSlider;

        stat.OnHpAction -= UpdateHpText;

        stat.OnMpAction -= UpdateMpText;

        stat.OnExpAction -= UpdateExpText;

        stat.OnLevelAction -= UpdateLevelText;
    }
}
