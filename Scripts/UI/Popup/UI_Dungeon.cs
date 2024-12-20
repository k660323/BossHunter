using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dungeon : UI_Popup
{
    enum Images
    {
        Background
    }

    enum Buttons
    {
        EnterButton,
        ExitButton
    }

    enum GameObjects
    {
        Content
    }

    InstancePortal _portal;

    // 생성될 아이템 슬롯 이름
    [Header("필수 기입"), SerializeField]
    private string _slotPrefabName;

    // 목표 씬
    public Define.InstanceScene targetScene = Define.InstanceScene.Unknown;

    // 선택 된 ui
    UI_DungeonInfoItem _selectedItem;

    public UI_DungeonInfoItem SelectedItem 
    { 
        get 
        { 
            return _selectedItem; 
        } 
        set 
        {
            // 같은 객체면 스킵
            if (_selectedItem == value)
                return;

            // 이전에 선택된 아이콘 제거
            if (_selectedItem != null)
                _selectedItem.Select(false);

            _selectedItem = value;

            // 현재 선택된 아이콘 활성화
            if(value != null)
                _selectedItem.Select(true);
        } 
    }

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        // 던전 입장
        Get<Button>((int)Buttons.EnterButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            if (targetScene == Define.InstanceScene.Unknown)
                return;
            if (NetworkClient.localPlayer == null)
                return;

            _portal.MoveToInstanceScene(NetworkClient.localPlayer.gameObject, targetScene);
            Managers.UI.ClosePopupUI();
        });

        // 창닫기
        Get<Button>((int)Buttons.ExitButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.Input.IsCursor = false;
            Managers.UI.ClosePopupUI();
        });

        Managers.Input.IsCursor = true;

        Managers.Sound.Play2D("FX/MapOpen", Define.Sound2D.Effect2D);
    }

    public void InitInstanceSceneList(InstancePortal portal, ref List<Define.InstanceScene> sceneList, Define.InstanceBackground background)
    {
        _portal = portal;
        Get<Image>((int)Images.Background).sprite = Managers.Resource.Load<Sprite>(Enum.GetName(typeof(Define.InstanceBackground), background));
        for (int i = 0; i < sceneList.Count; i++)
        {
            int eScene = (int)sceneList[i];
            if (Managers.Data.InstanceSceneInfoDict.ContainsKey(eScene) == false)
                continue;
            Data.SceneInfo sInfo = Managers.Data.InstanceSceneInfoDict[eScene];
            RectTransform dSlotRt = CloneSlot();

            if(dSlotRt.TryGetComponent(out UI_DungeonInfoItem dInfoItem) == false)
                return;

            dInfoItem.SetDungeonInfo(this, (Define.InstanceScene)sInfo._scene, sInfo._sceneImage, sInfo._nickName, sInfo._recommendLevel, sInfo._comment);

            if (i == 0)
            {
                targetScene = dInfoItem.destinationScene;
                SelectedItem = dInfoItem;
            }
        }

        RectTransform CloneSlot()
        {
            GameObject slotGo = Managers.Resource.Instantiate(_slotPrefabName, null);
            slotGo.BindEvent(data =>
            {
                Managers.Sound.Play2D("FX/DungeonClick", Define.Sound2D.Effect2D);
            });
            if (slotGo.TryGetComponent(out RectTransform tr) == false)
            {
                Destroy(slotGo);
                return null;
            }

            if (Get<GameObject>((int)GameObjects.Content).transform is RectTransform _contentAreaRt)
            {
                tr.SetParent(_contentAreaRt, false);
            }

            return tr;
        }
    }
}
