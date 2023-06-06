using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Systems.Storage
{

    public interface IStorage
    {
        public bool Enabled { get; }

        public bool Read(string key, out string data);

        public bool Save(string key, string data);

        public bool Has(string key);
    }
}
