using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace GSC_Lr4
{
   public class Pgn
    {
        List<PointF> VertexList;
        int Ymax = 0, Ymin = 0;
        PointF center;
        int only_one = 0;
        public Pgn() { VertexList = new List<PointF>(); }

        // метод Добавление вершины
        public void Add(Point NewVertex) { VertexList.Add(NewVertex); }

        public void search_center() // поиск центра нашей фигуры
        {
            float x = 0, y = 0;
            for (int i = 0; i < VertexList.Count; i++)
            {
                x += VertexList[i].X;
                y += VertexList[i].Y;
            }
            center.X = x / VertexList.Count;
            center.Y = y / VertexList.Count;
            
        }
        public PointF GetCenter() { return center; }

        private void SearchY() // Высчитывает наивысшую и наименьшую точку нашей фигуры
        { 
            for (int i = 0; i < VertexList.Count(); i++)
            {
                if (Ymax < VertexList[i].Y)
                    Ymax = (int)VertexList[i].Y;
                if (Ymin > VertexList[i].Y)
                    Ymin = (int)VertexList[i].Y;
            }
        }
        public void Fill(Graphics g, Pen DrawPen)
        {
            if(only_one == 0)   SearchY(); only_one++;
            int k;
            for (int value = (int)Ymin; value < Ymax; value++) // проход по фигуре в координате Y
            {
                List<int> point = new List<int>();
                for (int i = 0; i < VertexList.Count; i++)
                {
                    if (i < VertexList.Count - 1) k = i + 1; else k = 0;
                    if ((VertexList[i].Y < value && VertexList[k].Y >= value) || (VertexList[i].Y >= value && VertexList[k].Y < value))
                    {
                        int x1;
                        double x = ((VertexList[k].X - VertexList[i].X) * (value - VertexList[i].Y) / (VertexList[k].Y - VertexList[i].Y)) + VertexList[i].X; // высчитывание точки пересечения
                        
                        if ((point.Count % 2 != 0) || point.Count != 0) x1 = (int)Math.Ceiling(x); else x1 = (int)Math.Floor(x);

                        point.Add(x1);
                    }
                }
                point.Sort();
                for (int i = 0; i < point.Count(); i += 2) // зарисовываем фигуру по найденым точкам
                    g.DrawLine(DrawPen, new Point(point[i], value), new Point(point[i + 1], value));
            }
            
        }

        // выделение многоугольника
        public bool ThisPgn(int mX, int mY)
        {
            int n = VertexList.Count() - 1, k = 0, m = 0;
            PointF Pi, Pk; 
            bool check = false;
            for (int i = 0; i <= n; i++)
            {
                if (i < n) k = i + 1; else k = 0;
                Pi = VertexList[i]; Pk = VertexList[k];
                if ((Pi.Y < mY) & (Pk.Y >= mY) | (Pi.Y >= mY) & (Pk.Y < mY))
                    if ((mY - Pi.Y) * (Pk.X - Pi.X) / (Pk.Y - Pi.Y) + Pi.X < mX) m++;
            }
            if (m % 2 == 1) check = true;
            return check;
        }

        private double[,] Multiplication(double[,] a, double[,] b) // умножение матриц
        {
            double[,] r = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    for (int k = 0; k < b.GetLength(0); k++)
                    {
                        r[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return r;
        }
        // плоско-параллельное перемещение
        public void Move(double dx, double dy)
        {
            int n = VertexList.Count() - 1;
            PointF fP = new PointF();
            double[,] A = new double[1,3];
            double[,] B = new double[3,3];
            for(int i = 0; i < B.GetLength(0);i++)
            {
                for(int j = 0;j < B.GetLength(1);j++)
                {
                    if (i - j == 0) B[i, j] = 1;
                    if (i == 2)
                        if (j == 0) B[i, j] = dx; else if (j == 1) B[i, j] = dy;
                }
            }
            for (int i = 0; i <= n; i++)
            {
                A[0, 0] = VertexList[i].X; A[0, 1] = VertexList[i].Y; A[0, 2] = 1;
                double[,] result = Multiplication(A, B);
                fP.X = (float)result[0,0]; fP.Y = (float)result[0, 1];
                VertexList[i] = fP;
            }
            SearchY();
        }

        public void Rotate(double angle) // поворот фигуры
        {
            int n = VertexList.Count() - 1;
            PointF fP = new PointF();
            double[,] A = new double[1, 3];
            double[,] B = new double[3, 3];
            B[2, 2] = 1;
            B[0, 0] = B[1,1] = Math.Cos(angle);
            B[0, 1] = -Math.Sin(angle);
            B[1, 0] = Math.Sin(angle);

            for (int i = 0; i <= n; i++)
            {
                A[0, 0] = VertexList[i].X; A[0, 1] = VertexList[i].Y; A[0, 2] = 1;
                double[,] result = Multiplication(A, B);
                fP.X = (float)result[0, 0]; fP.Y = (float)result[0, 1];
                VertexList[i] = fP;
            }
            SearchY();
        }

        public void Scale(double scale)// изменение размеров фигуры
        {
            int n = VertexList.Count() - 1;
            PointF fP = new PointF();
            double[,] A = new double[1, 3];
            double[,] B = new double[3, 3];
            B[0, 0] = B[1,1] = scale; B[2, 2] = 1;
            for (int i = 0; i <= n; i++)
            {
                A[0, 0] = VertexList[i].X; A[0, 1] = VertexList[i].Y; A[0, 2] = 1;
                double[,] result = Multiplication(A, B);
                fP.X = (float)result[0, 0]; fP.Y = (float)result[0, 1];
                VertexList[i] = fP;
            }
            SearchY();
        }
        public void Clear()
        {
            VertexList.Clear();
        }

    }
}
