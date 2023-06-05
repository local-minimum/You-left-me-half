using UnityEngine;

namespace DeCrawl.Primitives
{
    public class FindingSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static FindingSingleton<T> _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FindingSingleton<T>>();
                }
                return _instance as T;
            }
        }

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogError($"Duplicate Singleton: {_instance} exists yet {this} also exists");
                Destroy(this);
            }
        }

        protected void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}