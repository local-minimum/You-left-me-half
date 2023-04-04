using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AutoBootLooter : MonoBehaviour
{
    IEnumerable<Lootable> LootableDecendants
    {
        get
        {
            var nChildren = transform.childCount;
            for (int i = 0; i<nChildren; i++)
            {
                var child = transform.GetChild(i);
                var loot = child.GetComponentInChildren<Lootable>();
                if (loot != null) yield return loot;
            }
        }
    }

    private void AutoLoot()
    {
        var candidates = (OnlyDirectDecendants ? LootableDecendants : GetComponentsInChildren<Lootable>())
    .ToArray();


        for (int i = 0; i < candidates.Length; i++)
        {
            if (!candidates[i].Loot(OwnerType))
            {
                Debug.LogWarning($"No-one wanted {candidates[i].Id}");
            }
        }
    }

    [SerializeField]
    bool OnlyDirectDecendants = true;

    [SerializeField]
    LootOwner OwnerType;

    private void OnEnable()
    {
        AutoLoot();
    }
}