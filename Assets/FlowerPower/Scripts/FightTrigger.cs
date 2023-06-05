using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DeCrawl.Systems;
using DeCrawl.Primitives;

namespace FP
{
    public class FightTrigger : MonoBehaviour, IPhased
    {
        [SerializeField]
        CardinalDirection forceDirection;

        [SerializeField]
        string fightScene;

        [SerializeField]
        bool allowRepeatTriggering = false;

        [SerializeField]
        Sprite InterlocutorSprite;

        [SerializeField]
        string Interlocutor;

        [SerializeField]
        Interaction.TextPrompt[] prologue;

        [SerializeField]
        string FightWord;

        [SerializeField]
        Interaction.TextPrompt[] epilogue;

        [SerializeField]
        int InterlocutorHealth = 100;

        bool triggered = false;

        public string Id => Interlocutor;

        public event PhaseChangeEvent OnPhaseChange;

        private void OnEnable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        private void OnDisable()
        {
            PlayerController.OnPlayerMove += PlayerController_OnPlayerMove;
        }

        void LoadInteractionIfNeeded()
        {
            if (Interaction.instance == null)
            {
                Game.Status = GameStatus.FightScene;
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
                SceneManager.LoadScene(fightScene, LoadSceneMode.Additive);
            } else
            {
                ConfigInteraction();
            }
        }

        void ConfigInteraction()
        {
            var interaction = Interaction.instance;
            interaction.Configure(InterlocutorSprite, prologue, epilogue);
            var spellWordFight = interaction.GetComponent<MiniGameSpellWord>();
            spellWordFight.Configure(Interlocutor, FightWord, InterlocutorHealth);

            Game.Status = GameStatus.FightScene;

        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name != fightScene) return;
            
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            ConfigInteraction();
        }



        private void PlayerController_OnPlayerMove(Vector3Int position, DeCrawl.Primitives.CardinalDirection lookDirection)
        {
            if (triggered) return;
            if (GetComponent<LevelNode>().Coordinates != position) return;

            if (forceDirection != CardinalDirection.Invalid && lookDirection != forceDirection)
            {
                PlayerController.instance.Teleport(position, forceDirection);
            }
            
            LoadInteractionIfNeeded();            

            if (!allowRepeatTriggering) {
                triggered = true;
                OnPhaseChange?.Invoke(Id, "triggered");
            }
        }

        public void RestorePhase(string phase)
        {
            triggered = phase == "triggered";
        }
    }
}