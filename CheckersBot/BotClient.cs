using CheckersLib;
using Haus.Math;
using System;
using System.Diagnostics;
using System.Threading;

namespace CheckersBot
{
    abstract class BotClient : BaseClient
    {
        // Задержка хода
        private int moveDelay;
        // Начальное значение генератора случайных чисел
        private ulong seed;
        // Генератор случайных чисел
        protected XorShiftRandom random;
        // Сумма времени ходов
        private long moveTimeSum = 0;
        // Количество ходов
        private int moveCnt = 0;
        // Максимальное время хода
        private long maxMoveTime = 0;
        // Доска
        protected Board board;

        public BotClient(int moveDelay, ulong seed)
        {
            this.moveDelay = moveDelay;
            this.seed = seed;
            random = new XorShiftRandom(seed);

            BoardReceived += OnBoardReceived;
            ThisPlayerMoveRequested += OnThisPlayerMoveRequested;
            WhiteWon += OnWhiteWon;
            BlackWon += OnBlackWon;
            Draw += OnDraw;
            GameInterrupted += OnGameInterrupted;

            WhiteWon += OnGameFinished;
            BlackWon += OnGameFinished;
            Draw += OnGameFinished;
            GameInterrupted += OnGameFinished;
        }

        // Белый игрок выиграл
        private void OnWhiteWon(object sender, EventArgs e) => Console.WriteLine("whiteWin");

        // Чёрный игрок выиграл
        private void OnBlackWon(object sender, EventArgs e) => Console.WriteLine("blackWin");

        // Ничья
        private void OnDraw(object sender, EventArgs e) => Console.WriteLine("draw");

        // Игра прервана
        private void OnGameInterrupted(object sender, EventArgs e) => Console.WriteLine("interrupted");

        // Игра завершена
        private void OnGameFinished(object sender, EventArgs e)
        {
            Console.WriteLine($"Среднее время: {(double)moveTimeSum / moveCnt} мс Максимальное время: {maxMoveTime} мс");
            random = new XorShiftRandom(seed);
        }

        // Получена доска
        private void OnBoardReceived(object sender, BoardEventArgs e) => board = e.Board;

        // Запрошен ход этого игрока
        private void OnThisPlayerMoveRequested(object sender, EventArgs e)
        {
            // Задержка перед ходом
            Thread.Sleep(moveDelay);
            var watch = Stopwatch.StartNew();
            // Совершение хода
            var move = GetMove();
            watch.Stop();

            Console.WriteLine($"Время: {watch.ElapsedMilliseconds} мс");
            var moveTime = watch.ElapsedMilliseconds;
            maxMoveTime = Math.Max(maxMoveTime, moveTime);
            moveTimeSum += moveTime;
            moveCnt++;

            if (move != null)
                MakeMove(move);
        }

        // Ход бота
        protected abstract Move GetMove();
    }
}
