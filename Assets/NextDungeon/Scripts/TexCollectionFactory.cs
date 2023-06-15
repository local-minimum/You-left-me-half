using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ND
{
    [CreateAssetMenu(fileName = "TexCollection Factory", menuName = "Sprite Tools/Texture Collection Factory")]
    public class TexCollectionFactory : ScriptableObject
    {
        [SerializeField]
        public new string name;

        [Header("Configuration")]

        [SerializeField]
        Texture2D source;

        [SerializeField]
        Vector2Int padding = Vector2Int.zero;

        [SerializeField]
        Vector2Int spacing = Vector2Int.zero;

        [SerializeField]
        public Vector2Int tileSize = new Vector2Int(32, 32);

        [SerializeField, Tooltip("Sprites start top left rather than bottom left corner")]
        bool invertY = true;

        [SerializeField, Tooltip("Previews at x2 scale")]
        public bool magnified = true;

        [Header("Collection Production Settings")]
        //[ShowIf("sprite")]
        //[DynamicRange("spriteTilesLowerBounds")]
        [SerializeField, Range(0, 999)]
        int lowerBound = 0;

        [SerializeField, Range(0, 999)]
        int upperBound = 8;

        public int Magnification => magnified ? 2 : 1;

        public Vector2Int TargetTileSize => new Vector2Int(tileSize.x * Magnification, tileSize.y * Magnification);

        private void OnValidate()
        {
            if (tileSize.x == 0 || tileSize.y == 0)
            {
                Debug.LogError($"Not all dimensions on {name} has a size ({tileSize})");
            }
        }

        int TileCols
        {
            get
            {
                if (source == null) return 0;
                int availableWidth = source.width - 2 * padding.x;
                return (availableWidth + spacing.x) / (spacing.x + tileSize.x);
            }
        }

        int TileRows
        {
            get
            {
                if (source == null) return 0;

                int availableHeight = source.height - 2 * padding.y;
                return (availableHeight + spacing.y) / (spacing.y + tileSize.y);
            }
        }

        RectInt GetRect(int x, int y) {
            var yOffset = y * (tileSize.y + spacing.y) + padding.y;
            return new RectInt(
            new Vector2Int(
                (x * (tileSize.x + spacing.x) + padding.x),
                invertY ? source.height - yOffset - tileSize.y  : yOffset
            ),
            tileSize);
        }
        

        RectInt GetRect(int index)
        {
            int columns = TileCols;
            int rows = TileRows;
            return GetRect(index % columns, Mathf.Min(rows - 1, index / columns));            
        }

        public IEnumerable<RectInt> TileRects()
        {
            
            for (int i=Mathf.Min(lowerBound, upperBound); i <= Mathf.Max(lowerBound, upperBound); i++)
            {
                yield return GetRect(i);
            }
        }

        public TextureFormat TextureFormat => source.format;

        public Color[] GetPixels(RectInt rect) => source.GetPixels(rect.x, rect.y, rect.width, rect.height);

        public void WriteTexture(RectInt rect, Texture2D target)
        {
            //Debug.Log($"{rect} from {source.width}x{source.height} => {target.width}x{target.height}");
            if (magnified)
            {
                var w = target.width;
                var m = Magnification;
                var pixels = GetPixels(rect);
                var scaled = new Color[pixels.Length * m * m];

                for (int i = 0; i<pixels.Length; i++)
                {
                    int sX = i % rect.width;
                    int sY = i / rect.height;
                    for (int oX = 0; oX < m; oX++)
                    {
                        for (int oY = 0; oY < m; oY++)
                        {
                            scaled[sX * m + oX + w * (sY * m + oY)] = pixels[i];
                        }
                    }
                }
                target.SetPixels(scaled);
            } else
            {
                target.SetPixels(source.GetPixels(rect.x, rect.y, rect.width, rect.height));
            }            
            target.Apply();
        }
    }
}
