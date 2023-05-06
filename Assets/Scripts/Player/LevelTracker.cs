using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void LevelUpEvent(int level);
public delegate void LevelTokenChange(int tokens);

public class LevelTracker : MonoBehaviour
{
    public static event LevelUpEvent OnLevelUp;
    public static event LevelTokenChange OnLevelTokenChange;

    [SerializeField]
    bool allowRepeatLastLevelConditions = true;

    int level;
    int tokens;

    [SerializeField]
    int[] xpNeededForLevel;

    [SerializeField]
    int[] tokensPerLevel;

    Inventory inventory;

    static LevelTracker instance { get; set; }

    public static bool ConsumeToken(int amount = 1)
    {
        if (instance.tokens - amount >= 0)
        {
            instance.tokens -= amount;
            OnLevelTokenChange?.Invoke(instance.tokens);
            return true;
        }
        return false;
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) Destroy(this);
    }

    private void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    private void OnEnable()
    {
        Inventory.OnCanisterChange += Inventory_OnCanisterChange;
    }

    private void OnDisable()
    {
        Inventory.OnCanisterChange -= Inventory_OnCanisterChange;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Inventory_OnCanisterChange(CanisterType type, int stored, int capacity)
    {
        if (type != CanisterType.XP) return;
        if (level >= xpNeededForLevel.Length && !allowRepeatLastLevelConditions) return;

        var refLevel = Mathf.Min(level, xpNeededForLevel.Length - 1);

        if (stored < xpNeededForLevel[refLevel]) return;


        tokens += tokensPerLevel[Mathf.Min(refLevel, tokensPerLevel.Length - 1)];
        level++;

        PropertyRecorder.SetInt(RecrodableProperty.PlayerLevel, level);
        PropertyRecorder.SetInt(RecrodableProperty.PlayerLevelTokens, tokens);

        inventory.Withdraw(xpNeededForLevel[refLevel], CanisterType.XP);
        OnLevelUp?.Invoke(level + 1);
        OnLevelTokenChange?.Invoke(tokens);
    }
}
