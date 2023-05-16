using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Lootables;

namespace DeCrawl.UI
{
    public class CanisterUI : ProgressionIcon
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