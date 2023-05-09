using DeCrawl.World;

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
}
