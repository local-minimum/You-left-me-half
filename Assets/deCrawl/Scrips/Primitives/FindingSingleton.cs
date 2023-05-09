using UnityEngine;

namespace DeCrawl.Primitives
{
    public class FindingSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private FindingSingleton<T> _instance;

        public FindingSingleton<T> instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FindingSingleton<T>>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}