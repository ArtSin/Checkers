using CheckersLib;
using System;
using WebSocketSharp.NetCore;
using WebSocketSharp.NetCore.Server;

namespace CheckersServer
{
    public enum PlayerRole { Spectator, Black, White };

    public class ServerClient : WebSocketBehavior
    {
        // Клиент подключается
        public event EventHandler<JoinEventArgs> Join;
        // Клиент становится белым игроком
        public event EventHandler SetWhite;
        // Клиент становится чёрным игроком
        public event EventHandler SetBlack;
        // Клиент делает ход
        public event EventHandler<MoveEventArgs> MakeMove;

        // Имя пользователя
        public string UserName { get; set; }
        public PlayerRole Role { get; set; }

        protected override void OnOpen()
        {
            Server.GetInstance().AddClient(this);
        }

        // Получение сообщений
        protected override void OnMessage(MessageEventArgs e)
        {
            // Позиция переноса строки
            int pos = e.Data.IndexOf('\n');
            // Заголовок сообщения
            var messageHeader = e.Data.Substring(0, pos);
            // Тело сообщения
            var messageBody = e.Data.Substring(pos + 1);
            // Обработка сообщения
            switch (messageHeader)
            {
                // Клиент подключается
                case "Join":
                    Join?.Invoke(this, new JoinEventArgs(messageBody));
                    break;
                // Клиент становится белым игроком
                case "SetWhite":
                    SetWhite?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент становится чёрным игроком
                case "SetBlack":
                    SetBlack?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент делает ход
                case "MakeMove":
                    MakeMove?.Invoke(this, new MoveEventArgs(new Move(messageBody)));
                    break;
                default:
                    break;
            }
        }

        protected override void OnClose(CloseEventArgs e) => Server.GetInstance().RemoveClient(ID);

        public new void Close() => Sessions.CloseSession(ID);
    }
}