using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Chat : UI_Base
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public InputField inputField;

    // ä�� Text���� ��� ť
    Queue<Text> chatQueue = new Queue<Text>();

    enum GameObjects
    {
        Content
    }

    public override void Init()
    {
        transform.Find("Scroll View").TryGetComponent(out scrollRect);
        transform.Find("Scrollbar Vertical").TryGetComponent(out scrollbar);
        transform.Find("ChatInputField").TryGetComponent(out inputField);

        Bind<GameObject>(typeof(GameObjects));
        GameObject content = Get<GameObject>((int)GameObjects.Content);
        int childCount = content.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (content.transform.GetChild(i).TryGetComponent(out Text text))
                chatQueue.Enqueue(text);
        }

        // inputField�� ��Ŀ���� �Ǿ� �ְ� EnterŰ�� ������ ȣ��ȴ� �� �޽����� ������.
        inputField.onSubmit.AddListener(SendChat);
        // �Է��ʵ带 ���콺�� Ŭ�������� �÷��̾� ��ǲ ������Ʈ�� ��Ȱ��ȭ ��Ų��.
        inputField.gameObject.BindEvent(gameObject => { PlayerController.SetInputField(false); });
        // �Է��ʵ忡�� ��Ŀ���� ������ ��� �ٽ� �÷��̾� ��ǲ ������Ʈ�� Ȱ��ȭ ��Ų��.
        inputField.onEndEdit.AddListener((string value) => { PlayerController.SetInputField(true); });
    }

    private void Start()
    {
        scrollbar.value = 0.0f;    
    }

    // ä�� ���� UI�� ������Ʈ
    public void UpdateChat(string message)
    {
        Text text = chatQueue.Dequeue();
        text.text = message;
        text.transform.SetAsLastSibling();
        chatQueue.Enqueue(text);
    }

    // �޽��� ������
    public void SendChat(string message)
    {
        if (NetworkClient.localPlayer.TryGetComponent(out NetworkClientInfo info))
        {
            string str = $"{info.NickName} : {message}";
            BaseScene bs = Managers.Scene.GetBaseScene(SceneManager.GetActiveScene());
            if(bs != null)
            {
                BaseSceneNetwork bn = bs.GetSceneNetwork<BaseSceneNetwork>();
                if (bn is IChatable)
                {
                    IChatable chatable = bn as IChatable;
                    chatable.CTS_ChatRPC(str);

                }
            }
        }
        inputField.text = "";
    }
}
