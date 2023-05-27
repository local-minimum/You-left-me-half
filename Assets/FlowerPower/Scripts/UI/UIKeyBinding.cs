using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Systems;

namespace FP
{
    public class UIKeyBinding : MonoBehaviour
    {
        [SerializeField]
        DungeonInput.InputEvent inputEvent;

        [SerializeField]
        int bindingIndex = 0;

        [SerializeField]
        TMPro.TextMeshProUGUI Text;

        [SerializeField]
        ColorSettings colorSettings;

        [SerializeField]
        Image backgrounImage;

        UISettings _Settings;

        bool binding;

        bool _PointerIsOver;
        public bool PointerIsOver { 
            get => _PointerIsOver; 
            set {
                _PointerIsOver = value;
                SyncKey();
            }
        }

        UISettings Settings
        {
            get
            {
                if (_Settings == null)
                {
                    _Settings = GetComponentInParent<UISettings>();
                }
                return _Settings;
            }
        }

        private void OnEnable()
        {
            DungeonInput.OnInput += DungeonInput_OnInput;
            SyncKey();
        }

        private void OnDisable()
        {
            DungeonInput.OnInput -= DungeonInput_OnInput;    
        }

        public void StartBind()
        {
            if (Settings.KeyBinding || Time.realtimeSinceStartup < nextBindAllowed) return;

            Settings.KeyBinding = true;
            binding = true;
            Text.text = "[PRESS KEY]";
        }

        float noRebindTime = 0.5f;
        float nextBindAllowed = -1f;

        void StopBinding()
        {
            Settings.KeyBinding = false;
            binding = false;

            foreach (var keyBinding in transform.parent.GetComponentsInChildren<UIKeyBinding>())
            {
                keyBinding.SyncKey();
            }
            
            nextBindAllowed = Time.realtimeSinceStartup + noRebindTime;
        }

        public void SyncKey()
        {
            var key = DungeonInput.instance.GetKey(inputEvent, bindingIndex);
            if (!binding)
            {
                if (key == KeyCode.None)
                {
                    Text.text = "[UNSET]";
                }
                else
                {
                    Text.text = $"{key}";
                }
            }
            if (backgrounImage && colorSettings.InUse)
            {
                backgrounImage.color = colorSettings.GetColor(binding || PointerIsOver, key == KeyCode.None, false);
            }
        }

        private void DungeonInput_OnInput(DungeonInput.InputEvent input, DungeonInput.InputType type)
        {
            if (!binding || !DungeonInput.OverlappingTypes(type, DungeonInput.InputType.Down)) return;
            
            if (input == DungeonInput.InputEvent.Abort && inputEvent != DungeonInput.InputEvent.Abort)
            {
                StopBinding();
                return;
            }
        }

        static KeyCode[] _allKeys;

        static KeyCode[] AllKeys
        {
            get
            {
                if (_allKeys == null)
                {
                    _allKeys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
                }
                return _allKeys;
            }
        }

        static KeyCode KeyPressed
        {
            get
            {
                var keys = AllKeys;
                for (int i=0; i<keys.Length; i++)
                {
                    if (Input.GetKeyDown(keys[i]))
                    {
                        return keys[i];
                    }
                }
                return KeyCode.None;
            }
        }

        private void Update()
        {
            if (!binding) return;
            if (Input.anyKeyDown)
            {
                var keyCode = KeyPressed;
                if (keyCode != KeyCode.None)
                {
                    DungeonInput.instance.SetKey(inputEvent, bindingIndex, keyCode);
                    Settings.HasCustomBindings = true;
                }

                StopBinding();
            }
        }
    }
}
