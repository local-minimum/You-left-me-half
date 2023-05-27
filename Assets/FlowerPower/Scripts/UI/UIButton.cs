using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FP
{
    public class UIButton : MonoBehaviour
    {
        public delegate void ButtonClickEvent(UIButton button);
        public delegate void ButtonHoverEvent(UIButton button);

        public event ButtonClickEvent OnClick;
        public event ButtonHoverEvent OnHover;

        [System.Serializable]
        public struct ColorSettings
        {
            [Tooltip("If color settings should be applied")]
            public bool InUse;
            public Color Default;
            public Color Hover;
            public Color Disabled;

            public Color GetColor(bool hovered, bool disabled)
            {
                if (disabled) return Disabled;
                return hovered ? Hover : Default;
            }
        }

        bool _disabled;
        public bool Disabled {
            get => _disabled;
            set
            {
                _disabled = value;
                SyncState();
            }
        }

        bool _hovered;
        public bool Hovered
        {
            get => _hovered;
            set
            {
                _hovered = value;
                SyncState();
                OnHover?.Invoke(this);
            }
        }

        [Header("Selection Image")]
        [SerializeField]
        Image ActiveChoiceImage;

        [Header("Text")]
        [SerializeField]
        TMPro.TextMeshProUGUI Text;
        [SerializeField]
        ColorSettings TextColor;

        [Header("Background")]
        [SerializeField]
        Image BackgroundImage;
        [SerializeField]
        ColorSettings BackgroundColor;

        public void SyncState()
        {
            ActiveChoiceImage.enabled = Hovered && !Disabled;
            if (Text != null && TextColor.InUse)
            {
                Text.color = TextColor.GetColor(Hovered, Disabled);
            }
            if (BackgroundImage != null && BackgroundColor.InUse)
            {
                BackgroundImage.color = BackgroundColor.GetColor(Hovered, Disabled);
            }
        }

        public void HandleClick()
        {
            if (!Disabled)
            {
                OnClick?.Invoke(this);
            }
        }


        private void OnEnable()
        {
            SyncState();
        }
    }
}
