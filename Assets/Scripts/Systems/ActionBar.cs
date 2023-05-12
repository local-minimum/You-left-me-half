using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;

namespace YLHalf
{
    public class ActionBar : MonoBehaviour
    {
        List<Attack> Attacks = new List<Attack>();
        List<GameObject> AttackUI = new List<GameObject>();
        [SerializeField]
        private int MaxAttacks = 8;

        private void OnEnable()
        {
            Inventory.OnInventoryChange += Inventory_OnInventoryChange;
        }

        private void OnDisable()
        {
            Inventory.OnInventoryChange -= Inventory_OnInventoryChange;
        }

        int FirstEmptySlot
        {
            get
            {
                int i = 0;
                int l = Attacks.Count;
                while (i < l)
                {
                    if (Attacks[i] == null) return i;
                    i++;
                }
                Attacks.Add(null);
                AttackUI.Add(null);
                return i;
            }
        }

        GameObject CreateActionUI(Attack attack)
        {
            var go = new GameObject($"Attack {attack}");
            var image = go.AddComponent<Image>();
            image.preserveAspect = true;

            var tex = attack.texture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            sprite.name = $"Attack BG {attack}";
            image.sprite = sprite;

            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform);

            var hud = go.AddComponent<AttackInventoryHUD>();
            hud.attack = attack;
            hud.IsActionHud = true;

            return go;
        }

        private void Slot(Attack attack)
        {
            int slot = FirstEmptySlot;
            Attacks[slot] = attack;
            if (slot < MaxAttacks)
            {
                AttackUI[slot] = CreateActionUI(attack);
            }
        }

        private void EnableWaitingAttacks()
        {
            if (Attacks.Count <= MaxAttacks) return;

            // TODO: Fill up and create for empty lower indices
        }

        private void UnSlot(Attack attack)
        {
            var i = Attacks.IndexOf(attack);
            if (i < 0)
            {
                Debug.LogError($"Could not find {attack.Id}");
                return;
            }
            Attacks[i] = null;
            if (i < MaxAttacks)
            {
                Destroy(AttackUI[i]);
                AttackUI[i] = null;
                EnableWaitingAttacks();
            }
        }

        private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
        {
            if (loot.GetType() == typeof(Attack))
            {
                switch (inventoryEvent)
                {
                    case InventoryEvent.PickUp:
                        Slot(loot as Attack);
                        break;
                    case InventoryEvent.Drop:
                        UnSlot(loot as Attack);
                        break;
                }
            }
        }
    }
}
