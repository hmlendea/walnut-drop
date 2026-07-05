namespace WalnutDrop.Model
{
    public sealed class CoinDropResult
    {
        public int FinalColumn { get; }
        public int CoinCount { get; }

        public CoinDropResult(int finalColumn, int coinCount)
        {
            FinalColumn = finalColumn;
            CoinCount = coinCount;
        }
    }
}
