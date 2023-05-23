using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FP
{
    public interface IFight
    {
        public void DisableContent();

        public void InitiateFight();

        public void RewardPlayer();
    }
}
