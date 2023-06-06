using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;

namespace DeCrawl.Systems.Storage {
    public class PlayerPrefsStorage : MonoBehaviour, IStorage
    {
        [SerializeField] string KeyPrefix = "save";

        [SerializeField] bool CompressData;

        [SerializeField] bool _Enabled = true;
        public bool Enabled => _Enabled;

        private string Key(string key) => string.IsNullOrEmpty(KeyPrefix) ? key : $"{KeyPrefix}-{key}";
        
        public bool Has(string key) => PlayerPrefs.HasKey(Key(key));

        public bool Read(string key, out string data)
        {
            var fullKey = Key(key);
            if (Has(key))
            {
                data = PlayerPrefs.GetString(fullKey);
                if (CompressData)
                {
                    try
                    {
                        data = StringCompressor.DecompressString(data);
                    } catch (System.FormatException)
                    {
                        Debug.Log($"Could not decompress {data}");
                        return false;
                    }
                }
                return true;       
            }

            data = null;
            return false;
        }

        public bool Save(string key, string data)
        {
            PlayerPrefs.SetString(Key(key), CompressData ? StringCompressor.CompressString(data) : data);
            return true;
        }
    }
}