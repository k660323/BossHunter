using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Respawn : UI_Popup
{
    enum Images
    {
        Background,
    }

    enum Texts
    {
        RespawnText,
    }

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        
    }

    public void SpawnCorStart(float time)
    {
        StartCoroutine(RespawnCor(time));
    }

    IEnumerator RespawnCor(float time)
    {
        while(time > 0)
        {
            time -= Time.deltaTime;
            Get<Text>((int)Texts.RespawnText).text = $"{time.ToString("F1")}초 후 시작 지점에 스폰됩니다...";
            yield return null;
        }

        Managers.UI.ClosePopupUI();
    }
}
