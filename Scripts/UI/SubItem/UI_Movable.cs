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
        // target�� ���ٸ� �θ��� transform�� ����
        if (target == null)
            target = transform.parent;
    }

    // ���콺 Ŭ��
    public void OnPointerDown(PointerEventData eventData)
    {
        // ����� ���� ��ġ
        startPos = target.position;
        // �巡�� ������ġ
        dragStartPos = eventData.position;
    }

    // �巡��
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
