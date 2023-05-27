using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class GameStateAsSpawn : MonoBehaviour
    {
        [SerializeField]
        GameStatus status;

        [SerializeField]
        UIMenuSystem.State MenuState;

        [SerializeField]
        UIMenuSystem MenuSystem;

        private void Start()
        {
            if (status != GameStatus.Unknown)
            {
                Game.Status = status;
            }

            if (MenuSystem)
            {
                MenuSystem.state = MenuState;
            }
        }
    }
}
