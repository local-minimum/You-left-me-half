using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ND {
    [CustomEditor(typeof(TexCollectionFactory), true)]
    public class TexCollectionFactoryEditor : Editor
    {
        private List<Texture2D> tiles = new List<Texture2D>();
        private int nSamples = 0;
        private float previewPadding = 2; 
        private TexCollectionFactory factory;

        private void OnEnable()
        {
            factory = target as TexCollectionFactory;
        }

        int maxShow => factory.magnified ? 32 : 42;

        Rect DrawGenerate(Rect previewsRect, float minX, RectInt[] rects)
        {
            previewsRect.x = minX;
            previewsRect.y += 10;
            var btnRect = new Rect(previewsRect);
            btnRect.height = 24;
            if (GUI.Button(btnRect, "Generate TexCollection"))
            {
                var texCollection = ScriptableObject.CreateInstance<TexCollection>();

                texCollection.name = "Generated Texture";

                texCollection.id = texCollection.GenerateID();

                texCollection.textures = rects
                    .Select(r => new Tex(factory.GetPixels(r), r.width, r.height))
                    .ToArray();

                var location = $"Assets/NextDungeon/Sprites/{texCollection.name}.asset";

                EditorUtility.SetDirty(texCollection);
                
                AssetDatabase.CreateAsset(texCollection, location);
                AssetDatabase.SaveAssets();

                AssetDatabase.Refresh();

                Debug.Log($"Created {texCollection.name} @ {location}");
            }

            previewsRect.y += 24 + 2;

            return previewsRect;
        }

        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();

            GUILayout.Label("Previews");
            
            var rects = factory.TileRects().ToArray();
            nSamples = rects.Length;

            // Calculate area we may populate
            var targetTileSize = factory.TargetTileSize;
            Rect previewsRect = EditorGUILayout.GetControlRect();
            float minX = previewsRect.x;

            // TODO: Does not account for preview padding
            float maxX = Mathf.FloorToInt(previewsRect.width / targetTileSize.x) * targetTileSize.x + previewsRect.x;

            // Ensure we have right format on previews
            if (tiles.Count > 0)
            {
                var tile = tiles[0];
                if (tile.width != targetTileSize.x || tile.height != targetTileSize.y)
                {
                    tiles.Clear();
                }
            }

            previewsRect = DrawGenerate(previewsRect, minX, rects);

            for (int i = 0, l = Mathf.Min(maxShow, nSamples); i<l; i++)
            {
                var idx = (nSamples > maxShow && i >= maxShow / 2) ? nSamples - (i + maxShow) / 2 + i : i;

                if (idx >= nSamples) continue;

                if (i == maxShow / 2 && nSamples > maxShow)
                {
                    previewsRect.x = minX;
                    previewsRect.y += targetTileSize.y * 1.5f + previewPadding ;
                }


                // Create texture if needed;
                if (i >= tiles.Count)
                {                    
                    tiles.Add(
                        new Texture2D(
                            targetTileSize.x, 
                            targetTileSize.y,
                            TextureFormat.RGBA32,
                            false)
                        );                    
                }

                // Copy texture
                factory.WriteTexture(rects[idx], tiles[i]);                

                // Draw preview
                var previewRect = new Rect(previewsRect);
                previewRect.width = tiles[i].width;
                previewRect.height = tiles[i].height;
                
                GUI.DrawTexture(previewRect, tiles[i]);

                // Offset for next preview
                if (previewsRect.x + previewRect.width < maxX)
                {
                    previewsRect.x += previewRect.width + previewPadding;
                } else {
                    previewsRect.y += previewRect.height + previewPadding;
                    previewsRect.x = minX;
                }               
            }

        }               
    }
}