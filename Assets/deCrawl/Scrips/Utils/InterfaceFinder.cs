using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;

namespace DeCrawl.Utils
{
    public static class InterfaceFinder
    {
        private static MonoBehaviour FindFirstMonoBehavioursWithInterface<T>()
        {
            MonoBehaviour[] behaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T)
                {
                    return behaviours[i];
                }
            }
            return null;
        }

        public static IGrid FindMonoBehaviourWithIGrid()
        {
            return FindFirstMonoBehavioursWithInterface<IGrid>() as IGrid;
        }

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
            return FindFirstMonoBehavioursWithInterface<IMovingEntity>() as IMovingEntity;
        }
    }
}
