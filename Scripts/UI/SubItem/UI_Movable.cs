using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Movable : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField]
    Transform target;

    Vector2 startPos;
    Vector2 dragStartPos;
    void Awake()
    {
        // target이 없다면 부모의 transform을 설정
        if (target == null)
            target = transform.parent;
    }

    // 마우스 클릭
    public void OnPointerDown(PointerEventData eventData)
    {
        // 대상이 시작 위치
        startPos = target.position;
        // 드래그 시작위치
        dragStartPos = eventData.position;
    }

    // 드래그
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 nextPos = eventData.position;
        if (eventData.position.y < 0)
        {
            nextPos.y = 0;
        }
        else if (eventData.position.y > Screen.height)
        {
            nextPos.y = Screen.height;
        }

        if (eventData.position.x < 0)
        {
            nextPos.x = 0;
        }
        else if(eventData.position.x > Screen.width)
        {
            nextPos.x = Screen.width;
        }

        Vector2 gap = nextPos - dragStartPos;
        target.position = startPos + gap;
    }
}
