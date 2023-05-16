using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

namespace DeCrawl.Utils
{
    public static class InterfaceFinder
    {
        public static IEnumerable<IMovingEntity> FindMonoBehavioursWithIMovingEntity()
        {
            MonoBehaviour[] behaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i< behaviours.Length; i++)
            {
                if (behaviours[i] is IMovingEntity)
                {
                    yield return behaviours[i] as IMovingEntity;
                }
            }
        }

        public static IMovingEntity FindMonoBehaviourWithIMovingEntity()
        {
            MonoBehaviour[] behaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IMovingEntity)
                {
                    return behaviours[i] as IMovingEntity;
                }
            }

            return null;
        }

    }
}
