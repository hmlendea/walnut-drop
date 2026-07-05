using System;
using System.Collections.Generic;

namespace WalnutDrop.Model
{
    public sealed class Board
    {
        public const int Columns = 8;
        public const int SwitchRowCount = 5;

        private readonly GameSwitch[,] _switchGrid;
        private readonly List<GameSwitch> _allSwitches;
        private readonly Random _random;

        public Board()
        {
            _switchGrid = new GameSwitch[SwitchRowCount, Columns];
            _allSwitches = [];
            _random = new Random();
            InitializeSwitches();
        }

        public GameSwitch GetSwitch(int switchRow, int column)
        {
            if (switchRow < 0 || switchRow >= SwitchRowCount)
            {
                return null;
            }

            if (column < 0 || column >= Columns)
            {
                return null;
            }

            return _switchGrid[switchRow, column];
        }

        public List<CoinDropResult> DropCoin(int startColumn)
        {
            List<CoinDropResult> results = [];
            Queue<PendingCoin> pending = new();
            pending.Enqueue(new PendingCoin(startColumn, 1, 0));

            while (pending.Count > 0)
            {
                PendingCoin item = pending.Dequeue();
                SimulateFall(item.Column, item.CoinCount, item.FromRow, results, pending);
            }

            return results;
        }

        public void ResetPadCoins()
        {
            foreach (GameSwitch sw in _allSwitches)
            {
                sw.PadCoinCount = 0;
            }
        }

        public IEnumerable<(int column, int row)> TracePrimaryPath(int startColumn)
        {
            List<(int column, int row)> path = [];
            int column = startColumn;

            path.Add((column, -1));

            for (int row = 0; row < SwitchRowCount; row++)
            {
                GameSwitch sw = _switchGrid[row, column];

                if (sw is null)
                {
                    continue;
                }

                if (sw.IsPadColumn(column))
                {
                    if (sw.PadCoinCount.Equals(0))
                    {
                        path.Add((column, row));
                        return path;
                    }

                    path.Add((column, row));
                    column = sw.LeverColumn;
                    path.Add((column, row));
                }
                else
                {
                    path.Add((column, row));
                    column = sw.PadColumn;
                    path.Add((column, row));
                }
            }

            path.Add((column, SwitchRowCount));
            return path;
        }

        private void InitializeSwitches()
        {
            for (int row = 0; row < SwitchRowCount; row++)
            {
                int startCol;

                if ((row % 2).Equals(0))
                {
                    startCol = 0;
                }
                else
                {
                    startCol = 1;
                }

                for (int col = startCol; col < Columns - 1; col += 2)
                {
                    SwitchSide side;

                    if (_random.Next(2).Equals(0))
                    {
                        side = SwitchSide.Left;
                    }
                    else
                    {
                        side = SwitchSide.Right;
                    }

                    GameSwitch sw = new(col, col + 1, side);
                    _allSwitches.Add(sw);
                    _switchGrid[row, col] = sw;
                    _switchGrid[row, col + 1] = sw;
                }
            }
        }

        private void SimulateFall(
            int column,
            int coinCount,
            int fromRow,
            List<CoinDropResult> results,
            Queue<PendingCoin> pending)
        {
            for (int row = fromRow; row < SwitchRowCount; row++)
            {
                GameSwitch sw = _switchGrid[row, column];

                if (sw == null)
                {
                    continue;
                }

                if (sw.IsPadColumn(column))
                {
                    if (sw.PadCoinCount.Equals(0) && coinCount.Equals(1))
                    {
                        sw.PadCoinCount = 1;
                        return;
                    }
                    else if (sw.PadCoinCount.Equals(0) && coinCount > 1)
                    {
                        // Multi-coin on empty pad: 1 rests, rest cross to lever side
                        sw.PadCoinCount = 1;
                        int crossingCount = coinCount - 1;
                        int leverCol = sw.LeverColumn;
                        int originalPadCol = column;

                        for (int i = 0; i < crossingCount; i++)
                        {
                            sw.Flip();
                        }

                        if ((crossingCount % 2).Equals(1))
                        {
                            // Odd flips: the resting coin launches from the original pad column
                            sw.PadCoinCount = 0;
                            pending.Enqueue(new PendingCoin(originalPadCol, 1, row + 1));
                        }

                        column = leverCol;
                        coinCount = crossingCount;
                    }
                    else
                    {
                        // Falling coin hits occupied pad: all merge and bounce to lever side
                        int mergedCount = coinCount + sw.PadCoinCount;
                        int leverCol = sw.LeverColumn;
                        sw.PadCoinCount = 0;

                        for (int i = 0; i < mergedCount; i++)
                        {
                            sw.Flip();
                        }

                        column = leverCol;
                        coinCount = mergedCount;
                    }
                }
                else
                {
                    // Coin arrives at lever side: crosses to pad column, flipping the switch
                    int padCol = sw.PadColumn;
                    int padCoinCount = sw.PadCoinCount;

                    for (int i = 0; i < coinCount; i++)
                    {
                        sw.Flip();
                    }

                    if (padCoinCount > 0 && (coinCount % 2).Equals(1))
                    {
                        // Odd flips: the coin resting on the pad gets launched
                        sw.PadCoinCount = 0;
                        pending.Enqueue(new PendingCoin(padCol, padCoinCount, row + 1));
                    }

                    column = padCol;
                }
            }

            results.Add(new CoinDropResult(column, coinCount));
        }
    }
}
