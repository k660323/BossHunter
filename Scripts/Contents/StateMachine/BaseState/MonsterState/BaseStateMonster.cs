using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateMonster : BaseState
{
    private Monster monster;
    public Monster MonsterGS { get { return monster; } protected set { monster = value; } }

    private MonsterController monsterController;

    public MonsterController MonsterControllerGS { get { return monsterController; } protected set { monsterController = value; } }

    protected BaseStateMonster(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
        monster = _monster;
        monsterController = _monsterController;
    }
}
