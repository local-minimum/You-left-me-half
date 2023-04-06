using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMaster : MonoBehaviour
{
    private void OnEnable()
    {
        AttackInventoryHUD.OnAttack += AttackInventoryHUD_OnAttack;
    }

    private void OnDisable()
    {
        AttackInventoryHUD.OnAttack -= AttackInventoryHUD_OnAttack;
    }

    private void AttackInventoryHUD_OnAttack(Attack attack)
    {
        var type = attack.GetAttack(out int amount);
        Debug.Log($"{type}: {amount}");
    }
}
