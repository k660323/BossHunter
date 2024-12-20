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

    // 채팅 Text들을 담는 큐
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

        // inputField가 포커스가 되어 있고 Enter키를 누르면 호출된다 즉 메시지를 보낸다.
        inputField.onSubmit.AddListener(SendChat);
        // 입력필드를 마우스로 클릭했을때 플레이어 인풋 컴포넌트를 비활성화 시킨다.
        inputField.gameObject.BindEvent(gameObject => { PlayerController.SetInputField(false); });
        // 입력필드에서 포커스가 떨어진 경우 다시 플레이어 인풋 컴포넌트를 활성화 시킨다.
        inputField.onEndEdit.AddListener((string value) => { PlayerController.SetInputField(true); });
    }

    private void Start()
    {
        scrollbar.value = 0.0f;    
    }

    // 채팅 내역 UI에 업데이트
    public void UpdateChat(string message)
    {
        Text text = chatQueue.Dequeue();
        text.text = message;
        text.transform.SetAsLastSibling();
        chatQueue.Enqueue(text);
    }

    // 메시지 보내기
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
