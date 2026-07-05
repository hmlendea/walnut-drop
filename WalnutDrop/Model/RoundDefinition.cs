using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WalnutDrop.Model
{
    public sealed class RoundDefinition
    {
        public static readonly ReadOnlyCollection<RoundDefinition> All;

        public int RoundNumber { get; }
        public int TargetScore { get; }
        public int[] SlotValues { get; }

        static RoundDefinition()
        {
            List<RoundDefinition> rounds = new List<RoundDefinition>
            {
                new RoundDefinition(1, 10,  new int[] { 2,  2,  2,  2,  2,  2,  2,  2  }),
                new RoundDefinition(2, 40,  new int[] { 21, 13, 8,  5,  5,  8,  13, 21 }),
                new RoundDefinition(3, 20,  new int[] { 8,  6,  4,  2,  2,  4,  6,  8  }),
                new RoundDefinition(4, 80,  new int[] { 64, 49, 36, 25, 25, 36, 49, 64 })
            };

            All = rounds.AsReadOnly();
        }

        private RoundDefinition(int roundNumber, int targetScore, int[] slotValues)
        {
            RoundNumber = roundNumber;
            TargetScore = targetScore;
            SlotValues = slotValues;
        }
    }
}
