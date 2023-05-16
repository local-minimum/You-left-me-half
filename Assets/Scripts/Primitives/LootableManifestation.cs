using DeCrawl.World;

namespace YLHalf
{
    /// <summary>
    /// A simple manifestation of loot in the world that activates the gameobject
    /// when visibile and allows for pickup if the player is one tile away
    /// no matter where on the tile the loot is.
    /// </summary>
    public class LootableManifestation : AbstractLootableManifestation<GridEntity, bool>
    {
        protected override float MaxWorldPickupDistance => 4f;
        protected override float IgnoreHightDistanceThreshold => 1.5f;
    }
}