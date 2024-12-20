using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMainCamera : MonoBehaviour
{
    public GameObject uiGroup;

    public float disableDisSqr;

    // 컴포넌트 배치시 자동 비활성화
    private void OnValidate()
    {
        this.enabled = false;
    }

    void Awake()
    {
#if UNITY_SERVER
        gameObject.SetActive(false);
#else
        gameObject.GetComponentInParent<Canvas>().worldCamera = Camera.main;
#endif
    }

    public void Init(GameObject _uiGroup, float _disableDisSqr)
    {
        uiGroup = _uiGroup;
        disableDisSqr = _disableDisSqr;
    }

    [ClientCallback]
    void FixedUpdate()
    {
        if(Camera.main != null && Vector3.SqrMagnitude(Camera.main.transform.position - gameObject.transform.position) <= disableDisSqr)
        {
            uiGroup.SetActive(true);
            transform.forward = Camera.main.transform.forward;
        }
        else
        {
            uiGroup.SetActive(false);
        }
    }
}
