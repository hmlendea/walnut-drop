namespace WalnutDrop.Model
{
    internal sealed class PendingCoin(
        int column,
        int coinCount,
        int fromRow)
    {
        public int Column { get; set; } = column;
        public int CoinCount { get; set; } = coinCount;
        public int FromRow { get; set; } = fromRow;
    }
}
