using UnityEngine;

namespace DeCrawl.Primitives
{
    /// <summary>
    /// The world camera must be a child of the player controller game object for this to work
    /// </summary>
    public class WorldCamera : FindingSingleton<WorldCamera>
    {
        public Vector3 GriddedWorldPosition => transform.parent.position;
    }
}
