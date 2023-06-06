using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UILoadSlot : UISaveSlot
    {
        UIMenuSystem menuSystem;

        private void Start()
        {
            menuSystem = GetComponentInParent<UIMenuSystem>();
        }

        public bool HasSave => GameSaver.instance?.HasSave(SaveSlot) ?? false;

        protected override void Button_OnClick(UIButton button)
        {
            GameSaver.instance.Load(SaveSlot);
            SyncWithSave();
            menuSystem.state = UIMenuSystem.State.Hidden;
        }
    }
}