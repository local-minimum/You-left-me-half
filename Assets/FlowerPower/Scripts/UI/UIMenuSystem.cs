using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UIMenuSystem : MonoBehaviour
    {
        [SerializeField]
        bool PauseGameWhenVisible = true;
        public enum State { Hidden, Main, About, KeyBindings, Inventory };

        private State _state;
        public State state
        {
            get => _state;
            set
            {
                _state = value;
                ToggleChildrenVisibility(value != State.Hidden);
                OnChangeState?.Invoke(value);
            }
        }

        void ToggleChildrenVisibility(bool visible)
        {
            for (int i = 0, l = transform.childCount; i < l; i++)
            {
                transform.GetChild(i).gameObject.SetActive(visible);
            }
        }

        GameStatus ResumeStatus = GameStatus.Playing;

        void HandleGameState(bool pausing)
        {
            if (!PauseGameWhenVisible) return;
            if (Game.Status != GameStatus.Paused && pausing)
            {
                ResumeStatus = Game.Status;
                Game.Status = GameStatus.Paused;
            } else if (Game.Status == GameStatus.Paused && !pausing)
            {
                Game.Status = ResumeStatus;
            }
        }

        public delegate void StateChangeEvent(State state);
        public event StateChangeEvent OnChangeState;
    }

}