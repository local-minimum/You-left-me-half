using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DeCrawl.Systems;

namespace FP
{
    public class FightTrigger : MonoBehaviour
    {
        [SerializeField]
        string fightScene;

        bool triggered = false;

        private void OnEnable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void OnDisable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void PlayerController_OnPlayerMove(Vector3Int position, DeCrawl.Primitives.CardinalDirection lookDirection)
        {
            if (triggered) return;
            if (GetComponent<LevelNode>().Coordinates != position) return;

            if (Interaction.instance == null)
            {
                SceneManager.LoadScene(fightScene, LoadSceneMode.Additive);
            }

            Game.Status = GameStatus.FightScene;
            triggered = true;
        }
    }
}