using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine : StateMachine
{
    [ServerCallback]

    private void Awake()
    {
       
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        BaseState?.FixedUpdateState();
    }

    [ServerCallback]
    private void Update()
    {
        BaseState?.UpdateState();
    }

    [ServerCallback]
    public Define.State RandomSelectSkill(List<Define.State> list)
    {
        if (list == null || list.Count == 0)
            return Define.State.None;

        int index = Random.Range(0, list.Count + 1);

        if (index >= list.Count)
            return Define.State.None;

        return list[index];
    }
}
