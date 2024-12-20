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
        // 기본 상태는 딱 한번만 설정가능
        if (this.defaultState != Define.State.None)
            return;

        // 상태 딕셔너리 추가
        stateDic.Add(defaultState, baseState);
        // 기본 동작 캐싱
        this.defaultState = defaultState;
        // 기본 상태 변수 값 넣고 초기화
        BaseState = baseState;
        State = defaultState;
        baseState.EnterState();
    }

    public void RegisterState(Define.State state, BaseState baseState)
    {
        // 덮어쓰기 방지 
        // 만약 변경하고 싶으면 삭제후 등록 권장
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
        // 같은 행동이면 리턴
        if (state == defaultState)
            return;

        // 다음 행동 키 가 있는지 확인
        if (stateDic.TryGetValue(defaultState, out BaseState _nextBaseState) == false)
            return;

        // 행동이 끝날때 실행할 함수
        if (baseState != null)
            baseState.ExitState(defaultState, _nextBaseState);

        baseState = _nextBaseState;
        State = defaultState;
        baseState.EnterState();
    }

    public bool ChangeState(Define.State _nextState, bool _checkCondition = true)
    {
        // 다음 행동 키 가 있는지 확인
        if (stateDic.TryGetValue(_nextState, out BaseState _nextBaseState) == false)
            return false;

        // 다음 행동 조건 체크
        if (_checkCondition && _nextBaseState.CheckCondition() == false)
            return false;

        // 행동이 끝날때 실행할 함수
        baseState.ExitState(_nextState, _nextBaseState);
        baseState = _nextBaseState;
        State = _nextState;
        baseState.EnterState();

        return true;
    }

    public void RemoveState(Define.State removeState)
    {
        // 기본 동작 삭제 불가
        if (defaultState == removeState)
            return;

        // 지금 현재 상태와 삭제할 상태가 같으면 삭제 불가
        if (State == removeState)
            return;

        // 삭제할 동작이 없으면 취소
        if (stateDic.ContainsKey(removeState) == false)
            return;

        int stateIndex = (int)state;
        if ((int)Define.State.Skill <= stateIndex && stateIndex < (int)Define.State.MAX)
        {
            skillCount--;
        }

        // 해당 동작 삭제
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
