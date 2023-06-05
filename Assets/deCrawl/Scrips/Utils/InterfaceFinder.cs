using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.World;
using DeCrawl.Primitives;

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

        private static IEnumerable<MonoBehaviour> FindMonoBehavioursWithInterface<T>()
        {
            MonoBehaviour[] behaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T)
                {
                    yield return behaviours[i];
                }
            }
        }


        public static IGrid FindMonoBehaviourWithIGrid() =>
            FindFirstMonoBehavioursWithInterface<IGrid>() as IGrid;
        

        public static IEnumerable<IMovingEntity> FindMonoBehavioursWithIMovingEntity() =>
            FindMonoBehavioursWithInterface<IMovingEntity>()
            .Select(behaviour => behaviour as IMovingEntity);
            

        public static IMovingEntity FindMonoBehaviourWithIMovingEntity() =>
            FindFirstMonoBehavioursWithInterface<IMovingEntity>() as IMovingEntity;

        public static IEnumerable<IPhased> FindMonoBeahviourWithIPhased() =>
            FindMonoBehavioursWithInterface<IPhased>()
            .Select(behaviour => behaviour as IPhased);

        public static ICurrencyPurse FindMonoBehaviourWithICurrencyPurse() =>
            FindFirstMonoBehavioursWithInterface<ICurrencyPurse>() as ICurrencyPurse;
    }
}
