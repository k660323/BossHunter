using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class StateMachine : NetworkBehaviour
{
    Define.State defaultState;

    [SerializeField, SyncVar]
    Define.State state;
    public Define.State State { get { return state; } private set { state = value; } }

    BaseState baseState;
    public BaseState BaseState { get { return baseState; } private set {  baseState = value; } }

    Dictionary<Define.State, BaseState> stateDic = new Dictionary<Define.State, BaseState>();

    protected int skillCount = 0;

    List<Define.State> skillList = new List<Define.State>();

    private void OnValidate()
    {
        ChangeState(state);
    }

    public void DefaultState(Define.State defaultState, BaseState baseState)
    {
        // �⺻ ���´� �� �ѹ��� ��������
        if (this.defaultState != Define.State.None)
            return;

        // ���� ��ųʸ� �߰�
        stateDic.Add(defaultState, baseState);
        // �⺻ ���� ĳ��
        this.defaultState = defaultState;
        // �⺻ ���� ���� �� �ְ� �ʱ�ȭ
        BaseState = baseState;
        State = defaultState;
        baseState.EnterState();
    }

    public void RegisterState(Define.State state, BaseState baseState)
    {
        // ����� ���� 
        // ���� �����ϰ� ������ ������ ��� ����
        if (stateDic.ContainsKey(state))
            return;

        int stateIndex = (int)state;
        if((int)Define.State.Skill <= stateIndex && stateIndex < (int)Define.State.MAX)
        {
            skillCount++;
        }

        stateDic.Add(state, baseState);
    }

    public void ChangeDefaultState()
    {
        // ���� �ൿ�̸� ����
        if (state == defaultState)
            return;

        // ���� �ൿ Ű �� �ִ��� Ȯ��
        if (stateDic.TryGetValue(defaultState, out BaseState _nextBaseState) == false)
            return;

        // �ൿ�� ������ ������ �Լ�
        if (baseState != null)
            baseState.ExitState(defaultState, _nextBaseState);

        baseState = _nextBaseState;
        State = defaultState;
        baseState.EnterState();
    }

    public bool ChangeState(Define.State _nextState, bool _checkCondition = true)
    {
        // ���� �ൿ Ű �� �ִ��� Ȯ��
        if (stateDic.TryGetValue(_nextState, out BaseState _nextBaseState) == false)
            return false;

        // ���� �ൿ ���� üũ
        if (_checkCondition && _nextBaseState.CheckCondition() == false)
            return false;

        // �ൿ�� ������ ������ �Լ�
        baseState.ExitState(_nextState, _nextBaseState);
        baseState = _nextBaseState;
        State = _nextState;
        baseState.EnterState();

        return true;
    }

    public void RemoveState(Define.State removeState)
    {
        // �⺻ ���� ���� �Ұ�
        if (defaultState == removeState)
            return;

        // ���� ���� ���¿� ������ ���°� ������ ���� �Ұ�
        if (State == removeState)
            return;

        // ������ ������ ������ ���
        if (stateDic.ContainsKey(removeState) == false)
            return;

        int stateIndex = (int)state;
        if ((int)Define.State.Skill <= stateIndex && stateIndex < (int)Define.State.MAX)
        {
            skillCount--;
        }

        // �ش� ���� ����
        stateDic.Remove(removeState);
    }

    public List<Define.State> UseableSkill()
    {
        if (skillCount == 0)
            return null;

        int start = (int)Define.State.Skill;
        int end = (int)Define.State.MAX;

        skillList.Clear();

        for (int i = start; i < end; i++)
        {
            Define.State eState = (Define.State)i;
            if (stateDic.ContainsKey(eState))
            {
                skillList.Add(eState);
            }
        }

        return skillList;
    }

    public int GetSkillCount()
    {
        return skillCount;
    }
}
