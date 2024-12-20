using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    #region UI 이벤트 할당 / 삭제 / 덮어쓰기 / 모두 삭제
    public static void BindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.AddUIEvent(go, action, type);
    }

    public static void RemoveBindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.RemoveUIEvent(go, action, type);
    }

    public static void CoverBindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.CoverUIEvent(go, action, type);
    }

    public static void RemoveAllEvent(this GameObject go, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.RemoveAllUIEvent(go, type);
    }
    #endregion

    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }

    public static bool IsValid(this BaseController bc)
    {
        return bc != null && bc.isActiveAndEnabled;
    }

}
