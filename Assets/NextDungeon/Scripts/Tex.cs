using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ND
{
    [System.Serializable]
    public class Tex
    {
        [SerializeField]
        Color[] data;

        [SerializeField]
        int width;

        [SerializeField]
        int height;

        Texture2D _Texture;

        public Tex(Color[] data, int width, int height)
        {
            if (data.Length != width * height) throw new System.ArgumentException($"{width}x{height} is not {data.Length}");

            this.data = data;
            this.width = width;
            this.height = height;
        }

        public Texture2D Texture {
            get {
                if (_Texture == null)
                {
                    _Texture = new Texture2D(width, height);
                    _Texture.SetPixels(data);
                    _Texture.Apply();
                }
                return _Texture;
            }
        }
    }
}
