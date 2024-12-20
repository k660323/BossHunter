using Mirror;
using System.Collections;
using UnityEngine;

public class InAnimate : NetworkBehaviour
{
    protected uint cachedNetId;

    protected PhysicsScene physicsScene;

    public override void OnStartServer()
    {
        base.OnStartServer();

        physicsScene = gameObject.scene.GetPhysicsScene();

        cachedNetId = netId;

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.InsertInAnimate(netId, gameObject);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.RemoveInAnimate(cachedNetId);

        Managers.Resource.NetworkDestory(gameObject, true);
    }
}
