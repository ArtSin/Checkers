using CheckersLib;
using Eto.Forms;
using System;
using WebSocketSharp.NetCore;

namespace CheckersClient
{
    public partial class MainForm : Form
    {
        Board board = new Board();
        BoardControl boardControl;

        public MainForm()
        {
            boardControl = new BoardControl(board);
            InitializeComponent();

            boardControl.InvalidateRequired += BoardRendererInvalidateRequired;
            PlayerClient.GetInstance().WebSocketOnError += PlayerClientWebSocketOnError;
            PlayerClient.GetInstance().JoinRejected += PlayerClientJoinRejected;
            PlayerClient.GetInstance().SetWhiteAccepted += PlayerClientSetWhiteAccepted;
            PlayerClient.GetInstance().SetWhiteRejected += PlayerClientSetWhiteRejected;
            PlayerClient.GetInstance().SetBlackAccepted += PlayerClientSetBlackAccepted;
            PlayerClient.GetInstance().SetBlackRejected += PlayerClientSetBlackRejected;
            PlayerClient.GetInstance().PlayerListReceived += PlayerClientPlayerListReceived;
            PlayerClient.GetInstance().MoveReceived += PlayerClientMoveReceived;
            PlayerClient.GetInstance().BoardReceived += PlayerClientBoardReceived;
            PlayerClient.GetInstance().WhiteWon += PlayerClientWhiteWon;
            PlayerClient.GetInstance().BlackWon += PlayerClientBlackWon;
            PlayerClient.GetInstance().Draw += PlayerClientDraw;
            PlayerClient.GetInstance().GameInterrupted += PlayerClientGameInterrupted;
            PlayerClient.GetInstance().StateChanged += PlayerClientStateChanged;
        }

        void ConnectButtonClicked(string userName, string host, string portStr)
        {
            int port;
            if (!int.TryParse(portStr, out port))
            {
                MessageBox.Show("Порт должен быть числом!", "Ошибка", MessageBoxType.Error);
                return;
            }

            if (userName.Length < 1 || userName.Length > 30)
            {
                MessageBox.Show("Имя должно содержать не менее 1 и не более 30 символов!", "Ошибка", MessageBoxType.Error);
                return;
            }

            Application.Instance.Invoke(() =>
            {
                FindChild<Button>("connectButton").Enabled = false;
                FindChild<Button>("disconnectButton").Enabled = false;
            });
            PlayerClient.GetInstance().Connect(userName, host, port);
        }

        // Требуется перерисовка доски
        private void BoardRendererInvalidateRequired(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            FindChild<Drawable>("boardDrawable").Invalidate());

        // Ошибка подключения
        private void PlayerClientWebSocketOnError(object sender, ErrorEventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Ошибка подключения: " + e.Message, "Ошибка", MessageBoxType.Error));

        // Клиент не подключён
        private void PlayerClientJoinRejected(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Сервер отклонил подключение!", "Ошибка", MessageBoxType.Error));

        // Клиент стал белым игроком
        private void PlayerClientSetWhiteAccepted(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Вы стали белым игроком.", "Информация", MessageBoxType.Information));

        // Клиент не стал белым игроком
        private void PlayerClientSetWhiteRejected(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Не удалось стать белым игроком!", "Ошибка", MessageBoxType.Error));

        // Клиент стал чёрным игроком
        private void PlayerClientSetBlackAccepted(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Вы стали чёрным игроком.", "Информация", MessageBoxType.Information));

        // Клиент не стал чёрным игроком
        private void PlayerClientSetBlackRejected(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Не удалось стать чёрным игроком!", "Ошибка", MessageBoxType.Error));

        // Получен список игроков
        private void PlayerClientPlayerListReceived(object sender, PlayerListEventArgs e) =>
            Application.Instance.Invoke(() => FindChild<ListBox>("playersListBox").DataStore = e.Players);

        // Получен ход другого игрока
        private void PlayerClientMoveReceived(object sender, MoveEventArgs e) =>
            Application.Instance.Invoke(() =>
            {
                board.DoMove(e.Move);
                boardControl.SetLastMove(e.Move);
                var items = FindChild<ListBox>("gameLogListBox").Items;
                items.Add($"{items.Count / 2 + 1}" + e.Move.ToHumanReadableString());
            });

        // Получена доска
        private void PlayerClientBoardReceived(object sender, BoardEventArgs e) =>
            Application.Instance.Invoke(() =>
            {
                board.Update(e.Board);
                FindChild<Drawable>("boardDrawable").Invalidate();
            });

        // Белый игрок выиграл
        private void PlayerClientWhiteWon(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Белый игрок выиграл!", "Информация", MessageBoxType.Information));

        // Чёрный игрок выиграл
        private void PlayerClientBlackWon(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Чёрный игрок выиграл!", "Информация", MessageBoxType.Information));

        // Ничья
        private void PlayerClientDraw(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Ничья!", "Информация", MessageBoxType.Information));

        // Игра прервана
        private void PlayerClientGameInterrupted(object sender, EventArgs e) =>
            Application.Instance.Invoke(() =>
            MessageBox.Show("Игра прервана!", "Ошибка", MessageBoxType.Warning));

        // Изменилось состояние клиента
        private void PlayerClientStateChanged(object sender, StateEventArgs e) =>
            Application.Instance.Invoke(() =>
            {
                var gameLogListBox = FindChild<ListBox>("gameLogListBox");
                var statusLabel = FindChild<Label>("statusLabel");
                switch (e.State)
                {
                    case ClientState.NotConnected:
                        gameLogListBox.Items.Clear();
                        statusLabel.Text = "Нет подключения к серверу.";
                        FindChild<Button>("connectButton").Enabled = true;
                        FindChild<Button>("disconnectButton").Enabled = false;
                        FindChild<Button>("setWhiteButton").Enabled = false;
                        FindChild<Button>("setBlackButton").Enabled = false;
                        FindChild<ListBox>("playersListBox").DataStore = null;
                        board.Update(new Board());
                        boardControl.Reset(true, true);
                        break;
                    case ClientState.NotParticipating:
                        gameLogListBox.Items.Clear();
                        statusLabel.Text = "Подключён к серверу.";
                        FindChild<Button>("connectButton").Enabled = false;
                        FindChild<Button>("disconnectButton").Enabled = true;
                        FindChild<Button>("setWhiteButton").Enabled = true;
                        FindChild<Button>("setBlackButton").Enabled = true;
                        boardControl.Reset(true, true);
                        break;
                    case ClientState.ThisPlayerMove:
                        statusLabel.Text = "Ваш ход.";
                        FindChild<Button>("setWhiteButton").Enabled = false;
                        FindChild<Button>("setBlackButton").Enabled = false;
                        break;
                    case ClientState.OtherPlayerMove:
                        statusLabel.Text = "Ход другого игрока.";
                        FindChild<Button>("setWhiteButton").Enabled = false;
                        FindChild<Button>("setBlackButton").Enabled = false;
                        break;
                }
            });
    }
}
