using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanisterHUD : HUDProgressionIcon
{
    public Canister canister;
    

    private void Start()
    {
        Configure();    
    }

    void Configure() => Configure(canister.textureProgress, canister.textureOverlay, canister.fillMethod, canister.Fill);

    private void Update()
    {
        progressImage.fillAmount = canister.Fill;
    }
}
