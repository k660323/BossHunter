using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerJoin : UI_Popup
{
    enum InputFields
    {
        IPInputField,
        PortInputField
    }

    enum Buttons
    {
        CloseButton,
        ConnectButton
    }
    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            ClosePopupUI();
        });

        Get<Button>((int)Buttons.ConnectButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            // 네트워크 IP 설정
            Managers.Instance.networkAddress = Get<InputField>((int)InputFields.IPInputField).text;
            // 네트워크 Port 설정
            if(Managers.Instance.transport is PortTransport portTransport)
            {
                string portStr = Get<InputField>((int)InputFields.PortInputField).text;
                if(ushort.TryParse(portStr, out var port))
                {
                    portTransport.Port = port;
                }
            }
            // 해당 서버에 클라이언트로 접속
            StartCoroutine(JoinWorld());
        });
    }

    IEnumerator JoinWorld()
    {
        yield return Managers.Instance.FadeInOut.FadeIn();

        Managers.Instance.StartClient();
    }
}
