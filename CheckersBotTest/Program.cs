using System;
using System.Diagnostics;
using System.Threading;

namespace CheckersBotTest
{
    class Program
    {
        private static int whiteWinCnt = 0, blackWinCnt = 0, drawCnt = 0, interruptedCnt = 0;

        static void Main(string[] args)
        {
            // Если нет аргументов запуска
            if (args.Length == 0)
            {
                Console.WriteLine("Аргументы: <количество игр> <тип бота №1> <параметры бота №1> : <тип бота №2> <параметры бота №2>");
                return;
            }

            // Количество игр
            int gameCount = int.Parse(args[0]);
            // Тип первого бота
            string bot1Type = args[1];
            // Параметры первого бота
            string bot1Args = "";
            int index;
            for (index = 2; index < args.Length; index++)
            {
                if (args[index] == ":")
                    break;
                bot1Args += args[index] + " ";
            }
            index++;
            // Тип второго бота
            string bot2Type = args[index];
            // Параметры второго бота
            string bot2Args = "";
            for (index++; index < args.Length; index++)
                bot2Args += args[index] + " ";

            // Запуск сервера
            var serverProcess = new Process();
            serverProcess.StartInfo = new ProcessStartInfo("../../../../CheckersServer/bin/Release/netcoreapp3.1/CheckersServer.exe");
            serverProcess.StartInfo.RedirectStandardInput = true;
            serverProcess.StartInfo.RedirectStandardOutput = true;
            serverProcess.StartInfo.RedirectStandardError = true;
            serverProcess.OutputDataReceived += ServerProcessOutputDataReceived;
            serverProcess.ErrorDataReceived += ServerProcessOutputDataReceived;
            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();
            Thread.Sleep(1000);

            // Запуск первого бота
            var bot1Process = new Process();
            bot1Process.StartInfo = new ProcessStartInfo("../../../../CheckersBot/bin/Release/netcoreapp3.1/CheckersBot.exe",
                "127.0.0.1 43210 bot1 " + bot1Type + " white " + gameCount + " 0 " + bot1Args);
            bot1Process.StartInfo.RedirectStandardOutput = true;
            bot1Process.StartInfo.RedirectStandardError = true;
            bot1Process.OutputDataReceived += Bot1ProcessOutputDataReceived;
            bot1Process.ErrorDataReceived += Bot1ProcessOutputDataReceived;
            bot1Process.Start();
            bot1Process.BeginOutputReadLine();
            bot1Process.BeginErrorReadLine();
            Thread.Sleep(1000);

            // Запуск второго бота
            var bot2Process = new Process();
            bot2Process.StartInfo = new ProcessStartInfo("../../../../CheckersBot/bin/Release/netcoreapp3.1/CheckersBot.exe",
                "127.0.0.1 43210 bot2 " + bot2Type + " black " + gameCount + " 0 " + bot2Args);
            bot2Process.StartInfo.RedirectStandardOutput = true;
            bot2Process.StartInfo.RedirectStandardError = true;
            bot2Process.OutputDataReceived += Bot2ProcessOutputDataReceived;
            bot2Process.ErrorDataReceived += Bot2ProcessOutputDataReceived;
            bot2Process.Start();
            bot2Process.BeginOutputReadLine();
            bot2Process.BeginErrorReadLine();
            Thread.Sleep(1000);

            bot1Process.WaitForExit();
            bot2Process.WaitForExit();
            serverProcess.StandardInput.WriteLine();
            serverProcess.WaitForExit();

            Console.WriteLine($"Первый:второй:ничья:прервано {whiteWinCnt}:{blackWinCnt}:{drawCnt}:{interruptedCnt}");
            Console.WriteLine($"Счёт {whiteWinCnt - blackWinCnt}");
        }

        // Вывод сервера
        private static void ServerProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("[Server] " + e.Data);
        }

        // Вывод первого бота
        private static void Bot1ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("[Bot1] " + e.Data);

            switch (e.Data)
            {
                case "whiteWin":
                    whiteWinCnt++;
                    break;
                case "blackWin":
                    blackWinCnt++;
                    break;
                case "draw":
                    drawCnt++;
                    break;
                case "interrupted":
                    interruptedCnt++;
                    break;
            }
        }

        // Вывод второго бота
        private static void Bot2ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("[Bot2] " + e.Data);
        }
    }
}
