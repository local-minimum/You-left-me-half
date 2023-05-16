using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;
using DeCrawl.Lootables;

namespace YLHalf
{
    public class ActionBar : MonoBehaviour
    {
        List<ActionLoot> Actions = new List<ActionLoot>();
        List<GameObject> ActionUIs = new List<GameObject>();

        [SerializeField]
        private int MaxActions = 8;

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
                int l = Actions.Count;
                while (i < l)
                {
                    if (Actions[i] == null) return i;
                    i++;
                }
                Actions.Add(null);
                ActionUIs.Add(null);
                return i;
            }
        }

        GameObject CreateActionUI(ActionLoot actionLoot)
        {
            var go = new GameObject($"Action {actionLoot}");
            var image = go.AddComponent<Image>();
            image.preserveAspect = true;

            var tex = actionLoot.texture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            sprite.name = $"ActionLoot BG {actionLoot}";
            image.sprite = sprite;

            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform);

            var hud = go.AddComponent<AttackInventoryHUD>();
            if (actionLoot is Attack)
            {
                hud.attack = actionLoot as Attack;
            }            
            hud.IsActionHud = true;

            return go;
        }

        private void Slot(ActionLoot actionLoot)
        {
            int slot = FirstEmptySlot;
            Actions[slot] = actionLoot;
            if (slot < MaxActions)
            {
                ActionUIs[slot] = CreateActionUI(actionLoot);
            }
        }

        private void EnableWaitingAttacks()
        {
            if (Actions.Count <= MaxActions) return;

            // TODO: Fill up and create for empty lower indices
        }

        private void UnSlot(ActionLoot actionLoot)
        {
            var i = Actions.IndexOf(actionLoot);
            if (i < 0)
            {
                Debug.LogError($"Could not find {actionLoot.Id}");
                return;
            }
            Actions[i] = null;
            if (i < MaxActions)
            {
                Destroy(ActionUIs[i]);
                ActionUIs[i] = null;
                EnableWaitingAttacks();
            }
        }

        private void Inventory_OnInventoryChange(Lootable loot, InventoryEvent inventoryEvent, Vector3Int placement)
        {            
            if (loot is ActionLoot)
            {
                switch (inventoryEvent)
                {
                    case InventoryEvent.PickUp:
                        Slot(loot as ActionLoot);
                        break;
                    case InventoryEvent.Drop:
                        UnSlot(loot as ActionLoot);
                        break;
                }
            }
        }
    }
}
