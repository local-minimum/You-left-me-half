using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoboxHUD : MonoBehaviour
{
    int level = 1;
    int tokens;

    [SerializeField]
    TMPro.TextMeshProUGUI ui;

    private void OnEnable()
    {
        LevelTracker.OnLevelUp += LevelTracker_OnLevelUp;
        LevelTracker.OnLevelTokenChange += LevelTracker_OnLevelTokenChange;
    }

    private void OnDisable()
    {
        LevelTracker.OnLevelUp -= LevelTracker_OnLevelUp;
        LevelTracker.OnLevelTokenChange -= LevelTracker_OnLevelTokenChange;
    }

    private void Start()
    {
        ui.text = InfoText;
    }

    private void LevelTracker_OnLevelTokenChange(int tokens)
    {
        this.tokens = tokens;
        ui.text = InfoText;
    }

    private void LevelTracker_OnLevelUp(int level)
    {
        this.level = level;
        ui.text = InfoText;
    }

    string InfoText
    {
        get => $"Lvl: {level}\nInventory Repairs: {tokens}";
    }
}
