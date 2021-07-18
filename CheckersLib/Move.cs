using System.Collections.Generic;
using System.Linq;

namespace CheckersLib
{
    // Ход одного игрока
    public class Move
    {
        // Игрок
        public PlayerColor Player { get; private set; }
        // Поля
        public List<(int, int)> Cells { get; private set; }
        // Номер поля, когда шашка становится дамкой
        public int PosKing { get; set; }
        // Побитые за ход шашки
        public List<((int, int), PieceType)> Used { get; private set; }

        // Создание хода
        public Move(PlayerColor player, List<(int, int)> cells, int posKing)
        {
            Player = player;
            Cells = cells;
            PosKing = posKing;
            Used = new List<((int, int), PieceType)>();
        }

        // Создание хода из строки
        public Move(string message)
        {
            // Разделение сообщения
            var parts = message.Split(' ');
            // Игрок
            Player = (parts[0] == "White" ? PlayerColor.White : PlayerColor.Black);
            // Поля
            Cells = parts[1].Split('-').Select(str => (str[0] - '0', str[1] - '0')).ToList();
            // Становится ли шашка дамкой
            PosKing = int.Parse(parts[2]);
            // Побитые за ход шашки
            Used = parts[3].Split('-', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(str => ((str[0] - '0', str[1] - '0'), PieceType.Normal)).ToList();
        }

        // Создание хода из другого хода
        public Move(Move other)
        {
            Player = other.Player;
            Cells = new List<(int, int)>(other.Cells);
            PosKing = other.PosKing;
            Used = new List<((int, int), PieceType)>(other.Used.Count);
            foreach (var pr in other.Used)
                Used.Add(pr);
        }

        // Добавление поля в ход
        public void AddCell((int, int) cell) => Cells.Add(cell);

        // Преобразование хода в строку
        public override string ToString() =>
            (Player == PlayerColor.White ? "White " : "Black ") +
            string.Join('-', Cells.Select(pr => $"{pr.Item1}{pr.Item2}")) +
            " " + PosKing.ToString() +
            " " + string.Join("-", Used.Select(pr => $"{pr.Item1.Item1}{pr.Item1.Item2}"));

        // Преобразование хода в удобочитаемую строку
        public string ToHumanReadableString() =>
            (Player == PlayerColor.White ? "Б: " : "Ч: ") +
            string.Join(Used.Count == 0 ? '-' : ':',
                Cells.Select(pr => $"{(char)(pr.Item2 + 'a')}{pr.Item1 + 1}"));
    }
}
