namespace WalnutDrop.Model
{
    public sealed class CoinDropResult(
        int finalColumn,
        int coinCount)
    {
        public int FinalColumn { get; } = finalColumn;
        public int CoinCount { get; } = coinCount;
    }
}
