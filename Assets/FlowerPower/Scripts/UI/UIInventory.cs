using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Primitives;

namespace FP {
    public class UIInventory : FindingSingleton<UIInventory>
    {
        [SerializeField]
        UIInventoryItem Prefab;

        private void Start()
        {
            GetComponentInParent<UIMenuSystem>().OnChangeState += UIInventory_OnChangeState;
        }

        new protected void OnDestroy()
        {
            GetComponentInParent<UIMenuSystem>().OnChangeState -= UIInventory_OnChangeState;
            base.OnDestroy();
        }

        private void UIInventory_OnChangeState(UIMenuSystem.State state)
        {
            gameObject.SetActive(state == UIMenuSystem.State.Inventory);
        }

        public void Configure(Inventory inventory)
        {
            var letters = inventory.Where(l => l is LetterLoot).Select(ll => ((LetterLoot)ll).Letter).OrderBy(ch => ch).ToArray();
            var nChildren = transform.childCount;
            for (int i = 0, l = Mathf.Max(nChildren, letters.Length); i<l; i++)
            {
                if (i > letters.Length)
                {
                    transform.GetChild(i).gameObject.SetActive(false);                    
                } else if (i < nChildren)
                {
                    transform.GetChild(i).GetComponent<UIInventoryItem>().Text.text = $"{letters[i]}";
                } else
                {
                    var newLetter = Instantiate(Prefab);
                    newLetter.transform.SetParent(transform);
                    newLetter.Text.text = $"{letters[i]}";
                }
            }
        }
    }
}