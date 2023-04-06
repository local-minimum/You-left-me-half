using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInventoryHUD : HUDProgressionIcon
{
    public Attack attack;

    private void Start()
    {
        Configure();
    }

    void Configure() => Configure(attack.textureProgress, attack.textureOverlay, attack.fillMethod);
}
