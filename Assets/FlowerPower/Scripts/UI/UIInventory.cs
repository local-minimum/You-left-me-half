using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace FP {
    public class UIInventory : FindingSingleton<UIInventory>, IUIMenuView
    {
        [SerializeField]
        UIInventoryItem Prefab;

        [SerializeField]
        RectTransform Container;

        public UIMenuSystem.State State => UIMenuSystem.State.Inventory;

        public bool Active
        {
            set
            {                
                gameObject.SetActive(value);
            }
        }

        UIMenuSystem menuSystem;

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
            menuSystem.OnChangeState += UIInventory_OnChangeState;
        }

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;
        }
        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (input != DungeonInput.InputEvent.Inventory) return;
            if (DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down))
            {
                menuSystem.state = UIMenuSystem.State.Hidden;
            }
        }

        new protected void OnDestroy()
        {
            menuSystem.OnChangeState -= UIInventory_OnChangeState;
            base.OnDestroy();
        }

        private void UIInventory_OnChangeState(UIMenuSystem.State state)
        {
            gameObject.SetActive(state == UIMenuSystem.State.Inventory);
        }

        public void Configure(Inventory inventory)
        {
            var letters = inventory.Where(l => l is LetterLoot).Select(ll => ((LetterLoot)ll).Letter).OrderBy(ch => ch).ToArray();
            var nChildren = Container.childCount;
            for (int i = 0, l = Mathf.Max(nChildren, letters.Length); i<l; i++)
            {
                if (i > letters.Length)
                {
                    Container.GetChild(i).gameObject.SetActive(false);                    
                } else if (i < nChildren)
                {
                    Container.GetChild(i).GetComponent<UIInventoryItem>().Text.text = $"{letters[i]}";
                } else
                {
                    var newLetter = Instantiate(Prefab);
                    newLetter.transform.SetParent(Container);
                    newLetter.Text.text = $"{letters[i]}";
                }
            }
        }
    }
}