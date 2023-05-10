using DeCrawl.Utils;
using DeCrawl.World;

public class LootableManifestation : WorldClickable
{
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

    protected override bool RefuseClick() =>
        lootable.Owner != LootOwner.Level ||
        (PlayerController.instance.Position - lootable.Coordinates).CheckerMagnitude() > 1;

    protected override void OnClick()
    {
        lootable.Loot(LootOwner.Player);
    }
}
