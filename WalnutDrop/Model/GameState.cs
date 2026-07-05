using System;
using System.Collections.Generic;

namespace WalnutDrop.Model
{
    public sealed class GameState
    {
        public const float TurnDuration = 30.0f;
        public const float RoundCompletePauseDuration = 3.0f;

        private readonly Random _random;
        private float _roundCompleteTimer;

        public Player Player1 { get; }
        public Player Player2 { get; }
        public Board Board { get; }
        public int CurrentRoundIndex { get; private set; }
        public PlayerIndex CurrentPlayerIndex { get; private set; }
        public GamePhase Phase { get; private set; }
        public float TurnTimeRemaining { get; private set; }
        public bool IsOpponentFinalDrop { get; private set; }
        public PlayerIndex? Winner { get; private set; }

        public RoundDefinition CurrentRound => RoundDefinition.All[CurrentRoundIndex];

        public Player CurrentPlayer
        {
            get
            {
                if (CurrentPlayerIndex.Equals(PlayerIndex.Player1))
                {
                    return Player1;
                }

                return Player2;
            }
        }

        public GameState()
        {
            _random = new Random();
            Player1 = new Player(PlayerIndex.Player1);
            Player2 = new Player(PlayerIndex.Player2);
            Board = new Board();
            CurrentRoundIndex = 0;
            CurrentPlayerIndex = PlayerIndex.Player1;
            Phase = GamePhase.Playing;
            TurnTimeRemaining = TurnDuration;
            IsOpponentFinalDrop = false;
            Winner = null;
        }

        public void Update(float deltaTime)
        {
            if (Phase.Equals(GamePhase.RoundComplete))
            {
                _roundCompleteTimer -= deltaTime;

                if (_roundCompleteTimer <= 0)
                {
                    BeginNextRound();
                }

                return;
            }

            if (!Phase.Equals(GamePhase.Playing))
            {
                return;
            }

            TurnTimeRemaining -= deltaTime;

            if (TurnTimeRemaining <= 0)
            {
                DropCoin(_random.Next(Board.Columns));
            }
        }

        public List<CoinDropResult> DropCoin(int column)
        {
            if (!Phase.Equals(GamePhase.Playing))
            {
                return new List<CoinDropResult>();
            }

            List<CoinDropResult> results = Board.DropCoin(column);

            int scored = 0;

            foreach (CoinDropResult result in results)
            {
                scored += CurrentRound.SlotValues[result.FinalColumn] * result.CoinCount;
            }

            CurrentPlayer.AddScore(scored);
            AfterDrop();

            return results;
        }

        private void AfterDrop()
        {
            if (CurrentPlayer.RoundScore >= CurrentRound.TargetScore && !IsOpponentFinalDrop)
            {
                IsOpponentFinalDrop = true;
                SwitchTurn();
                TurnTimeRemaining = TurnDuration;
                return;
            }

            if (IsOpponentFinalDrop)
            {
                IsOpponentFinalDrop = false;
                TriggerRoundComplete();
                return;
            }

            SwitchTurn();
            TurnTimeRemaining = TurnDuration;
        }

        private void TriggerRoundComplete()
        {
            Phase = GamePhase.RoundComplete;
            _roundCompleteTimer = RoundCompletePauseDuration;
        }

        private void BeginNextRound()
        {
            CurrentRoundIndex++;

            if (CurrentRoundIndex >= RoundDefinition.All.Count)
            {
                Phase = GamePhase.GameOver;
                DetermineWinner();
                return;
            }

            Player1.StartNewRound();
            Player2.StartNewRound();

            Phase = GamePhase.Playing;
            TurnTimeRemaining = TurnDuration;
        }

        private void SwitchTurn()
        {
            if (CurrentPlayerIndex.Equals(PlayerIndex.Player1))
            {
                CurrentPlayerIndex = PlayerIndex.Player2;
            }
            else
            {
                CurrentPlayerIndex = PlayerIndex.Player1;
            }
        }

        private void DetermineWinner()
        {
            if (Player1.TotalScore > Player2.TotalScore)
            {
                Winner = PlayerIndex.Player1;
            }
            else if (Player2.TotalScore > Player1.TotalScore)
            {
                Winner = PlayerIndex.Player2;
            }
            else
            {
                Winner = null;
            }
        }
    }
}
