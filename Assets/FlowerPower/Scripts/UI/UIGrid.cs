using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FP
{
    [ExecuteInEditMode]
    public class UIGrid : MonoBehaviour
    {
        [SerializeField, Range(1, 20)]
        int Columns;

        [SerializeField, Range(1, 20)]
        int Rows;

        [SerializeField]
        bool HideOverFlow = true;

        (int, int) IndexAsRowCol(int index) => (index / Columns, index % Columns);



        void AlignChild(RectTransform child, int row, int col)
        {
            child.offsetMax = Vector2.zero;
            child.offsetMin = Vector2.zero;
            child.anchorMin = new Vector2(col * 1f / Columns, (Rows - row - 1) * 1f / Rows);
            child.anchorMax = new Vector2((col + 1) * 1f / Columns, (Rows - row) * 1f / Rows);
        }

        void ShapeChildren()
        {
            var overflowIndex = Columns * Rows;
            for (int i=0, l=transform.childCount; i<l; i++)
            {
                var childTransform = transform.GetChild(i).transform as RectTransform;

                if (i >= overflowIndex && HideOverFlow)
                {
                    childTransform.gameObject.SetActive(false);
                    continue;
                }

                var (row, col) = IndexAsRowCol(i);
                AlignChild(childTransform, row, col);
            }
        }

        private void Update()
        {
            ShapeChildren();
        }
    }
}
