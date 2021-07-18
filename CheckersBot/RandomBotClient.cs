using CheckersLib;
using System;
using System.Collections.Generic;

namespace CheckersBot
{
    class RandomBotClient : BotClient
    {
        public RandomBotClient(int moveDelay, ulong seed) : base(moveDelay, seed)
        {
        }

        protected override Move GetMove()
        {
            // Проверка, можно ли совершить взятие
            bool canCapture = board.CanPlayerCapture(Color);
            // Все возможные ходы
            var allMoves = new List<Move>();
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                    allMoves.AddRange(board.GetMoves(Color, (row, col), canCapture));
            if (allMoves.Count == 0)
                return null;
            // Случайный ход
            return allMoves[(int)(random.NextUInt32() % (uint)allMoves.Count)];
        }
    }
}
