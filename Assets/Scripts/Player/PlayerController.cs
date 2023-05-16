using DeCrawl.World;
using DeCrawl.Primitives;

namespace YLHalf
{
    public class PlayerController : AbstractPlayerController<GridEntity, bool>
    {
        protected override GridEntity PlayerEntity => GridEntity.Player;
        protected override bool ClaimCond => !inventory.Has(loot => loot is Uplink, out Lootable loot);

        private ILevel<GridEntity, bool> level;
        public override ILevel<GridEntity, bool> Level
        {
            get
            {
                if (level != null) return level;
                level = FindObjectOfType<Level>();
                return level;
            }
        }

        private Inventory inventory;

        private new void Start()
        {
            inventory = GetComponentInChildren<Inventory>();
            base.Start();
        }

        protected void OnEnable()
        {
            MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
        }

        protected void OnDisable()
        {
            MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
        }

        private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
        {
            enabled = false;
        }

    }
}