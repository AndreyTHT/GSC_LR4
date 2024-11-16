using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSC_Lr4
{
    public partial class Form1 : Form
    {
        Bitmap myBitmap;
        Graphics g;
        Pen DrawPen = new Pen(Color.Black, 1);
        List<Point> VertexList = new List<Point>();
        List<Pgn> pgns = new List<Pgn>();
        int Operation = 1; // Рисование
        int checkPgn = -1; // Индекс выбранной фигуры (-1 означает, что ничего не выбрано)
        Point pictureBox1MousePos = new Point(); // Позиция последнего нажатия

        public Form1()
        {
            InitializeComponent();
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(myBitmap);
        }

        // Метод для заполнения списка вершин новой фигуры
        

        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    Operation = 2; // Перемещение
                    break;
                case 1:
                    Operation = 3; // Вращение
                    break;
                case 2:
                    Operation = 4; // Масштабирование
                    break;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1MousePos = e.Location;

            switch (Operation)
            {
                case 1: // Ввод новой фигуры
                    InputPgn(e);
                    break;

                default: // Выбор фигуры
                    checkPgn = -1;
                    for (int i = 0; i < pgns.Count; i++)
                    {
                        if (pgns[i].ThisPgn(e.X, e.Y))
                        {
                            checkPgn = i;
                            break;
                        }
                    }
                    break;
            }

            pictureBox1.Image = myBitmap;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: DrawPen.Color = Color.Black; break;
                case 1: DrawPen.Color = Color.Red; break;
                case 2: DrawPen.Color = Color.Green; break;
                case 3: DrawPen.Color = Color.Blue; break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pgns.Clear();
            VertexList.Clear();
            Operation = 1;
            RedrawAll();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || checkPgn < 0 || checkPgn >= pgns.Count)
                return; // Проверка на корректный индекс

            var selectedPgn = pgns[checkPgn];

            switch (Operation)
            {
                case 2: // Перемещение
                    selectedPgn.Move(e.X - pictureBox1MousePos.X, e.Y - pictureBox1MousePos.Y);
                    break;

                case 3: // Вращение
                    selectedPgn.Move(-selectedPgn.GetCenter().X, -selectedPgn.GetCenter().Y);
                    selectedPgn.Rotate(CalculateAngle(e, selectedPgn));
                    selectedPgn.Move(selectedPgn.GetCenter().X, selectedPgn.GetCenter().Y);
                    break;

                case 4: // Масштабирование
                    selectedPgn.Move(-selectedPgn.GetCenter().X, -selectedPgn.GetCenter().Y);
                    double scale = CalculateScale(e, selectedPgn);
                    if (scale > 0) // Убедимся, что масштабирование корректно
                        selectedPgn.Scale(scale);
                    selectedPgn.Move(selectedPgn.GetCenter().X, selectedPgn.GetCenter().Y);
                    break;
            }

            pictureBox1MousePos = e.Location;
            RedrawAll();
        }

        private double CalculateScale(MouseEventArgs e, Pgn selectedPgn)
        {
            double lengthNew = Math.Sqrt(Math.Pow(e.X - selectedPgn.GetCenter().X, 2) + Math.Pow(e.Y - selectedPgn.GetCenter().Y, 2));
            double lengthOld = Math.Sqrt(Math.Pow(pictureBox1MousePos.X - selectedPgn.GetCenter().X, 2) + Math.Pow(pictureBox1MousePos.Y - selectedPgn.GetCenter().Y, 2));

            // Проверка на деление на ноль
            if (lengthOld == 0) return 1; // Возвращаем масштаб 1 (без изменений)

            return lengthNew / lengthOld;
        }

        private double CalculateAngle(MouseEventArgs e, Pgn selectedPgn)
        {
            PointF A = new PointF(e.X - selectedPgn.GetCenter().X, e.Y - selectedPgn.GetCenter().Y);
            PointF B = new PointF(pictureBox1MousePos.X - selectedPgn.GetCenter().X, pictureBox1MousePos.Y - selectedPgn.GetCenter().Y);
            return -Math.Atan2(A.Y, A.X) + Math.Atan2(B.Y, B.X);
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            // Сохраняем существующий контент при изменении размера
            Bitmap newBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics tempG = Graphics.FromImage(newBitmap);
            tempG.DrawImage(myBitmap, 0, 0);
            myBitmap = newBitmap;
            g = Graphics.FromImage(myBitmap);
            RedrawAll();
        }

        private void InputPgn(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point NewP = new Point(e.X, e.Y);
                VertexList.Add(NewP);
                int k = VertexList.Count;

                if (k > 1)
                    g.DrawLine(DrawPen, VertexList[k - 2], VertexList[k - 1]);
                else
                    g.DrawRectangle(DrawPen, e.X, e.Y, 1, 1);
            }
            else if (e.Button == MouseButtons.Right && VertexList.Count > 2) // Завершение ввода фигуры
            {
                Point NewP = new Point(e.X, e.Y);
                VertexList.Add(NewP);
                int k = VertexList.Count;
                if (k > 1)
                    g.DrawLine(DrawPen, VertexList[k - 1], VertexList[0]);
                else
                    g.DrawRectangle(DrawPen, e.X, e.Y, 1, 1);
                Pgn NewPgn = new Pgn();
                foreach (var item in VertexList)
                    NewPgn.Add(item);

                NewPgn.search_center();
                g.DrawRectangle(new Pen(Color.Red), NewPgn.GetCenter().X, NewPgn.GetCenter().Y, 2, 2);
                VertexList.Clear();

                pgns.Add(NewPgn);
                RedrawAll();
            }
        }


        private void RedrawAll()
        {
            g.Clear(pictureBox1.BackColor);
            foreach (var pgn in pgns)
            {
                pgn.Fill(g, DrawPen);
                pgn.search_center();
                g.DrawRectangle(new Pen(Color.Red), pgn.GetCenter().X, pgn.GetCenter().Y, 2, 2);
            }
            pictureBox1.Image = myBitmap;
        }
    }
}
