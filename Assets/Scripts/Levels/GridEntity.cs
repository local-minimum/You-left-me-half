public enum GridEntity
{        
    None,
    [StringValue("Pp")]
    Player,
    [StringValue("Ss")]
    PlayerSpawn,
    [StringValue("Oo")]
    Other,
    [StringValue("Xx")]
    InBound,
    [StringValue("-")]
    OutBound,
    [StringValue("Vv")]
    VirtualSpace,
}

public static class GridEntityExtensions
{
    public static GridEntity ToGridEntity(this char ch)
    {
        switch (ch)
        {
            case 'p':
            case 'P':
                return GridEntity.Player;
            case 's':
            case 'S':
                return GridEntity.PlayerSpawn;
            case 'O':
            case 'o':
                return GridEntity.Other;
            case 'X':
            case 'x':
                return GridEntity.InBound;
            case 'V':
            case 'v':
                return GridEntity.VirtualSpace;
            default:
                return GridEntity.OutBound;
        }
    }

    public static bool IsClaimable(this GridEntity entity, bool allowVirtual = false) =>
        entity == GridEntity.InBound || entity == GridEntity.PlayerSpawn || (allowVirtual && entity == GridEntity.VirtualSpace);

    public static bool IsInbound(this GridEntity entity, bool allowVirtual = false) =>
        entity != GridEntity.OutBound && (allowVirtual || entity != GridEntity.VirtualSpace);

    public static bool IsOccupied(this GridEntity entity) =>
        entity == GridEntity.Player || entity == GridEntity.Other;

    public static bool IsBaseType(this GridEntity entity) =>
        entity == GridEntity.OutBound || entity == GridEntity.InBound || entity == GridEntity.VirtualSpace;
}
