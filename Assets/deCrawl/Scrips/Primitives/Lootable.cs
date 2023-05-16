using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Systems;

namespace DeCrawl.Primitives
{
    public class LootEventArgs : System.EventArgs
    {
        public LootOwner Owner { get; private set; }
        public Vector3Int Coordinates { get; set; }
        public bool DefinedPosition { get; set; }
        public bool Consumed { get; set; }

        public LootEventArgs(LootOwner owner)
        {
            Owner = owner;
            DefinedPosition = false;
        }

        public LootEventArgs(LootOwner owner, Vector3Int coordinates)
        {
            Owner = owner;
            DefinedPosition = true;
            Coordinates = coordinates;
        }
    }

    public delegate void LootEvent(Lootable loot, LootEventArgs args);
    public delegate void ManifestChangeEvent(bool visible);

    public enum LootOwner { None, Player, Level };

    public class Lootable : IdentifiableEntity
    {
        public static event LootEvent OnLoot;
        public event ManifestChangeEvent OnManifestChange;

        public Vector3Int Coordinates { get; set; }
        public LootOwner Owner { get; set; }

        public CardinalDirection ManifestSide { get; set; }

        [SerializeField]
        public Vector2Int[] InventoryShape = new Vector2Int[] { Vector2Int.zero };

        public int InventorySize => InventoryShape.Length;

        [SerializeField]
        bool ShouldBeVisibleInInventory = true;

        public IEnumerable<Vector2Int> InventorySlots() => InventorySlots(Coordinates);

        public IEnumerable<Vector2Int> InventorySlots(Vector3Int placement)
        {
            return InventoryShape
                    .Select(coords => new Vector2Int(coords.x + placement.x, coords.y + placement.y));
        }

        public RectInt UIShape
        {
            get
            {
                var minX = InventoryShape.Min(v => v.x);
                var minY = InventoryShape.Min(v => v.y);
                var maxX = InventoryShape.Max(v => v.x);
                var maxY = InventoryShape.Max(v => v.y);
                return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
        }

        [SerializeField]
        public Texture2D texture;

        private bool Loot(LootEventArgs args)
        {
            OnLoot?.Invoke(this, args);
            if (args.Consumed)
            {
                OnManifestChange?.Invoke(args.Owner == LootOwner.Level);
                Owner = args.Owner;
                Coordinates = args.Coordinates;
            }
            else if (args.Owner == LootOwner.Player)
            {
                // If player let level know it's their responsibility and has to
                // figure out where to place it by its own rules (probably track player).
                Loot(LootOwner.Level);
            }
            else
            {
                // If level can't hold it, it goes back to the loot table
                Loot(LootOwner.None);
            }
            return args.Consumed;
        }

        public bool Loot(LootOwner owner) => Loot(new LootEventArgs(owner));
        public bool Loot(LootOwner owner, Vector3Int coordnates) => Loot(new LootEventArgs(owner, coordnates));

        private void Awake()
        {
            if (InventoryShape.Length == 0)
            {
                if (ShouldBeVisibleInInventory)
                {
                    Debug.LogWarning($"{Id} has no inventory shape so it will forever be picked up and invisible");
                }
            }
            else if (InventoryShape.Length > 0 && !ShouldBeVisibleInInventory)
            {
                Debug.LogWarning($"{Id} has inventory shape but should be invisible");
            }
            else if (InventoryShape.Min(coords => coords.y) != 0)
            {
                Debug.LogError($"{Id} inventory shape does not have a 0 y-offset or has negative y-offsets");
            }
            else if (InventoryShape.GroupBy(choords => choords).Count() != InventoryShape.Length)
            {
                Debug.LogWarning($"{Id} inventory shape has duplicate coordinates");
            }

        }

        private void OnEnable()
        {
            Game.OnChangeStatus += Game_OnChangeStatus;
        }

        private void OnDisable()
        {
            Game.OnChangeStatus -= Game_OnChangeStatus;
        }

        private void Game_OnChangeStatus(GameStatus status, GameStatus oldStatus)
        {
            switch (status)
            {
                case GameStatus.CutScene:
                case GameStatus.Paused:
                case GameStatus.GameOver:
                    enabled = false;
                    return;
                case GameStatus.Playing:
                    enabled = true;
                    return;
            }
        }
    }
}