using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FP
{
    public class UIMenuSpawner : MonoBehaviour
    {
        [SerializeField]
        UIMenuSystem Prefab;

        UIMenuSystem _system;

        UIMenuSystem System
        {
            get
            {
                if (_system == null)
                {
                    _system = Instantiate(Prefab);
                }
                return _system;
            }
        }
    }
}
