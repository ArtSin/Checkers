using System;
using System.Net;
using WebSocketSharp.NetCore.Server;

namespace CheckersServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(IPAddress.Any, 43210);
            wssv.AddWebSocketService<ServerClient>("/");
            wssv.Start();
            Server.GetInstance().SetSessions(wssv.WebSocketServices["/"].Sessions);
            if (wssv.IsListening)
                Console.WriteLine($"Сервер запущен на {wssv.Address}:{wssv.Port}");
            Console.WriteLine("Нажмите любую клавишу для остановки сервера...");
            Console.Read();
            wssv.Stop();
        }
    }
}
