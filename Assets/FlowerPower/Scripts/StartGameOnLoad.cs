using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class StartGameOnLoad : MonoBehaviour
    {
        void Start()
        {
            Game.Status = GameStatus.Playing;
        }
    }
}