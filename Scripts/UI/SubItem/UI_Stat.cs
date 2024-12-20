using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
    enum Buttons
    {
        CloseButton,
    }

    enum Texts
    {
        CreatureName,
        LevelText,
        HpText,
        HpRecoveryText,
        MpText,
        MpRecoveryText,
        AtkText,
        AtkSpeedText,
        DefenseText,
        MoveSpeedText,
        HitStatePrioirtyText,
        AtkDistanceText
    }


    [HideInInspector]
    public Stat stat;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        // 창닫기 버튼 기능 바인드
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });


    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void PlayerUIStatInit(Stat _stat)
    {
        if (_stat == null)
            return;

        stat = _stat;

        stat.OnLevelAction -= UpdateLevelText;
        stat.OnLevelAction += UpdateLevelText;
        UpdateLevelText(stat.Level);

        stat.OnNameAction -= UpdateNameText;
        stat.OnNameAction += UpdateNameText;
        UpdateNameText(stat.CreatureName);

        stat.OnHpAction -= UpdateHpText;
        stat.OnHpAction += UpdateHpText;
        UpdateHpText(stat.Hp, stat.TotalMaxHp);

        stat.OnHpRecoveryAction -= UpdateHpRecoveryAmountText;
        stat.OnHpRecoveryAction += UpdateHpRecoveryAmountText;
        UpdateHpRecoveryAmountText(stat.TotalHpRecoveryAmount);

        stat.OnMpAction -= UpdateMpText;
        stat.OnMpAction += UpdateMpText;
        UpdateMpText(stat.Mp, stat.TotalMaxMp);

        stat.OnMpRecoveryAction -= UpdateMpRecoveryAmountText;
        stat.OnMpRecoveryAction += UpdateMpRecoveryAmountText;
        UpdateMpRecoveryAmountText(stat.TotalMpRecoveryAmount);

        stat.OnAttackAction -= UpdateAtkText;
        stat.OnAttackAction += UpdateAtkText;
        UpdateAtkText(stat.TotalAttack);

        stat.OnAttackSpeedAction -= UpdateAtkSpeedText;
        stat.OnAttackSpeedAction += UpdateAtkSpeedText;
        UpdateAtkSpeedText(stat.TotalAtkSpeed);

        stat.OnDefenseAction -= UpdateDefenseText;
        stat.OnDefenseAction += UpdateDefenseText;
        UpdateDefenseText(stat.TotalDefense);

        stat.OnMoveSpeedAction -= UpdateMoveSpeedText;
        stat.OnMoveSpeedAction += UpdateMoveSpeedText;
        UpdateMoveSpeedText(stat.TotalMoveSpeed);

        stat.OnHSPriorityAction -= UpdateHitStatePriorityText;
        stat.OnHSPriorityAction += UpdateHitStatePriorityText;
        UpdateHitStatePriorityText(stat.TotalHitStatePriority);

        stat.OnAtkDistanceAction -= UpdateAtkDistanceText;
        stat.OnAtkDistanceAction += UpdateAtkDistanceText;
        UpdateAtkDistanceText(stat.TotalAtkDistance);
    }

    public void UpdateLevelText(int level)
    {
        Get<Text>((int)Texts.LevelText).text = level.ToString();
    }

    public void UpdateNameText(string name)
    {
        Get<Text>((int)Texts.CreatureName).text = $"타입 : {name}";
    }

    public void UpdateHpText(int curHp, int maxHp)
    {
        Get<Text>((int)Texts.HpText).text = $"{curHp} / {maxHp}";
    }

    public void UpdateHpRecoveryAmountText(int recoveryHp)
    {
        Get<Text>((int)Texts.HpRecoveryText).text = recoveryHp.ToString();
    }

    public void UpdateMpText(int curMp, int maxMp)
    {
        Get<Text>((int)Texts.MpText).text = $"{curMp} / {maxMp}";
    }

    public void UpdateMpRecoveryAmountText(int recoveryMp)
    {
        Get<Text>((int)Texts.MpRecoveryText).text = recoveryMp.ToString();
    }

    public void UpdateAtkText(int atk)
    {
        Get<Text>((int)Texts.AtkText).text = atk.ToString();
    }

    public void UpdateAtkSpeedText(float atkSpeed)
    {
        Get<Text>((int)Texts.AtkSpeedText).text = atkSpeed.ToString();
    }

    public void UpdateDefenseText(int defense)
    {
        Get<Text>((int)Texts.DefenseText).text = defense.ToString();
    }

    public void UpdateMoveSpeedText(float moveSpeed)
    {
        Get<Text>((int)Texts.MoveSpeedText).text = moveSpeed.ToString();
    }

    public void UpdateHitStatePriorityText(int hitPriority)
    {
        Get<Text>((int)Texts.HitStatePrioirtyText).text = hitPriority.ToString();
    }

    public void UpdateAtkDistanceText(float attackDistance)
    {
        Get<Text>((int)Texts.AtkDistanceText).text = attackDistance.ToString();
    }

    private void OnDestroy()
    {
        stat.OnLevelAction -= UpdateLevelText;

        stat.OnNameAction -= UpdateNameText;

        stat.OnHpAction -= UpdateHpText;

        stat.OnHpRecoveryAction -= UpdateHpRecoveryAmountText;

        stat.OnMpAction -= UpdateMpText;

        stat.OnMpRecoveryAction -= UpdateMpRecoveryAmountText;

        stat.OnAttackAction -= UpdateAtkText;

        stat.OnAttackSpeedAction -= UpdateAtkSpeedText;

        stat.OnDefenseAction -= UpdateDefenseText;

        stat.OnMoveSpeedAction -= UpdateMoveSpeedText;

        stat.OnHSPriorityAction -= UpdateHitStatePriorityText;

        stat.OnAtkDistanceAction -= UpdateAtkDistanceText;
    }
}
