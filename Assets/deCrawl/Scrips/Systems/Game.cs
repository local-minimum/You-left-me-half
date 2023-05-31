using UnityEngine;

namespace DeCrawl.Systems
{
    public enum GameStatus { Unknown, Playing, CutScene, Paused, FightScene, GameOver, Loading };

    public delegate void GameStatusChangeEvent(GameStatus status, GameStatus oldStatus);
    public static class Game
    {
        public static event GameStatusChangeEvent OnChangeStatus;

        private static GameStatus _status = GameStatus.Unknown;

        private static GameStatus _previousStatus = GameStatus.Unknown;

        public static GameStatus Status
        {
            get
            {
                return _status;
            }

            set
            {
                Debug.Log($"Game Status {_status} => {value}");
                OnChangeStatus?.Invoke(value, _status);
                if (value != _status)
                {
                    _previousStatus = _status;
                }
                _status = value;
            }
        }

        public static void RevertStatus()
        {
            if (_previousStatus == GameStatus.Unknown) return;
            Status = _previousStatus;
            _previousStatus = GameStatus.Unknown;
        }
    }
}