namespace WalnutDrop.Model
{
    public sealed class GameSwitch
    {
        public int LeftColumn { get; }
        public int RightColumn { get; }
        public SwitchSide PadSide { get; private set; }
        public int PadCoinCount { get; set; }

        public int PadColumn
        {
            get
            {
                if (PadSide.Equals(SwitchSide.Left))
                {
                    return LeftColumn;
                }

                return RightColumn;
            }
        }

        public int LeverColumn
        {
            get
            {
                if (PadSide.Equals(SwitchSide.Left))
                {
                    return RightColumn;
                }

                return LeftColumn;
            }
        }

        public GameSwitch(int leftColumn, int rightColumn, SwitchSide initialPadSide)
        {
            LeftColumn = leftColumn;
            RightColumn = rightColumn;
            PadSide = initialPadSide;
            PadCoinCount = 0;
        }

        public bool IsPadColumn(int column) => column.Equals(PadColumn);

        public bool IsLeverColumn(int column) => column.Equals(LeverColumn);

        public void Flip()
        {
            if (PadSide.Equals(SwitchSide.Left))
            {
                PadSide = SwitchSide.Right;
            }
            else
            {
                PadSide = SwitchSide.Left;
            }
        }
    }
}
