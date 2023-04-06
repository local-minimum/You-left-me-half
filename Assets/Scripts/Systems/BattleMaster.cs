using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public delegate void HitPlayer(int amount);
public delegate void HitMonster(string monsterId, int amount);


public class BattleMaster : MonoBehaviour
{
    public static event HitPlayer OnHitPlayer;
    public static event HitMonster OnHitMonster;

    [SerializeField]
    BattleHit hitPrefab;

    List<BattleHit> uiHits = new List<BattleHit>();

    private void Start()
    {
        uiHits.AddRange(GetComponentsInChildren<BattleHit>());
    }

    BattleHit GetFreeHitUI()
    {
        var hit = uiHits.FirstOrDefault(hit => !hit.gameObject.activeSelf);

        if (hit == null)
        {
            hit = Instantiate(hitPrefab);
            hit.transform.SetParent(transform);
            uiHits.Add(hit);
        }
        return hit;
    }

    private void OnEnable()
    {
        AttackInventoryHUD.OnAttack += AttackInventoryHUD_OnAttack;
        PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
    }

    private void OnDisable()
    {
        AttackInventoryHUD.OnAttack -= AttackInventoryHUD_OnAttack;
        PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
    }

    Vector3Int playerPosition;
    FaceDirection playerLookDirection;

    private void PlayerController_OnPlayerMove(Vector3Int position, FaceDirection lookDirection)
    {
        playerPosition = position;
        playerLookDirection = lookDirection;
    }

    private string GetMonsterTarget(Attack attack)
    {
        return null;
    }

    private void AttackInventoryHUD_OnAttack(Attack attack)
    {
        var type = attack.GetAttack(out int amount);
        var target = GetMonsterTarget(attack);

        if (target != null)
        {
            OnHitMonster(target, amount);
        }
        
        var hit = GetFreeHitUI();
        hit.SetHit(amount, type);
        hit.gameObject.SetActive(true);
    }
}
