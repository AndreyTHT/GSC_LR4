using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSC_Lr4
{
    public partial class Form1 : Form
    {
        // Основной холст для рисования
        Bitmap myBitmap;

        // Графический объект для работы с Bitmap
        Graphics g;

        // Перо для рисования (цвет и толщина)
        Pen DrawPen = new Pen(Color.Black, 1);

        // Список точек для хранения вершин текущей фигуры
        List<Point> VertexList = new List<Point>();

        // Список всех нарисованных фигур
        List<Pgn> pgns = new List<Pgn>();

        // Текущая операция: 1 — рисование, 2 — перемещение, 3 — вращение, 4 — масштабирование
        int Operation = 1;

        // Индекс выбранной фигуры (-1 означает, что фигура не выбрана)
        int checkPgn = -1;

        // Координаты последнего нажатия мыши
        Point pictureBox1MousePos = new Point();

        public Form1()
        {
            InitializeComponent(); // Генерируемая автоматически инициализация формы
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height); // Создание холста с размерами pictureBox1
            g = Graphics.FromImage(myBitmap); // Привязка графического объекта к холсту
        }

        // Обработчик кнопки для выбора операции (перемещение, вращение, масштабирование)
        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    Operation = 2; // Установить операцию на перемещение
                    break;
                case 1:
                    Operation = 3; // Установить операцию на вращение
                    break;
                case 2:
                    Operation = 4; // Установить операцию на масштабирование
                    break;
            }
        }

        // Обработчик нажатия кнопки мыши на pictureBox1
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Сохраняем позицию нажатия мыши
            pictureBox1MousePos = e.Location;

            // В зависимости от текущей операции выполняем соответствующие действия
            switch (Operation)
            {
                case 1: // Если операция — рисование новой фигуры
                    InputPgn(e); // Вызываем метод для ввода новой фигуры
                    break;

                default: // Если операция — выбор фигуры
                    checkPgn = -1; // Сбрасываем выбранную фигуру
                    for (int i = 0; i < pgns.Count; i++) // Проходим по всем фигурам
                    {
                        if (pgns[i].ThisPgn(e.X, e.Y)) // Проверяем, находится ли точка внутри фигуры
                        {
                            checkPgn = i; // Устанавливаем индекс выбранной фигуры
                            break;
                        }
                    }
                    break;
            }

            // Обновляем изображение на pictureBox1
            pictureBox1.Image = myBitmap;
        }

        // Обработчик изменения выбранного цвета из comboBox1
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Устанавливаем цвет пера в зависимости от выбора
            switch (comboBox1.SelectedIndex)
            {
                case 0: DrawPen.Color = Color.Black; break;
                case 1: DrawPen.Color = Color.Red; break;
                case 2: DrawPen.Color = Color.Green; break;
                case 3: DrawPen.Color = Color.Blue; break;
            }
        }

        // Обработчик кнопки для очистки всех фигур
        private void button2_Click(object sender, EventArgs e)
        {
            pgns.Clear(); // Очищаем список фигур
            VertexList.Clear(); // Очищаем текущий список вершин
            Operation = 1; // Сбрасываем операцию на рисование
            RedrawAll(); // Перерисовываем всё
        }

        // Обработчик движения мыши при нажатой кнопке
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Проверяем, нажата ли левая кнопка и выбран ли корректный индекс фигуры
            if (e.Button != MouseButtons.Left || checkPgn < 0 || checkPgn >= pgns.Count)
                return;

            // Получаем выбранную фигуру
            var selectedPgn = pgns[checkPgn];

            // Выполняем действие в зависимости от текущей операции
            switch (Operation)
            {
                case 2: // Перемещение
                    selectedPgn.Move(e.X - pictureBox1MousePos.X, e.Y - pictureBox1MousePos.Y);
                    break;

                case 3: // Вращение
                    selectedPgn.Move(-selectedPgn.GetCenter().X, -selectedPgn.GetCenter().Y); // Перенос к центру координат
                    selectedPgn.Rotate(CalculateAngle(e, selectedPgn)); // Вращение
                    selectedPgn.Move(selectedPgn.GetCenter().X, selectedPgn.GetCenter().Y); // Возврат на место
                    break;

                case 4: // Масштабирование
                    selectedPgn.Move(-selectedPgn.GetCenter().X, -selectedPgn.GetCenter().Y); // Перенос к центру координат
                    double scale = CalculateScale(e, selectedPgn); // Вычисляем масштаб
                    if (scale > 0) // Убедимся, что масштабирование корректно
                        selectedPgn.Scale(scale);
                    selectedPgn.Move(selectedPgn.GetCenter().X, selectedPgn.GetCenter().Y); // Возврат на место
                    break;
            }

            pictureBox1MousePos = e.Location; // Обновляем позицию мыши
            RedrawAll(); // Перерисовываем
        }

        // Вычисление масштаба на основе движения мыши
        private double CalculateScale(MouseEventArgs e, Pgn selectedPgn)
        {
            double lengthNew = Math.Sqrt(Math.Pow(e.X - selectedPgn.GetCenter().X, 2) + Math.Pow(e.Y - selectedPgn.GetCenter().Y, 2));
            double lengthOld = Math.Sqrt(Math.Pow(pictureBox1MousePos.X - selectedPgn.GetCenter().X, 2) + Math.Pow(pictureBox1MousePos.Y - selectedPgn.GetCenter().Y, 2));

            // Проверка деления на ноль
            if (lengthOld == 0) return 1; // Возвращаем масштаб 1 (без изменений)

            return lengthNew / lengthOld;
        }

        // Вычисление угла поворота на основе движения мыши
        private double CalculateAngle(MouseEventArgs e, Pgn selectedPgn)
        {
            PointF A = new PointF(e.X - selectedPgn.GetCenter().X, e.Y - selectedPgn.GetCenter().Y);
            PointF B = new PointF(pictureBox1MousePos.X - selectedPgn.GetCenter().X, pictureBox1MousePos.Y - selectedPgn.GetCenter().Y);
            return -Math.Atan2(A.Y, A.X) + Math.Atan2(B.Y, B.X);
        }

        // Обработчик изменения размера pictureBox1
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            // Сохраняем текущий холст при изменении размеров pictureBox1
            Bitmap newBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics tempG = Graphics.FromImage(newBitmap);
            tempG.DrawImage(myBitmap, 0, 0); // Копируем старое изображение на новый холст
            myBitmap = newBitmap;
            g = Graphics.FromImage(myBitmap); // Обновляем Graphics
            RedrawAll(); // Перерисовываем всё
        }

        // Метод для ввода новой фигуры
        private void InputPgn(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Добавляем новую точку в список вершин
                Point NewP = new Point(e.X, e.Y);
                VertexList.Add(NewP);
                int k = VertexList.Count;

                // Рисуем линию между точками
                if (k > 1)
                    g.DrawLine(DrawPen, VertexList[k - 2], VertexList[k - 1]);
                else // Если точка первая, то рисуем её как маленький квадрат
                    g.DrawRectangle(DrawPen, e.X, e.Y, 1, 1);
            }
            else if (e.Button == MouseButtons.Right && VertexList.Count > 2) // Завершаем ввод фигуры
            {
                Point NewP = new Point(e.X, e.Y);
                VertexList.Add(NewP);
                int k = VertexList.Count;

                // Замыкаем фигуру линией от последней точки к первой
                if (k > 1)
                    g.DrawLine(DrawPen, VertexList[k - 1], VertexList[0]);
                else
                    g.DrawRectangle(DrawPen, e.X, e.Y, 1, 1);

                // Создаём новую фигуру и добавляем её в список
                Pgn NewPgn = new Pgn();
                foreach (var item in VertexList)
                    NewPgn.Add(item);

                NewPgn.search_center(); // Ищем центр новой фигуры
                g.DrawRectangle(new Pen(Color.Red), NewPgn.GetCenter().X, NewPgn.GetCenter().Y, 2, 2); // Рисуем центр
                VertexList.Clear(); // Очищаем список вершин

                pgns.Add(NewPgn); // Добавляем фигуру в список
                RedrawAll(); // Перерисовываем всё
            }
        }

        // Метод для перерисовки всех фигур
        private void RedrawAll()
        {
            g.Clear(pictureBox1.BackColor); // Очищаем холст
            foreach (var pgn in pgns)
            {
                pgn.Fill(g, DrawPen); // Заполняем фигуру
                pgn.search_center(); // Находим центр фигуры
                g.DrawRectangle(new Pen(Color.Red), pgn.GetCenter().X, pgn.GetCenter().Y, 2, 2); // Рисуем центр фигуры
            }
            pictureBox1.Image = myBitmap; // Обновляем изображение в pictureBox1
        }
    }
}
