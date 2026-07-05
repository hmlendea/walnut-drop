using System;
using WalnutDrop;

namespace WalnutDrop
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using GameRoot game = new();
            game.Run();
        }
    }
}
