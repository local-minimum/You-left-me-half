using UnityEngine;

namespace FP
{
    [System.Serializable]
    public struct ColorSettings
    {
        [Tooltip("If color settings should be applied")]
        public bool InUse;
        public Color Default;
        public Color Hover;
        public Color Disabled;

        public Color GetColor(bool hovered, bool disabled, bool prioritizeDisabled = true)
        {
            if (prioritizeDisabled)
            {
                if (disabled) return Disabled;
                return hovered ? Hover : Default;
            } else
            {
                if (hovered) return Hover;
                return disabled ? Disabled : Default;
            }
        }
    }
}
