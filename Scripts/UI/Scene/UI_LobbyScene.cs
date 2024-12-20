using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Scene
{
    enum Buttons
    {
        LeftButton,
        RightButton,
        WorldJoinButton,
        WorldExitButton
    }

    enum Texts
    {
        CharacterTypeText,
        MainWeaponText,
        CharacterCommentText
    }

    enum InputFields
    {
        NickNameInputField
    }

    enum RawImages
    {
        CharacterRawImage
    }

    enum GameObjects
    {
        Warrior,
        Archer,
        Wizard,
        CharacterSelectBackground,
        DifficultyGroup,
        OffensePowerGroup,
        DefenseGroup,
        MobilityGroup
    }


    Define.PlayerType lastindex;
    Define.PlayerType currentIndex;
    int characterInfoLen;

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<InputField>(typeof(InputFields));
        Bind<RawImage>(typeof(RawImages));
        Bind<GameObject>(typeof(GameObjects));

        // �⺻ ���� �ʱ�ȭ
        lastindex = Define.PlayerType.Warrior;
        currentIndex = Define.PlayerType.Warrior;
        characterInfoLen = Managers.Data.PlayerCharacterInfoDict.Count;

        // ��ư �̺�Ʈ ���ε�
        Get<Button>((int)Buttons.LeftButton).gameObject.BindEvent(data=> {
            Managers.Sound.Play2D("FX/NextPage", Define.Sound2D.Effect2D);
            UpdateArrowButton((int)currentIndex - 1);
        });

        Get<Button>((int)Buttons.RightButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/NextPage", Define.Sound2D.Effect2D);
            UpdateArrowButton((int)currentIndex + 1);
        });

        // Raw�̹��� ���� ������Ʈ ���� ��Ȱ��ȭ
        string[] playerTypes = Enum.GetNames(typeof(Define.PlayerType));
        for (int i=0; i < playerTypes.Length; i++)
        {
            Get<GameObject>(i).SetActive(false);
        }

        // ��ư UI �ʱ�ȭ
        UpdateArrowButton(0);

        // ���� ����
        Get<Button>((int)Buttons.WorldJoinButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            string nickName = Get<InputField>((int)InputFields.NickNameInputField).text;
            if (nickName.Length <= 0)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("�г����� �ۼ����ּ���.");
                return;
            }
            else if (nickName.Length > 8)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("�г����� 8���ڸ� �ʰ��Ҽ� �����ϴ�.");
                return;
            }

            Get<Button>((int)Buttons.WorldJoinButton).interactable = false;

            // ĳ���� ���� �� �� �̵� (�޽��� ����)
            StartSpawnPlayer message = new StartSpawnPlayer
            {
                nickName = nickName,
                playerType = currentIndex // ���� ����
            };

            NetworkClient.Send(message);
        });

        // ���� ����
        Get<Button>((int)Buttons.WorldExitButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);

            if (Managers.Instance.mode == Mirror.NetworkManagerMode.ClientOnly)
                Managers.Instance.StopClient();
            else if(Managers.Instance.mode == Mirror.NetworkManagerMode.Host)
                Managers.Instance.StopHost();
        });
    }

    public void UpdateArrowButton(int index)
    {
        // ���� ����� �����ع���
        if (index < 0 || index >= characterInfoLen)
            return;

        // �ϴ� ��ȣ�ۿ��� �ǵ��� Ȱ��ȭ �Ѵ�.
        Get<Button>((int)Buttons.LeftButton).interactable = true;
        Get<Button>((int)Buttons.RightButton).interactable = true;

        // �ε����� ���� ��ư Ȱ��ȭ ��Ȱ��ȭ ó��
        if (index == 0)
        {
            Get<Button>((int)Buttons.LeftButton).interactable = false;
        }

        if (index == characterInfoLen - 1)
        {
            Get<Button>((int)Buttons.RightButton).interactable = false;
        }

        // ���� ĳ���� Ÿ��
        lastindex = currentIndex;
        currentIndex = (Define.PlayerType)index;

        // ���� ĳ���� ���� ������Ʈ
        ShowPlayerInfoData(ref currentIndex);
    }

    void ShowPlayerInfoData(ref Define.PlayerType playerInfoType)
    {
        var playerCharacterInfo = Managers.Data.PlayerCharacterInfoDict[playerInfoType];
        // �г��� ������Ʈ
        Get<Text>((int)Texts.CharacterTypeText).text = playerCharacterInfo._name;

        // RawImages ������Ʈ
        VisualCharacterSet(ref lastindex, ref currentIndex);

        // �÷��̾� �� ���� �ؽ�Ʈ
        Get<Text>((int)Texts.MainWeaponText).text = playerCharacterInfo._mainWeapon;

        // ĳ���� Ÿ�Ժ� ���� ǥ��
        GameObject dGroup = Get<GameObject>((int)GameObjects.DifficultyGroup);
        int difficulty = playerCharacterInfo._difficulty;
        VisualStarSet(ref dGroup, ref difficulty);

        GameObject opGroup = Get<GameObject>((int)GameObjects.OffensePowerGroup);
        int offensePower = playerCharacterInfo._offensePower;
        VisualStarSet(ref opGroup, ref offensePower);

        GameObject dfGroup = Get<GameObject>((int)GameObjects.DefenseGroup);
        int defence = playerCharacterInfo._defense;
        VisualStarSet(ref dfGroup, ref defence);

        GameObject moGroup = Get<GameObject>((int)GameObjects.MobilityGroup);
        int mobility = playerCharacterInfo._mobility;
        VisualStarSet(ref moGroup, ref mobility);

        // �߰� ���� �ؽ�Ʈ
        Get<Text>((int)Texts.CharacterCommentText).text = playerCharacterInfo._comment;
    }

    void VisualStarSet(ref GameObject go, ref int cnt)
    {
        // �ش� ���ɿ� ���� ���� ǥ��
        int childCnt = go.transform.childCount;

        for (int i = 0; i < cnt; i++)
        {
            go.transform.GetChild(i).gameObject.SetActive(true);
        }

        for (int i = cnt; i < childCnt; i++)
        {
            go.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void VisualCharacterSet(ref Define.PlayerType _prev, ref Define.PlayerType _cur)
    {
        // ���� ĳ���ʹ� Ȱ��ȭ ���� �ʴ´�.
        Get<GameObject>((int)_prev).SetActive(false);
        // ���� ĳ���ʹ� Ȱ��ȭ �Ѵ�.
        Get<GameObject>((int)_cur).SetActive(true);
    }
}
