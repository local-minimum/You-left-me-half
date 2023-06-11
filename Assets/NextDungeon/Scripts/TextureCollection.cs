using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;

namespace ND {
    [CreateAssetMenu(fileName = "TextureCollection", menuName = "Sprite Tools/Texture Collection")]
    public class TextureCollection : ScriptableObject
    {        
        public new string name;

        [SerializeField, WithAction("InitID", "Generate", "Constructs a new ID based on the name and a random number")]
        private string _id;


        void InitID()
        {
            _id = GenerateID();
        }

        public string id { 
            get { 
                if (string.IsNullOrEmpty(_id)) { 
                    _id = GenerateID(); 
                } 
                return _id;
            } 
            set { _id = value; } 
        }

        public Texture2D[] textures;

        string GenerateID() => $"{name}-{Random.Range(0, (int) 1e8)}";        
    }
}
