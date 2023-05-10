using DeCrawl.Primitives;
using UnityEngine;

namespace DeCrawl.World
{
    /// <summary>
    /// A simple manifestation of loot in the world that activates the gameobject
    /// when visibile and allows for pickup if the player is close enough
    /// </summary>
    abstract public class AbstractLootableManifestation<Entity, ClaimCondition> : WorldClickable
    {
        protected abstract float MaxWorldPickupDistance { get; }
        protected abstract float IgnoreHightDistanceThreshold { get;  }

        Lootable lootable;

        private void Awake()
        {
            lootable = GetComponentInParent<Lootable>();
            lootable.OnManifestChange += Lootable_OnManifestChange;
        }

        private void OnDestroy()
        {
            lootable.OnManifestChange -= Lootable_OnManifestChange;
        }

        private void Lootable_OnManifestChange(bool visible)
        {
            gameObject.SetActive(visible);
        }

        protected override bool PreClickCheckRefusal() => !lootable.enabled;        

        protected override bool RefuseClick()
        {
            if (lootable.Owner != LootOwner.Level) return false;
            var offset = AbstractPlayerController<Entity, ClaimCondition>.instance.transform.position - transform.position;
            if (Mathf.Abs(offset.y) < IgnoreHightDistanceThreshold)
            {
                offset.y = 0;
            }
            Debug.Log(offset);
            Debug.Log(offset.magnitude);
            return offset.magnitude > MaxWorldPickupDistance;
        }

        protected override void OnClick()
        {
            lootable.Loot(LootOwner.Player);
        }
    }
}
