using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace YLHalf
{
    [CustomEditor(typeof(InventoryRack), true)]
    public class InventoryRackEditor : Editor
    {

        public bool showCorruption = true;
        public bool showOccupancy = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            InventoryRack rack = (InventoryRack)target;
            EditorGUILayout.Space();

            showCorruption = EditorGUILayout.Foldout(showCorruption, "Corruption");
            if (showCorruption)
            {
                EditorGUI.indentLevel++;
                var corruption = rack.CorruptionAsStrings.ToArray();
                for (int i = 0; i < corruption.Length; i++)
                {
                    EditorGUILayout.LabelField(corruption[i]);
                }
                EditorGUI.indentLevel--;
            }

            showOccupancy = EditorGUILayout.Foldout(showCorruption, "Occupied");
            if (showCorruption)
            {
                EditorGUI.indentLevel++;
                var occupancy = rack.OccupancyAsStrings.ToArray();
                for (int i = 0; i < occupancy.Length; i++)
                {
                    EditorGUILayout.LabelField(occupancy[i]);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}