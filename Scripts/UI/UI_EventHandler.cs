using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityAction<PointerEventData> OnClickHandler = null;
    public UnityAction<PointerEventData> OnEnterHandler = null;
    public UnityAction<PointerEventData> OnDownHandler = null;
    public UnityAction<PointerEventData> OnDragHandler = null;
    public UnityAction<PointerEventData> OnUpHandler = null;
    public UnityAction<PointerEventData> OnExitHandler = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnterHandler?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDownHandler?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragHandler?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnUpHandler?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExitHandler?.Invoke(eventData);
    }
}