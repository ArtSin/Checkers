using CheckersLib;
using System;
using System.Collections.Generic;

namespace CheckersBot
{
    class AlphaBetaBotClient : BotClient
    {
        // Бесконечность
        private const int INF = 1000000000;
        // Максимальная глубина
        private int maxDepth;

        public AlphaBetaBotClient(int moveDelay, ulong seed, int maxDepth) : base(moveDelay, seed)
        {
            this.maxDepth = maxDepth;
        }

        protected override Move GetMove()
        {
            Move maxMove = null;
            AlphaBetaPruning(Color, maxDepth, -INF, INF, ref maxMove);
            return maxMove;
        }

        private int AlphaBetaPruning(PlayerColor currPlayer, int depth, int alpha, int beta, ref Move maxMove)
        {
            // Достигнута максимальная глубина поиска
            if (depth == 0)
                return EvalPieceRow(currPlayer);

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
                int res = -AlphaBetaPruning(1 - currPlayer, depth - 1, -beta, -alpha, ref maxMove);
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

                alpha = Math.Max(alpha, res);
                if (alpha >= beta)
                    break;
            }
            return result;
        }

        // Оценка позиции
        private int EvalPieceRow(PlayerColor currPlayer)
        {
            int result = 0;
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    int res = 0;
                    // Обычная шашка: 5 + количество рядов от начала доски до шашки
                    if (board.PiecesType[row, col] == PieceType.Normal)
                        res = 5 + ((board.PiecesColor[row, col] == PlayerColor.White) ? row : (Board.SIZE - 1 - row));
                    // Дамка: 15
                    else if (board.PiecesType[row, col] == PieceType.King)
                        res = 7 + Board.SIZE;
                    // Разность сумм значений шашек текущего игрока и противника
                    result += board.PiecesColor[row, col] == currPlayer ? res : -res;
                }
            return (result * 256) + random.NextByte();
        }
    }
}
