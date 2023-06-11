using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace ND
{
    /// <summary>
    /// Idea but not made complete because tired
    /// </summary>
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute), true)]
    public class MinMaxRangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinMaxRangeAttribute minMaxAttrib = attribute as MinMaxRangeAttribute;
            var lower = property.FindPropertyRelative("lowerBound");
            var upper = property.FindPropertyRelative("upperBound");

            EditorGUI.BeginProperty(position, label, property);

            int lowerInt = lower.intValue;
            // EditorGUI.MinMaxSlider(label, position, ref lowerInt, ref upper, minMaxAttrib.minValue, minMaxAttrib.maxValue);

            EditorGUI.EndProperty();
        }
    }
}

#endif