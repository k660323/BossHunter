using Mirror.Examples.AdditiveLevels;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldItem : UI_Base
{
    // ī�޶� �������� �ٶ󺸴� ������Ʈ
    protected LookAtMainCamera _lookAtMainCamera;
    public LookAtMainCamera GetlookAtMainCamera { get { return _lookAtMainCamera; } }

    enum GameObjects
    {
        UIGroup
    }

    enum Images
    {
        ItemImage
    }

    public override void Init()
    {
        TryGetComponent(out _lookAtMainCamera);
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        // 25ĭ���� ������ ����
        _lookAtMainCamera.Init(Get<GameObject>((int)GameObjects.UIGroup), 625.0f);
    }

    public void UpdateImage(Sprite sprite)
    {
        Get<Image>((int)Images.ItemImage).sprite = sprite;
    }
}
