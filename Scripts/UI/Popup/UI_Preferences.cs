using UnityEngine;
using UnityEngine.UI;

public class UI_Preferences : UI_Popup
{
    enum GameObjects
    {
        Graphic,
        Sound,
        GamePlay
    }

    enum Toggles
    {
        GraphicToggle,
        SoundToggle,
        GamePlayToggle
    }

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));
        Bind<Toggle>(typeof(Toggles));

        Get<GameObject>((int)GameObjects.Graphic).SetActive(true);
        Get<GameObject>((int)GameObjects.Sound).SetActive(false);
        Get<GameObject>((int)GameObjects.GamePlay).SetActive(false);

        Get<Toggle>((int)Toggles.GraphicToggle).onValueChanged.AddListener(value => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Get<GameObject>((int)GameObjects.Graphic).SetActive(value);
        });

        Get<Toggle>((int)Toggles.SoundToggle).onValueChanged.AddListener(value => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Get<GameObject>((int)GameObjects.Sound).SetActive(value);
        });

        Get<Toggle>((int)Toggles.GamePlayToggle).onValueChanged.AddListener(value => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Get<GameObject>((int)GameObjects.GamePlay).SetActive(value);
        });
    }
}
