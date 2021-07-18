using CheckersLib;
using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp.NetCore.Server;

namespace CheckersServer
{
    public class Server
    {
        // Максимальное количество ходов до ничьей
        private const int MAX_MOVES = 200;

        private static Server instance;

        private Server()
        {
        }

        public static Server GetInstance()
        {
            if (instance == null)
                instance = new Server();
            return instance;
        }

        WebSocketSessionManager sessions;
        // Все подключенные клиенты
        Dictionary<string, ServerClient> clients = new Dictionary<string, ServerClient>();
        string whitePlayerId = null;
        string blackPlayerId = null;
        Board board = null;
        int moves = 0;

        public void SetSessions(WebSocketSessionManager sessions)
        {
            this.sessions = sessions;
        }

        // Клиент подключается
        private void OnClientJoin(object sender, JoinEventArgs e)
        {
            var client = (ServerClient)sender;
            if (clients.FirstOrDefault(pr => pr.Value.UserName == e.UserName).Key != null)
                sessions.SendTo("JoinRejected\n", client.ID);
            else
            {
                client.UserName = e.UserName;
                sessions.SendTo("JoinAccepted\n", client.ID);
                SendPlayerList();
            }
        }

        // Клиент становится белым игроком
        private void OnClientSetWhite(object sender, EventArgs e)
        {
            var client = (ServerClient)sender;
            if (whitePlayerId == null && client.Role == PlayerRole.Spectator)
            {
                whitePlayerId = client.ID;
                client.Role = PlayerRole.White;
                sessions.SendTo("SetWhiteAccepted\n", client.ID);
                SendPlayerList();
                if (blackPlayerId != null)
                    StartGame();
            }
            else
                sessions.SendTo("SetWhiteRejected\n", client.ID);
        }

        // Клиент становится чёрным игроком
        private void OnClientSetBlack(object sender, EventArgs e)
        {
            var client = (ServerClient)sender;
            if (blackPlayerId == null && client.Role == PlayerRole.Spectator)
            {
                blackPlayerId = client.ID;
                client.Role = PlayerRole.Black;
                sessions.SendTo("SetBlackAccepted\n", client.ID);
                SendPlayerList();
                if (whitePlayerId != null)
                    StartGame();
            }
            else
                sessions.SendTo("SetBlackRejected\n", client.ID);
        }

        // Запуск игры
        private void StartGame()
        {
            // Создание доски с начальной расстановкой
            board = new Board(false);
            // Сброс количества ходов
            moves = 0;
            // Отправка доски
            SendBoard();
            // Запрос хода белого игрока
            RequestMove(whitePlayerId);
        }

        // Остановка игры
        private void StopGame(bool correct = true)
        {
            // Сброс роли белого игрока
            if (whitePlayerId != null)
            {
                clients[whitePlayerId].Role = PlayerRole.Spectator;
                whitePlayerId = null;
            }
            // Сброс роли чёрного игрока
            if (blackPlayerId != null)
            {
                clients[blackPlayerId].Role = PlayerRole.Spectator;
                blackPlayerId = null;
            }
            // Пустая доска
            board = new Board();
            // Отправка доски
            SendBoard();
            if (!correct)
            {
                // Сообщение о прерванной игре
                SendGameInterrupted();
            }
            else
            {
                // Отправка списка игроков
                SendPlayerList();
            }
        }

        // Клиент делает ход
        private void OnClientMakeMove(object sender, MoveEventArgs e)
        {
            // Совершение хода на доске
            board.DoMove(e.Move);
            // Отправка хода
            SendMove(e.Move);
            // Отправка доски
            SendBoard();
            // Если белый игрок выиграл
            if (e.Move.Player == PlayerColor.White && !board.CanPlayerMove(PlayerColor.Black))
            {
                SendWhiteWon();
                StopGame();
                return;
            }
            // Если чёрный игрок выиграл
            else if (e.Move.Player == PlayerColor.Black && !board.CanPlayerMove(PlayerColor.White))
            {
                SendBlackWon();
                StopGame();
                return;
            }
            moves++;
            // Если ничья
            if (moves >= MAX_MOVES)
            {
                SendDraw();
                StopGame();
                return;
            }
            // Запрос хода другого игрока
            RequestMove(e.Move.Player == PlayerColor.Black ? whitePlayerId : blackPlayerId);
        }

        // Отправка списка игроков всем клиентам
        private void SendPlayerList() =>
            sessions.Broadcast("PlayerListReceived\n" +
            string.Join('\n', clients.Select(pr => (pr.Value.Role != PlayerRole.Spectator ?
            (pr.Value.Role == PlayerRole.White ? "[Белый] " : "[Чёрный] ") : "") + pr.Value.UserName)
            .OrderBy(x => x)));

        // Отправка хода всем клиентам
        private void SendMove(Move move) => sessions.Broadcast("MoveReceived\n" + move.ToString());

        // Отправка доски всем клиентам
        private void SendBoard() => sessions.Broadcast("BoardReceived\n" + board.ToString());

        // Запросить ход
        private void RequestMove(string id)
        {
            foreach (var pr in clients)
                sessions.SendTo((pr.Key == id) ? "ThisPlayerMoveRequested\n" : "OtherPlayerMoveRequested\n", pr.Key);
        }

        // Отправка сообщения о победе белого игрока всем клиентам
        private void SendWhiteWon() => sessions.Broadcast("WhiteWon\n");

        // Отправка сообщения о победе чёрного игрока всем клиентам
        private void SendBlackWon() => sessions.Broadcast("BlackWon\n");

        // Отправка сообщения о ничьей всем клиентам
        private void SendDraw() => sessions.Broadcast("Draw\n");

        // Отправка сообщения о прерванной игре
        private void SendGameInterrupted() => sessions.Broadcast("GameInterrupted\n");

        // Добавление клиента
        public void AddClient(ServerClient client)
        {
            clients.Add(client.ID, client);
            client.Join += OnClientJoin;
            client.SetWhite += OnClientSetWhite;
            client.SetBlack += OnClientSetBlack;
            client.MakeMove += OnClientMakeMove;
        }

        // Удаление клиента
        public void RemoveClient(string id)
        {
            // Нет клиента
            if (!clients.ContainsKey(id))
                return;

            var client = clients[id];
            // Если не наблюдатель, то остановка игры
            if (whitePlayerId == client.ID || blackPlayerId == client.ID)
                StopGame(false);

            client.Join -= OnClientJoin;
            client.SetWhite -= OnClientSetWhite;
            client.SetBlack -= OnClientSetBlack;
            client.MakeMove -= OnClientMakeMove;
            clients.Remove(id);

            // Отправка списка игроков
            SendPlayerList();
        }
    }
}
