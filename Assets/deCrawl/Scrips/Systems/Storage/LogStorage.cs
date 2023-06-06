using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Systems.Storage {
    public class LogStorage : MonoBehaviour, IStorage
    {
        public bool _Enabled = true;
        public bool Enabled => _Enabled;

        public bool Has(string key) => false;

        public bool Read(string key, out string data)
        {
            data = null;
            return false;
        }

        public bool Save(string key, string data)
        {
            Debug.Log($"+++ Save @ {key} +++\n{data}");
            return true;
        }
    }
}
