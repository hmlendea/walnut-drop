namespace WalnutDrop.Model
{
    internal sealed class PendingCoin
    {
        public int Column { get; set; }
        public int CoinCount { get; set; }
        public int FromRow { get; set; }

        public PendingCoin(int column, int coinCount, int fromRow)
        {
            Column = column;
            CoinCount = coinCount;
            FromRow = fromRow;
        }
    }
}
