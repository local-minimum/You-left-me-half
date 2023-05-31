using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FP
{
    public class SingleEntity : MonoBehaviour
    {
        static Dictionary<System.Type, SingleEntity> RegisteredBehaviors = new Dictionary<System.Type, SingleEntity>();

        [SerializeField]
        MonoBehaviour behaviour;

        System.Type behaviourType => behaviour.GetType();

        private void Awake()
        {
            if (RegisteredBehaviors.ContainsKey(behaviourType))
            {
                Destroy(gameObject);
            }
            else
            {
                RegisteredBehaviors.Add(behaviourType, this);
            }
        }

        private void OnDestroy()
        {
            if (RegisteredBehaviors.ContainsKey(behaviourType) && RegisteredBehaviors[behaviourType] == this)
            {
                RegisteredBehaviors.Remove(behaviourType);
            }
        }
    }
}
