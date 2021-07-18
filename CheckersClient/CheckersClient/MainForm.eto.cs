using Eto.Drawing;
using Eto.Forms;

namespace CheckersClient
{
    partial class MainForm : Form
    {
        void InitializeComponent()
        {
            Title = "Шашки";
            MinimumSize = new Size(640, 360);
            Size = new Size(768, 480);
            Padding = 5;

            var gameLogListBox = new ListBox() { Width = 150, ID = "gameLogListBox" };

            var statusLabel = new Label() { ID = "statusLabel", Text = "Нет подключения к серверу", TextAlignment = TextAlignment.Center };

            var boardDrawable = new Drawable(false) { ID = "boardDrawable" };
            boardDrawable.Paint += boardControl.Paint;
            boardDrawable.MouseDown += boardControl.MouseDown;
            boardDrawable.MouseUp += boardControl.MouseUp;

            var hostLabel = new Label() { Text = "Адрес:", TextAlignment = TextAlignment.Right };
            var hostTextBox = new TextBox();

            var portLabel = new Label() { Text = "Порт:", TextAlignment = TextAlignment.Right };
            var portTextBox = new TextBox();

            var userNameLabel = new Label() { Text = "Имя:", TextAlignment = TextAlignment.Right };
            var userNameTextBox = new TextBox();

            var connectButtonCommand = new Command((sender, e) =>
                ConnectButtonClicked(userNameTextBox.Text, hostTextBox.Text, portTextBox.Text));
            var connectButton = new Button()
            {
                ID = "connectButton",
                Text = "Подключиться к серверу",
                Command = connectButtonCommand
            };

            var disconnectButtonCommand = new Command((sender, e) => PlayerClient.GetInstance().Disconnect());
            var disconnectButton = new Button()
            {
                ID = "disconnectButton",
                Text = "Отключиться от сервера",
                Command = disconnectButtonCommand,
                Enabled = false
            };

            var setWhiteButtonCommand = new Command((sender, e) => PlayerClient.GetInstance().SetWhite());
            var setWhiteButton = new Button()
            {
                ID = "setWhiteButton",
                Text = "Стать белым игроком",
                Command = setWhiteButtonCommand,
                Enabled = false
            };

            var setBlackButtonCommand = new Command((sender, e) => PlayerClient.GetInstance().SetBlack());
            var setBlackButton = new Button()
            {
                ID = "setBlackButton",
                Text = "Стать чёрным игроком",
                Command = setBlackButtonCommand,
                Enabled = false
            };

            var playersListBox = new ListBox() { ID = "playersListBox" };

            Content = new StackLayout(
                new StackLayoutItem(gameLogListBox, HorizontalAlignment.Left, false),

                new StackLayoutItem(
                    new StackLayout(
                        new StackLayoutItem(statusLabel, VerticalAlignment.Top, false),
                        new StackLayoutItem(boardDrawable, VerticalAlignment.Stretch, true)
                        )
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 5
                    },
                    HorizontalAlignment.Stretch, true),

                new StackLayoutItem(
                    new StackLayout(
                            new StackLayout(
                                new StackLayoutItem(hostLabel, HorizontalAlignment.Left, false),
                                new StackLayoutItem(hostTextBox, HorizontalAlignment.Stretch, true)
                                )
                            {
                                Orientation = Orientation.Horizontal,
                                VerticalContentAlignment = VerticalAlignment.Stretch,
                                Spacing = 5
                            },

                            new StackLayout(
                                new StackLayoutItem(portLabel, HorizontalAlignment.Left, false),
                                new StackLayoutItem(portTextBox, HorizontalAlignment.Stretch, true)
                                )
                            {
                                Orientation = Orientation.Horizontal,
                                VerticalContentAlignment = VerticalAlignment.Stretch,
                                Spacing = 5
                            },

                            new StackLayout(
                                new StackLayoutItem(userNameLabel, HorizontalAlignment.Left, false),
                                new StackLayoutItem(userNameTextBox, HorizontalAlignment.Stretch, true)
                                )
                            {
                                Orientation = Orientation.Horizontal,
                                VerticalContentAlignment = VerticalAlignment.Stretch,
                                Spacing = 5
                            },

                            connectButton,
                            disconnectButton,
                            setWhiteButton,
                            setBlackButton,
                            new StackLayoutItem(playersListBox, VerticalAlignment.Stretch, true)
                    )
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 5
                    },
                    HorizontalAlignment.Right, false)
                )
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Spacing = 5,
                Padding = new Padding(5)
            };
        }
    }
}
