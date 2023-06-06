using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class StartGameOnLoad : MonoBehaviour
    {
        [SerializeField]
        string MetadataRegion;

        void Start()
        {
            Game.Status = GameStatus.Playing;

            if (!string.IsNullOrEmpty(MetadataRegion))
            {
                MetadataRecorder.instance.Region = MetadataRegion;
            }
        }
    }
}