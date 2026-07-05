namespace WalnutDrop.Model
{
    public sealed class Player
    {
        public PlayerIndex Index { get; }
        public int RoundScore { get; set; }
        public int TotalScore { get; private set; }

        public Player(PlayerIndex index)
        {
            Index = index;
            RoundScore = 0;
            TotalScore = 0;
        }

        public void AddScore(int points)
        {
            RoundScore += points;
            TotalScore += points;
        }

        public void StartNewRound()
        {
            RoundScore = 0;
        }
    }
}
