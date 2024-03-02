using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Session : NetworkBehaviour
{
    public override void Spawned()
    {
        NetworkManager.Instance.Session = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }

    public void LoadGameScene()
    {
        if (Object.HasStateAuthority && (Runner.CurrentScene == 0 || Runner.CurrentScene == SceneRef.None))
        {
            Runner.SetActiveScene(NetworkManager.GameScene);
        }
    }

}
