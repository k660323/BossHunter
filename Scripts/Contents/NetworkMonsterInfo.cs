using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMonsterInfo : NetworkObjectInfo
{
    //UI
    [SerializeField]
    protected UI_NetworkMonsterInfo ui_NetworkMonsterInfo;
    public UI_NetworkMonsterInfo Ui_NetworkMonsterInfo { get { return ui_NetworkMonsterInfo; } set { ui_NetworkMonsterInfo = value; } }

    public override void Init()
    {
        base.Init();

        if (uI_NetworkObjectInfo == false)
            return;

        ui_NetworkMonsterInfo = uI_NetworkObjectInfo as UI_NetworkMonsterInfo;

        // 몬스터 스텟이면 이름을 스텟에서 받아온다.
        if (targetStat is MonsterStat)
        {
            targetStat.OnNameAction -= uI_NetworkObjectInfo.UpdateNickName;
            targetStat.OnNameAction += uI_NetworkObjectInfo.UpdateNickName;
        }
    }

    #region NetworkBehaviour 콜백 함수
    public override void OnStartClient()
    {
        base.OnStartClient();

        // 몬스터 스텟이면 이름을 스텟에서 가져오고 초기화 한다.
        if (targetStat is MonsterStat)
        {
            uI_NetworkObjectInfo.UpdateNickName(targetStat.CreatureName);
        }
    }
    #endregion

    private void OnDestroy()
    {
        targetStat.OnNameAction -= uI_NetworkObjectInfo.UpdateNickName;
    }
}
