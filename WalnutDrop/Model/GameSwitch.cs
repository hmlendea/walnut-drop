namespace WalnutDrop.Model
{
    public sealed class GameSwitch(
        int leftColumn,
        int rightColumn,
        SwitchSide initialPadSide)
    {
        public int LeftColumn { get; } = leftColumn;
        public int RightColumn { get; } = rightColumn;
        public SwitchSide PadSide { get; private set; } = initialPadSide;
        public int PadCoinCount { get; set; } = 0;

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
