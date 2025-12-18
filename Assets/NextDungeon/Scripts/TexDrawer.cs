using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ND
{
    [CustomPropertyDrawer(typeof(Tex), true)]
    public class TexDrawer : PropertyDrawer
    {
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        Texture2D GetTexture(SerializedProperty property)
        {
            var key = property.propertyPath;

            if (textures.ContainsKey(key)) return textures[key];
            
            int width = property.FindPropertyRelative("width").intValue;
            int height = property.FindPropertyRelative("height").intValue;

            var data = property.FindPropertyRelative("data");
            var colors = new Color[data.arraySize];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = data.GetArrayElementAtIndex(i).colorValue;
            }

            var tex = new Texture2D(width, height);
            tex.SetPixels(colors);
            tex.Apply();

            textures[key] = tex;

            return tex;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 
            property.FindPropertyRelative("height").intValue;
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            EditorGUI.BeginProperty(position, label, property);

            var tex = GetTexture(property);
            position.width = tex.width;
            position.height = tex.height;


            GUI.DrawTexture(position, tex);
            
            EditorGUI.EndProperty();
        }
    }
}
