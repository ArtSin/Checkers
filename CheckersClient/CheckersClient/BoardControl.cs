using CheckersLib;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckersClient
{
    // Отрисовка доски
    class BoardControl
    {
        // Цвета фона для клеток
        static readonly Color WHITE_CELL_COLOR = Color.FromArgb(255, 206, 158);
        static readonly Color BLACK_CELL_COLOR = Color.FromArgb(209, 139, 71);
        // Перья для выделенных, доступных, использованных полей
        static readonly Pen SELECTED_CELL_PEN = new Pen(Colors.Red, 3);
        static readonly Pen AVAILABLE_CELL_PEN = new Pen(Colors.Green, 3);
        static readonly Pen USED_CELL_PEN = new Pen(Colors.Blue, 3);
        // Перо для последнего хода
        static readonly Pen LAST_MOVE_PEN = new Pen(Colors.Gray, 5);
        static readonly Pen LAST_MOVE_USED_PEN = new Pen(Colors.DarkViolet, 5);

        // Требуется перерисовка доски
        public event EventHandler InvalidateRequired;

        // Доска
        private Board Board;
        // Нажата ли левая кнопка мыши
        private bool clicking = false;
        // Нажимаемое поле доски
        private (int, int) clickingCell = (-1, -1);
        // Выбранное поле
        private (int, int) selectedCell = (-1, -1);
        // Доступные ходы
        private List<Move> availableMoves = new List<Move>();
        // Доступные поля
        private bool[,] availableCells = new bool[Board.SIZE, Board.SIZE];
        // Количество использованных полей за ход
        private int moveIndex = 0;
        // Использованные поля
        private bool[,] usedCells = new bool[Board.SIZE, Board.SIZE];
        // Последний ход
        private Move lastMove = null;

        public BoardControl(Board board)
        {
            Board = board;
        }

        public void SetLastMove(Move move)
        {
            lastMove = move;
            // Требуется перерисовка доски
            InvalidateRequired?.Invoke(this, EventArgs.Empty);
        }

        // Сброс выделенных, доступных, использованных полей
        public void Reset(bool invalidate = false, bool resetLastMove = false)
        {
            // Сброс выбранного поля
            selectedCell = (-1, -1);
            // Сброс доступных ходов
            availableMoves = new List<Move>();
            // Сброс доступных полей
            for (int r = 0; r < Board.SIZE; r++)
                for (int c = 0; c < Board.SIZE; c++)
                    availableCells[r, c] = false;
            // Cброс использованных полей
            for (int r = 0; r < Board.SIZE; r++)
                for (int c = 0; c < Board.SIZE; c++)
                    usedCells[r, c] = false;
            // Сброс последнего хода
            if (resetLastMove)
                lastMove = null;
            // Если требуется перерисовка доски
            if (invalidate)
                InvalidateRequired?.Invoke(this, EventArgs.Empty);
        }

        // Нажата кнопка мыши
        public void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.Primary)
                return;
            // Нажата левая кнопка мыши
            var boardDrawable = (Drawable)sender;
            // Координаты поля по координатам мыши
            int row, col;
            (row, col) = GetCellCoords(e.Location, boardDrawable.ClientSize);
            // Правильные координаты
            if (row >= 0 && row < Board.SIZE && col >= 0 && col < Board.SIZE)
            {
                clicking = true;
                clickingCell = (row, col);
            }
        }

        // Отпущена кнопка мыши
        public void MouseUp(object sender, MouseEventArgs e)
        {
            if (!clicking || e.Buttons != MouseButtons.Primary)
                return;
            // Нажата левая кнопка мыши
            clicking = false;
            if (PlayerClient.GetInstance().State != ClientState.ThisPlayerMove)
                return;
            // Ход текущего игрока

            var boardDrawable = (Drawable)sender;
            // Координаты поля по координатам мыши
            int row, col;
            (row, col) = GetCellCoords(e.Location, boardDrawable.ClientSize);
            // Правильные координаты
            if (row >= 0 && row < Board.SIZE && col >= 0 && col < Board.SIZE &&
                clickingCell == (row, col))
            {
                // Если поле уже выбрано, то снять выделение
                if (moveIndex == 0 && selectedCell == clickingCell)
                    Reset();
                // Иначе выбрать поле для начала хода
                else if (moveIndex == 0 && !availableCells[row, col])
                {
                    Reset();
                    // Выбранное поле - нажатое поле
                    selectedCell = clickingCell;
                    // Доступные ходы
                    availableMoves = Board.GetMoves(PlayerClient.GetInstance().Color,
                        selectedCell, Board.CanPlayerCapture(PlayerClient.GetInstance().Color));
                    // Заполнение доступных полей
                    foreach (var move in availableMoves)
                        availableCells[move.Cells[1].Item1, move.Cells[1].Item2] = true;
                    // Использовано начальное поле
                    usedCells[row, col] = true;
                }
                // Ход уже выполняется
                else
                {
                    // Выбор ходов, в которых используется нажатое поле
                    var newAvailableMoves = availableMoves.Where(move =>
                        move.Cells[moveIndex + 1] == clickingCell).ToList();
                    if (newAvailableMoves.Count != 0)
                    {
                        availableMoves = newAvailableMoves;
                        // Если конец хода
                        if (availableMoves[0].Cells.Count == moveIndex + 2)
                        {
                            // Совершить ход
                            PlayerClient.GetInstance().MakeMove(availableMoves[0]);
                            Reset(false, true);
                            moveIndex = 0;
                        }
                        else
                        {
                            // Поле использовано
                            moveIndex++;
                            // Сброс доступных полей
                            for (int r = 0; r < Board.SIZE; r++)
                                for (int c = 0; c < Board.SIZE; c++)
                                    availableCells[r, c] = false;
                            // Заполнение доступных полей
                            foreach (var move in availableMoves)
                                availableCells[move.Cells[moveIndex + 1].Item1, move.Cells[moveIndex + 1].Item2] = true;
                            // Поле использовано
                            usedCells[row, col] = true;
                        }
                    }
                }
            }
            // Требуется перерисовка доски
            InvalidateRequired?.Invoke(this, EventArgs.Empty);
        }

        // Координаты поля по координатам мыши
        private (int, int) GetCellCoords(PointF mouseCoords, Size boardSize)
        {
            // Размер поля
            float cellSize = (Math.Min(boardSize.Width, boardSize.Height) - 30) / Board.SIZE;

            var loc = mouseCoords -
                new PointF(boardSize.Width / 2f, boardSize.Height / 2f) +
                new PointF(cellSize * Board.SIZE / 2f, cellSize * Board.SIZE / 2f);

            int col = (int)Math.Floor(loc.X / cellSize);
            int row = -(int)Math.Floor((loc.Y + 15) / cellSize - Board.SIZE + 1);
            return (row, col);
        }

        // Отрисовка
        public void Paint(object sender, PaintEventArgs e)
        {
            var boardDrawable = (Drawable)sender;
            var g = e.Graphics;
            g.AntiAlias = true;

            // Размер поля
            float cellSize = (Math.Min(boardDrawable.ClientSize.Width, boardDrawable.ClientSize.Height) - 30) / Board.SIZE;
            // Шашки белого и чёрного цвета
            var whitePiece = PaintPiece(cellSize, Colors.White, Colors.Black);
            var blackPiece = PaintPiece(cellSize, Colors.Black, Colors.White);

            // Центр доски в (0, 0)
            g.TranslateTransform(-cellSize * Board.SIZE / 2f, -cellSize * Board.SIZE / 2f);
            // Центр доски в центре области рисования
            g.TranslateTransform(boardDrawable.ClientSize.Width / 2f, boardDrawable.ClientSize.Height / 2f);
            // Отрисовка полей
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    // Сохранение преобразования
                    g.SaveTransform();
                    // Переход к текущему полю
                    g.TranslateTransform(col * cellSize, (Board.SIZE - 1 - row) * cellSize - 15);
                    // Фон
                    g.FillRectangle(Board.GetPositionColor((row, col)) ? WHITE_CELL_COLOR : BLACK_CELL_COLOR,
                        0, 0, cellSize, cellSize);
                    // Если поле выделено
                    if ((row, col) == selectedCell)
                        g.DrawRectangle(SELECTED_CELL_PEN, 1, 1, cellSize - 3, cellSize - 3);
                    // Если поле доступно для хода
                    else if (availableCells[row, col])
                        g.DrawRectangle(AVAILABLE_CELL_PEN, 1, 1, cellSize - 3, cellSize - 3);
                    // Если поле использовано
                    else if (usedCells[row, col])
                        g.DrawRectangle(USED_CELL_PEN, 1, 1, cellSize - 3, cellSize - 3);
                    // Восстановление преобразования
                    g.RestoreTransform();
                }
            // Отрисовка последнего хода
            if (lastMove != null)
            {
                for (int i = 0; i < lastMove.Cells.Count - 1; i++)
                {
                    var currCell = lastMove.Cells[i];
                    var nextCell = lastMove.Cells[i + 1];
                    g.DrawLine(LAST_MOVE_PEN, (currCell.Item2 + 0.5f) * cellSize,
                        (Board.SIZE - 0.5f - currCell.Item1) * cellSize - 15,
                        (nextCell.Item2 + 0.5f) * cellSize,
                        (Board.SIZE - 0.5f - nextCell.Item1) * cellSize - 15);
                }
                foreach (var pr in lastMove.Used)
                {
                    g.SaveTransform();
                    g.TranslateTransform((pr.Item1.Item2 + 0.5f) * cellSize,
                        (Board.SIZE - 0.5f - pr.Item1.Item1) * cellSize - 15);
                    g.DrawLine(LAST_MOVE_USED_PEN, -cellSize / 8f, -cellSize / 8f, cellSize / 8f, cellSize / 8f);
                    g.DrawLine(LAST_MOVE_USED_PEN, -cellSize / 8f, cellSize / 8f, cellSize / 8f, -cellSize / 8f);
                    g.RestoreTransform();
                }
            }
            // Отрисовка шашек
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    // Сохранение преобразования
                    g.SaveTransform();
                    // Переход к текущему полю
                    g.TranslateTransform(col * cellSize, (Board.SIZE - 1 - row) * cellSize - 15);
                    // Отрисовка шашки
                    if (Board != null)
                    {
                        var colorPiece = (Board.PiecesColor[row, col] == PlayerColor.White) ? whitePiece : blackPiece;
                        switch (Board.PiecesType[row, col])
                        {
                            // Обычная
                            case PieceType.Normal:
                                g.DrawImage(colorPiece, 0, 0);
                                break;
                            // Дамка
                            case PieceType.King:
                                g.DrawImage(colorPiece, 0, 0.15f * cellSize);
                                g.DrawImage(colorPiece, 0, 0);
                                break;
                            // Нет шашки
                            default:
                                break;
                        }
                    }
                    // Восстановление преобразования
                    g.RestoreTransform();
                }

            // Шрифт
            var font = new Font(SystemFont.Default, 12);
            // Подписи для строк
            for (int row = 0; row < Board.SIZE; row++)
            {
                var str = $"{row + 1}";
                var sz = g.MeasureString(font, str);
                g.DrawText(font, Colors.Black, -sz.Width - 5, (Board.SIZE - 0.5f - row) * cellSize - 15 - sz.Height / 2f, str);
            }
            // Подписи для столбцов
            for (int col = 0; col < Board.SIZE; col++)
            {
                var str = $"{(char)(col + 'a')}";
                var sz = g.MeasureString(font, str);
                g.DrawText(font, Colors.Black, (col + 0.5f) * cellSize - sz.Width / 2f, Board.SIZE * cellSize - 15, str);
            }

            whitePiece.Dispose();
            blackPiece.Dispose();
        }

        // Отрисовка шашки в изображение
        Bitmap PaintPiece(float cellSize, Color color1, Color color2)
        {
            // Размер изображения
            int bmpSize = (int)Math.Ceiling(cellSize);
            // Изображение
            var bmp = new Bitmap(new Size(bmpSize, bmpSize), PixelFormat.Format32bppRgba);
            var g = new Graphics(bmp);
            g.AntiAlias = true;
            // Перо
            var pen2 = new Pen(color2, 2f / cellSize);

            // Масштабирование
            g.ScaleTransform(cellSize);

            // Заливка
            g.FillRectangle(color1, 0.1f, 0.4f, 0.8f, 0.15f);
            g.FillEllipse(color1, 0.1f, 0.25f, 0.8f, 0.3f);
            g.FillEllipse(color1, 0.1f, 0.4f, 0.8f, 0.3f);

            // Рамки
            g.DrawEllipse(pen2, 0.1f, 0.25f, 0.8f, 0.3f);
            g.DrawLine(pen2, 0.1f, 0.4f, 0.1f, 0.55f);
            g.DrawLine(pen2, 0.9f, 0.4f, 0.9f, 0.55f);
            g.DrawArc(pen2, 0.1f, 0.4f, 0.8f, 0.3f, 0, 180);

            pen2.Dispose();
            g.Dispose();
            return bmp;
        }
    }
}
