using DeCrawl.World;

namespace YLHalf
{
    public class GriddedEntity : AbstractGriddedEntity<GridEntity, bool> {
        protected override ILevel<GridEntity, bool> level => Level.instance;
    }
}