namespace DeCrawl.Systems
{
    public enum GameStatus { Unknown, Playing, CutScene, Paused, GameOver };

    public delegate void GameStatusChangeEvent(GameStatus status, GameStatus oldStatus);
    public static class Game
    {
        public static event GameStatusChangeEvent OnChangeStatus;

        private static GameStatus _status = GameStatus.Unknown;

        public static GameStatus Status
        {
            get
            {
                return _status;
            }

            set
            {
                OnChangeStatus?.Invoke(value, _status);
                _status = value;
            }
        }
    }
}