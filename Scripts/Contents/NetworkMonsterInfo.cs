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

        // ���� �����̸� �̸��� ���ݿ��� �޾ƿ´�.
        if (targetStat is MonsterStat)
        {
            targetStat.OnNameAction -= uI_NetworkObjectInfo.UpdateNickName;
            targetStat.OnNameAction += uI_NetworkObjectInfo.UpdateNickName;
        }
    }

    #region NetworkBehaviour �ݹ� �Լ�
    public override void OnStartClient()
    {
        base.OnStartClient();

        // ���� �����̸� �̸��� ���ݿ��� �������� �ʱ�ȭ �Ѵ�.
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
