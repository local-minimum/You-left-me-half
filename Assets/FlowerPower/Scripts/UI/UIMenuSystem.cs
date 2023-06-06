using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Systems;

namespace FP
{
    public class UIMenuSystem : MonoBehaviour
    {
        [SerializeField]
        bool PauseGameWhenVisible = true;

        bool synced;

        public enum State { Hidden, Main, About, Settings, Inventory, Save, Load };

        private State _state;

        private List<IUIMenuView> _Options;

        List<IUIMenuView> Options
        {
            get
            {
                if (_Options == null)
                {
                    _Options = new List<IUIMenuView>();
                    _Options.AddRange(GetComponentsInChildren<IUIMenuView>(true));
                }
                return _Options;
            }
        }

        public State state
        {
            get => _state;
            set
            {
                _state = value;
                bool visible = value != State.Hidden;
                ToggleChildrenVisibility(visible, value);
                HandleGameState(visible);
                synced = true;
                OnChangeState?.Invoke(value);
            }
        }

        void ToggleChildrenVisibility(bool visible, State state)
        {
            for (int i = 0, l = transform.childCount; i < l; i++)
            {
                transform.GetChild(i).gameObject.SetActive(visible);
            }

            foreach (var view in Options)
            {
                view.Active = view.State == state;
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

        private void Start()
        {
            if (!synced)
            {
                state = state;
            }
        }
    }

}