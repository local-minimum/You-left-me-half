using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.World;
using DeCrawl.Primitives;

namespace YLHalf
{
    public class Button : WorldClickable, IPhased
    {
        [SerializeField]
        Transform GridSwapPosition;

        [SerializeField]
        GameObject door;

        [SerializeField]
        GridEntity openState = GridEntity.InBound;

        public event PhaseChangeEvent OnPhaseChange;

        private void Start()
        {
            UpdateDoor(GridPosition);
        }

        protected override bool PreClickCheckRefusal() => false;

        // TODO: Require player inventory keys, where needed
        protected override bool RefuseClick() => false;

        protected override void OnClick()
        {
            UpdateDoor(door.activeSelf);
        }

        Vector3Int GridPosition
        {
            get => Level.instance.AsGridPosition(GridSwapPosition.position);
        }

        [SerializeField]
        private string _Id;
        public string Id => _Id;

        void UpdateDoor(bool toOpen)
        {
            var gridPosition = GridPosition;

            if (toOpen)
            {
                if (!Level.instance.ClaimPositionForced(openState, gridPosition))
                {
                    Debug.Log($"Could not open door {name}");
                    return;
                }
            }
            else
            {
                if (!Level.instance.ReleasePosition(openState, gridPosition))
                {
                    Debug.Log($"Could not close door {name}");
                    return;
                }
                
            }

            UpdateDoor(gridPosition);
        }

        void UpdateDoor(Vector3Int gridPosition)
        {
            var status = Level.instance.GridStatus(gridPosition.x, gridPosition.z);
            bool closed = status == GridEntity.OutBound || status == GridEntity.VirtualSpace;
            door.SetActive(closed);
            OnPhaseChange?.Invoke(Id, closed ? "closed" : "open");
        }

        public void RestorePhase(string phase)
        {
            Debug.Log($"Restore phase: {phase}");

            if (phase == "open")
            {
                UpdateDoor(true);
            } else if (phase == "closed")
            {
                UpdateDoor(false);
            } else
            {
                Debug.LogWarning($"{Id} does not know how to handle phase {phase}");
            }
        }
    }
}