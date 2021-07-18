using CheckersLib;
using System;
using System.Threading;

namespace CheckersBot
{
    class Program
    {
        // Событие остановки
        private static AutoResetEvent stopEvent = new AutoResetEvent(false);
        // Цвет бота
        private static PlayerColor color;
        // Количество игр
        private static int gameCount;
        // Генератор случайных чисел
        private static Random rand = new Random();

        static void Main(string[] args)
        {
            // Если нет аргументов запуска
            if (args.Length == 0)
            {
                Console.WriteLine("Аргументы: <адрес> <порт> <имя бота> <тип бота> <цвет бота> <количество игр> <задержка хода> <параметры этого типа бота>");
                Console.WriteLine("Типы ботов: random, miniMaxWeak, miniMax, alphaBeta, negaScout, negaScoutTransposition");
                Console.WriteLine("Цвета: black, white");
                Console.WriteLine("Задержка хода: в миллисекундах");
                Console.WriteLine("Параметры типов ботов:");
                Console.WriteLine("random: <начальное значение ГПСЧ>");
                Console.WriteLine("miniMaxWeak: <начальное значение ГПСЧ> <максимальная глубина>");
                Console.WriteLine("miniMax: <начальное значение ГПСЧ> <максимальная глубина>");
                Console.WriteLine("alphaBeta: <начальное значение ГПСЧ> <максимальная глубина>");
                Console.WriteLine("negaScout: <начальное значение ГПСЧ> <максимальная глубина>");
                Console.WriteLine("negaScoutTransposition: <начальное значение ГПСЧ> <максимальная глубина>");
                return;
            }

            // Хост
            string host = args[0];
            // Порт
            int port = int.Parse(args[1]);
            // Имя бота
            string name = args[2];
            // Тип бота
            string botType = args[3];
            // Цвет бота
            color = args[4] == "white" ? PlayerColor.White : PlayerColor.Black;
            // Количество игр
            gameCount = int.Parse(args[5]);
            // Задержка хода
            int moveDelay = int.Parse(args[6]);
            // Начальное значение генератора случайных чисел
            ulong seed = ulong.Parse(args[7]);

            BotClient client = null;
            var t = new Thread(() =>
            {
                // Создание клиента бота
                switch (botType)
                {
                    case "random":
                        client = new RandomBotClient(moveDelay, seed);
                        break;
                    case "miniMaxWeak":
                        int maxDepth = int.Parse(args[8]);
                        client = new MinimaxWeakBotClient(moveDelay, seed, maxDepth);
                        break;
                    case "miniMax":
                        maxDepth = int.Parse(args[8]);
                        client = new MinimaxBotClient(moveDelay, seed, maxDepth);
                        break;
                    case "alphaBeta":
                        maxDepth = int.Parse(args[8]);
                        client = new AlphaBetaBotClient(moveDelay, seed, maxDepth);
                        break;
                    case "negaScout":
                        maxDepth = int.Parse(args[8]);
                        client = new NegaScoutBotClient(moveDelay, seed, maxDepth);
                        break;
                    case "negaScoutTransposition":
                        maxDepth = int.Parse(args[8]);
                        client = new NegaScoutTranspositionBotClient(moveDelay, seed, maxDepth);
                        break;
                    default:
                        break;
                }

                client.WhiteWon += GameFinished;
                client.BlackWon += GameFinished;
                client.Draw += GameFinished;
                client.GameInterrupted += GameFinished;

                // Подключение к серверу и выбор цвета
                client.Connect(name, host, port);
                if (color == PlayerColor.White)
                    client.SetWhite();
                else
                    client.SetBlack();
            });
            t.Start();
            // Ожидание события остановки
            stopEvent.WaitOne();
            // Отключение от сервера
            client.Disconnect();
        }

        // Закончилась одна игра
        private static void GameFinished(object sender, EventArgs e)
        {
            var client = (BaseClient)sender;
            gameCount--;
            // Сыграно нужное количество игр
            if (gameCount == 0)
            {
                stopEvent.Set();
                return;
            }
            Thread.Sleep(rand.Next(500, 1000));
            // Выбор цвета для новой игры
            if (color == PlayerColor.White)
                client.SetWhite();
            else
                client.SetBlack();
        }
    }
}
