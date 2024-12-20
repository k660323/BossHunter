using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatePlayer : BaseState
{
    private Player player;
    public Player PlayerGS { get { return player; } protected set { player = value; } }

    private PlayerController playerController;

    public PlayerController PlayerControllerGS { get { return playerController; } protected set { playerController = value; } }

    protected BaseStatePlayer(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {
        player = _player;
        playerController = _playerController;
    }
}
