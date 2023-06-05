using DeCrawl.World;

namespace YLHalf
{
    /// <summary>
    /// Component for all things moving about on a level grid
    /// </summary>
    public class MovingEntity : AbstractMovingEntity<GridEntity, bool>
    {
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

        public GridEntity Entity;

        public override void ClaimPosition()
        {
            Level.ClaimPosition(Entity, Position, true);
        }
    }
}
