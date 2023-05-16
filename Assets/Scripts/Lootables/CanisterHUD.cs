using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.UI;
using DeCrawl.Lootables;

namespace YLHalf
{
    public class CanisterHUD : ProgressionIcon
    {
        public Canister canister;

        private void Start()
        {
            Configure();
        }

        void Configure() => Configure(canister.textureProgress, canister.textureOverlay, canister.fillMethod, canister.ImageFill);

        private void Update()
        {
            progressImage.fillAmount = canister.ImageFill;
        }
    }
}