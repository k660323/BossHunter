using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DungeonInfoItem : UI_Base
{
    enum Images
    {
        DungeonImage,
        CheckImage
    }

    enum Texts
    {
        DungeonNameText,
        RecommandLevelText,
        CommentText
    }

    public Define.InstanceScene destinationScene;

    UI_Dungeon uI_Dungeon;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        Get<Image>((int)Images.CheckImage).gameObject.SetActive(false);

        gameObject.BindEvent((data) => {
            uI_Dungeon.SelectedItem = this;
            uI_Dungeon.targetScene = destinationScene;
        });
    }

    public void SetDungeonInfo(UI_Dungeon ui_Dungeon, Define.InstanceScene scene, string dungeonSpritePath, string dungeonName, string recommandLevel, string commant)
    {
        uI_Dungeon = ui_Dungeon;
        destinationScene = scene;
        Get<Image>((int)Images.DungeonImage).sprite = Managers.Resource.Load<Sprite>(dungeonSpritePath);
        Get<Text>((int)Texts.DungeonNameText).text = dungeonName;
        Get<Text>((int)Texts.RecommandLevelText).text = recommandLevel;
        Get<Text>((int)Texts.CommentText).text = commant;
    }

    public void Select(bool isActive)
    {
        Get<Image>((int)Images.CheckImage).gameObject.SetActive(isActive);
    }
}
