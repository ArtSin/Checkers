using CheckersLib;
using System.Collections.Generic;

namespace CheckersBot
{
    class MinimaxWeakBotClient : BotClient
    {
        // Бесконечность
        private const int INF = 1000000000;
        // Максимальная глубина
        private int maxDepth;

        public MinimaxWeakBotClient(int moveDelay, ulong seed, int maxDepth) : base(moveDelay, seed)
        {
            this.maxDepth = maxDepth;
        }

        protected override Move GetMove()
        {
            Move maxMove = null;
            Minimax(Color, maxDepth, ref maxMove);
            return maxMove;
        }

        private int Minimax(PlayerColor currPlayer, int depth, ref Move maxMove)
        {
            // Достигнута максимальная глубина поиска
            if (depth == 0)
                return EvalPieceCount(currPlayer);
            
            // Проверка, можно ли совершить взятие
            bool canCapture = board.CanPlayerCapture(currPlayer);
            // Все возможные ходы
            var allMoves = new List<Move>();
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                    allMoves.AddRange(board.GetMoves(currPlayer, (row, col), canCapture));
            // Нет ходов, проигрышная позиция
            if (allMoves.Count == 0)
                return -INF + 1;

            // Наименьший возможный результат
            int result = -INF;
            // Перебор возможных ходов
            foreach (var move in allMoves)
            {
                // Применение хода
                board.DoMove(move);
                // Рекурсивный поиск (для другого игрока)
                int res = -Minimax(1 - currPlayer, depth - 1, ref maxMove);
                if (res > result)
                {
                    // Обновление результата максимумом
                    result = res;
                    // Обновление лучшего хода
                    if (depth == maxDepth)
                        maxMove = move;
                }
                // Отмена хода
                board.UndoMove(move);
            }
            return result;
        }

        // Оценка позиции
        private int EvalPieceCount(PlayerColor currPlayer)
        {
            int result = 0;
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    if (board.PiecesType[row, col] == PieceType.Empty)
                        continue;
                    // Обычная шашка: 1, дамка: 2
                    int res = (board.PiecesType[row, col] == PieceType.Normal) ? 1 : 2;
                    // Разность сумм значений шашек текущего игрока и противника
                    result += board.PiecesColor[row, col] == currPlayer ? res : -res;
                }
            return (result * 256) + random.NextByte();
        }
    }
}
