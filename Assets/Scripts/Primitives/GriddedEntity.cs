using DeCrawl.World;

namespace YLHalf
{
    public class GriddedEntity : AbstractGriddedEntity<GridEntity, bool> {
        protected override IGrid grid => Level.instance;
    }
}