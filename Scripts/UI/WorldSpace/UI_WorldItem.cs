using Mirror.Examples.AdditiveLevels;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldItem : UI_Base
{
    // 카메라 방향으로 바라보는 컴포넌트
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

        // 25칸까지 빌보드 적용
        _lookAtMainCamera.Init(Get<GameObject>((int)GameObjects.UIGroup), 625.0f);
    }

    public void UpdateImage(Sprite sprite)
    {
        Get<Image>((int)Images.ItemImage).sprite = sprite;
    }
}
