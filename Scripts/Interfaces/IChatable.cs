using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChatable
{
    #region ä��
    public abstract void CTS_ChatRPC(string message);

    public abstract void ChatRPC(string message);
    #endregion
}
