using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Primitives;


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

        Enemy.OnAttackPlayer += Enemy_OnAttackPlayer;        
        AttackInventoryHUD.OnAttack += AttackInventoryHUD_OnAttack;
        PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
    }

    private void OnDisable()
    {
        Enemy.OnAttackPlayer -= Enemy_OnAttackPlayer;
        AttackInventoryHUD.OnAttack -= AttackInventoryHUD_OnAttack;
        PlayerController.OnPlayerMove -= PlayerController_OnPlayerMove;
    }

    private void Enemy_OnAttackPlayer(AttackMode mode, int amount)
    {
        
        OnHitPlayer?.Invoke(amount);

        var hit = GetFreeHitUI();
        hit.SetHit(amount, mode);
        hit.gameObject.SetActive(true);
    }

    Vector3Int playerPosition;
    CardinalDirection playerLookDirection;

    private void PlayerController_OnPlayerMove(Vector3Int position, CardinalDirection lookDirection)
    {
        playerPosition = position;
        playerLookDirection = lookDirection;
    }

    private string GetMonsterTarget(Attack attack)
    {
        var lookVector = playerLookDirection.AsIntVector();

        // This starts at 1 because you can never attack the same tile as you are
        for (int i = 1, l = attack.attackStats.attackRange; i <= l; i++)
        {
            if (i > 1)
            {
                throw new System.NotImplementedException("We need to check its valid to attack through tile");
            }

            var options = PositionRecorder.instance.GetEntities(playerPosition + i * lookVector);
            if (options.Length > 0)
            {
                return options[Random.Range(0, options.Length)];
            }
        }
        
        return null;
    }

    private void AttackInventoryHUD_OnAttack(Attack attack)
    {
        var mode = attack.attackStats.GetAttack(out int amount);
        var target = GetMonsterTarget(attack);

        if (target != null)
        {
            OnHitMonster?.Invoke(target, amount);

            var hit = GetFreeHitUI();
            hit.SetHit(amount, mode);
            hit.gameObject.SetActive(true);
        }
    }
}
