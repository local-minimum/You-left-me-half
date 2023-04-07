using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class WorldClickable : MonoBehaviour
{
    bool hovered = false;

    private void OnMouseEnter()
    {
        hovered = true;
    }

    private void OnMouseExit()
    {
        hovered = false;
    }

    abstract protected bool PreClickCheckRefusal();
    abstract protected bool RefuseClick();
    abstract protected void OnClick();

    private void OnDisable()
    {
        hovered = false;
    }

    private void OnDestroy()
    {
        hovered = false;
    }

    private void Update()
    {
        if (!hovered || PreClickCheckRefusal()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (RefuseClick()) return;

            OnClick();
        }
    }
}
