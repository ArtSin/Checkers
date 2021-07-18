using System;
using WebSocketSharp.NetCore;

namespace CheckersLib
{
    // Состояние клиента
    public enum ClientState { NotConnected, NotParticipating, ThisPlayerMove, OtherPlayerMove };

    public class PlayerListEventArgs
    {
        public string[] Players { get; private set; }

        public PlayerListEventArgs(string[] players)
        {
            Players = players;
        }
    }

    public class MoveEventArgs
    {
        public Move Move { get; private set; }

        public MoveEventArgs(Move move)
        {
            Move = move;
        }
    }

    public class BoardEventArgs
    {
        public Board Board { get; private set; }

        public BoardEventArgs(Board board)
        {
            Board = board;
        }
    }

    public class JoinEventArgs
    {
        public string UserName { get; private set; }

        public JoinEventArgs(string userName)
        {
            UserName = userName;
        }
    }

    public class StateEventArgs
    {
        public ClientState State { get; private set; }

        public StateEventArgs(ClientState state)
        {
            State = state;
        }
    }

    public abstract class BaseClient
    {
        // Ошибка подключения
        public event EventHandler<ErrorEventArgs> WebSocketOnError;
        // Подключение закрыто
        public event EventHandler<CloseEventArgs> WebSocketOnClose;
        // Клиент подключён
        public event EventHandler JoinAccepted;
        // Клиент не подключён
        public event EventHandler JoinRejected;
        // Клиент стал белым игроком
        public event EventHandler SetWhiteAccepted;
        // Клиент не стал белым игроком
        public event EventHandler SetWhiteRejected;
        // Клиент стал чёрным игроком
        public event EventHandler SetBlackAccepted;
        // Клиент не стал чёрным игроком
        public event EventHandler SetBlackRejected;
        // Получен список игроков
        public event EventHandler<PlayerListEventArgs> PlayerListReceived;
        // Получен ход игрока
        public event EventHandler<MoveEventArgs> MoveReceived;
        // Получена доска
        public event EventHandler<BoardEventArgs> BoardReceived;
        // Запрошен ход этого игрока
        public event EventHandler ThisPlayerMoveRequested;
        // Запрошен ход другого игрока
        public event EventHandler OtherPlayerMoveRequested;
        // Белый игрок выиграл
        public event EventHandler WhiteWon;
        // Чёрный игрок выиграл
        public event EventHandler BlackWon;
        // Ничья
        public event EventHandler Draw;
        // Игра прервана
        public event EventHandler GameInterrupted;
        // Изменилось состояние клиента
        public event EventHandler<StateEventArgs> StateChanged;

        WebSocket webSocket;
        string userName;
        // Состояние клиента
        private ClientState _state = ClientState.NotConnected;
        public ClientState State
        {
            get => _state;
            private set
            {
                StateChanged?.Invoke(this, new StateEventArgs(value));
                _state = value;
            }
        }
        // Цвет игрока
        public PlayerColor Color { get; private set; }

        public BaseClient()
        {
            WebSocketOnError += OnWebSocketOnError;
            WebSocketOnClose += OnWebSocketOnClose;
            JoinAccepted += OnJoinAccepted;
            JoinRejected += OnJoinRejected;
            SetWhiteAccepted += OnSetWhiteAccepted;
            SetWhiteRejected += OnSetWhiteRejected;
            SetBlackAccepted += OnSetBlackAccepted;
            SetBlackRejected += OnSetBlackRejected;
            MoveReceived += OnMoveReceived;
            ThisPlayerMoveRequested += OnThisPlayerMoveRequested;
            OtherPlayerMoveRequested += OnOtherPlayerMoveRequested;
            WhiteWon += OnWhiteWon;
            BlackWon += OnBlackWon;
            Draw += OnDraw;
            GameInterrupted += OnGameInterrupted;
        }

        // Клиент становится белым игроком
        public void SetWhite() => webSocket.Send("SetWhite\n");

        // Клиент становится чёрным игроком
        public void SetBlack() => webSocket.Send("SetBlack\n");

        // Клиент делает ход
        public void MakeMove(Move move)
        {
            webSocket.Send("MakeMove\n" + move.ToString());
            State = ClientState.OtherPlayerMove;
        }

        // Клиент подключён
        private void OnJoinAccepted(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Клиент не подключён
        private void OnJoinRejected(object sender, EventArgs e) =>
            State = ClientState.NotConnected;

        // Клиент стал белым игроком
        private void OnSetWhiteAccepted(object sender, EventArgs e) =>
            Color = PlayerColor.White;

        // Клиент не стал белым игроком
        private void OnSetWhiteRejected(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Клиент стал чёрным игроком
        private void OnSetBlackAccepted(object sender, EventArgs e) =>
            Color = PlayerColor.Black;

        // Клиент не стал чёрным игроком
        private void OnSetBlackRejected(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Получен ход другого игрока
        private void OnMoveReceived(object sender, MoveEventArgs e) =>
            State = ClientState.ThisPlayerMove;

        // Запрошен ход этого игрока
        private void OnThisPlayerMoveRequested(object sender, EventArgs e) =>
            State = ClientState.ThisPlayerMove;

        // Запрошен ход другого игрока
        private void OnOtherPlayerMoveRequested(object sender, EventArgs e) =>
            State = ClientState.OtherPlayerMove;

        // Белый игрок выиграл
        private void OnWhiteWon(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Чёрный игрок выиграл
        private void OnBlackWon(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Ничья
        private void OnDraw(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Игра прервана
        private void OnGameInterrupted(object sender, EventArgs e) =>
            State = ClientState.NotParticipating;

        // Подключение к серверу
        public void Connect(string userName, string host, int port)
        {
            this.userName = userName;
            // Подключение клиента
            webSocket = new WebSocket("ws://" + host + ":" + port);
            webSocket.OnOpen += WebSocketOnOpen;
            webSocket.OnMessage += WebSocketOnMessage;
            webSocket.OnError += WebSocketOnError;
            webSocket.OnClose += WebSocketOnClose;
            webSocket.Connect();
        }

        private void WebSocketOnOpen(object sender, EventArgs e)
        {
            // Отправка сообщения с именем
            webSocket.Send("Join\n" + userName);
        }

        // Получение сообщений
        private void WebSocketOnMessage(object sender, MessageEventArgs e)
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
                // Клиент подключён
                case "JoinAccepted":
                    JoinAccepted?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент не подключён
                case "JoinRejected":
                    JoinRejected?.Invoke(this, EventArgs.Empty);
                    webSocket.Close();
                    break;
                // Клиент стал белым игроком
                case "SetWhiteAccepted":
                    SetWhiteAccepted?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент не стал белым игроком
                case "SetWhiteRejected":
                    SetWhiteRejected?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент стал чёрным игроком
                case "SetBlackAccepted":
                    SetBlackAccepted?.Invoke(this, EventArgs.Empty);
                    break;
                // Клиент не стал чёрным игроком
                case "SetBlackRejected":
                    SetBlackRejected?.Invoke(this, EventArgs.Empty);
                    break;
                // Получен список игроков
                case "PlayerListReceived":
                    PlayerListReceived?.Invoke(this, new PlayerListEventArgs(messageBody.Split('\n')));
                    break;
                // Получен ход игрока
                case "MoveReceived":
                    MoveReceived?.Invoke(this, new MoveEventArgs(new Move(messageBody)));
                    break;
                // Получена доска
                case "BoardReceived":
                    BoardReceived?.Invoke(this, new BoardEventArgs(new Board(messageBody)));
                    break;
                // Запрошен ход этого игрока
                case "ThisPlayerMoveRequested":
                    ThisPlayerMoveRequested?.Invoke(this, EventArgs.Empty);
                    break;
                // Запрошен ход другого игрока
                case "OtherPlayerMoveRequested":
                    OtherPlayerMoveRequested?.Invoke(this, EventArgs.Empty);
                    break;
                // Белый игрок выиграл
                case "WhiteWon":
                    WhiteWon?.Invoke(this, EventArgs.Empty);
                    break;
                // Чёрный игрок выиграл
                case "BlackWon":
                    BlackWon?.Invoke(this, EventArgs.Empty);
                    break;
                // Ничья
                case "Draw":
                    Draw?.Invoke(this, EventArgs.Empty);
                    break;
                // Игра прервана
                case "GameInterrupted":
                    GameInterrupted?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    break;
            }
        }

        // Ошибка подключения
        private void OnWebSocketOnError(object sender, ErrorEventArgs e)
        {
            State = ClientState.NotConnected;
            webSocket.Close();
        }

        // Подключение закрыто
        private void OnWebSocketOnClose(object sender, CloseEventArgs e) =>
            State = ClientState.NotConnected;

        // Отключение от сервера
        public void Disconnect() => webSocket.Close();
    }
}
