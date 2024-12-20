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

        // 기본 정보 초기화
        lastindex = Define.PlayerType.Warrior;
        currentIndex = Define.PlayerType.Warrior;
        characterInfoLen = Managers.Data.PlayerCharacterInfoDict.Count;

        // 버튼 이벤트 바인딩
        Get<Button>((int)Buttons.LeftButton).gameObject.BindEvent(data=> {
            Managers.Sound.Play2D("FX/NextPage", Define.Sound2D.Effect2D);
            UpdateArrowButton((int)currentIndex - 1);
        });

        Get<Button>((int)Buttons.RightButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/NextPage", Define.Sound2D.Effect2D);
            UpdateArrowButton((int)currentIndex + 1);
        });

        // Raw이미지 게임 오브젝트 전부 비활성화
        string[] playerTypes = Enum.GetNames(typeof(Define.PlayerType));
        for (int i=0; i < playerTypes.Length; i++)
        {
            Get<GameObject>(i).SetActive(false);
        }

        // 버튼 UI 초기화
        UpdateArrowButton(0);

        // 월드 입장
        Get<Button>((int)Buttons.WorldJoinButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            string nickName = Get<InputField>((int)InputFields.NickNameInputField).text;
            if (nickName.Length <= 0)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("닉네임을 작성해주세요.");
                return;
            }
            else if (nickName.Length > 8)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("닉네임을 8글자를 초과할수 없습니다.");
                return;
            }

            Get<Button>((int)Buttons.WorldJoinButton).interactable = false;

            // 캐릭터 생성 및 맵 이동 (메시지 전송)
            StartSpawnPlayer message = new StartSpawnPlayer
            {
                nickName = nickName,
                playerType = currentIndex // 추후 변경
            };

            NetworkClient.Send(message);
        });

        // 월드 퇴장
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
        // 범위 벗어나면 리턴해버림
        if (index < 0 || index >= characterInfoLen)
            return;

        // 일단 상호작용이 되도록 활성화 한다.
        Get<Button>((int)Buttons.LeftButton).interactable = true;
        Get<Button>((int)Buttons.RightButton).interactable = true;

        // 인덱스에 따른 버튼 활성화 비활성화 처리
        if (index == 0)
        {
            Get<Button>((int)Buttons.LeftButton).interactable = false;
        }

        if (index == characterInfoLen - 1)
        {
            Get<Button>((int)Buttons.RightButton).interactable = false;
        }

        // 현재 캐릭터 타입
        lastindex = currentIndex;
        currentIndex = (Define.PlayerType)index;

        // 현재 캐릭터 정보 업데이트
        ShowPlayerInfoData(ref currentIndex);
    }

    void ShowPlayerInfoData(ref Define.PlayerType playerInfoType)
    {
        var playerCharacterInfo = Managers.Data.PlayerCharacterInfoDict[playerInfoType];
        // 닉네임 업데이트
        Get<Text>((int)Texts.CharacterTypeText).text = playerCharacterInfo._name;

        // RawImages 업데이트
        VisualCharacterSet(ref lastindex, ref currentIndex);

        // 플레이어 주 무기 텍스트
        Get<Text>((int)Texts.MainWeaponText).text = playerCharacterInfo._mainWeapon;

        // 캐릭터 타입별 성능 표시
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

        // 추가 설명 텍스트
        Get<Text>((int)Texts.CharacterCommentText).text = playerCharacterInfo._comment;
    }

    void VisualStarSet(ref GameObject go, ref int cnt)
    {
        // 해당 성능에 따라 별을 표시
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
        // 이전 캐릭터는 활성화 하지 않는다.
        Get<GameObject>((int)_prev).SetActive(false);
        // 현재 캐릭터는 활성화 한다.
        Get<GameObject>((int)_cur).SetActive(true);
    }
}
