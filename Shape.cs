using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace VectorEditor
{
    // абстрактный класс для всех фигур
    public abstract class Shape
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Color Color { get; set; }
        public int PenWidth { get; set; }
        public bool Selected { get; set; }
        public Color FillColor { get; set; }
        public bool IsFilled { get; set; }

        public Shape()
        {
            Color = Color.Black;
            PenWidth = 2;
            Selected = false;
            FillColor = Color.White;
            IsFilled = false;
        }

        public abstract void Draw(Graphics g);
        public abstract bool Contains(Point point);
        public abstract void Move(int deltaX, int deltaY);
        public abstract Shape Clone();

        public virtual void DrawSelectionHandles(Graphics g)
        {
            if (Selected)
            {
                Rectangle rect = GetBounds();
                rect.Inflate(5, 5);
                
                // Рисуем рамку выделения
                using (Pen pen = new Pen(Color.Blue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        public abstract Rectangle GetBounds();
    }
    
    // класс для линий
    public class LineShape : Shape
    {
        public override void Draw(Graphics g)
        {
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawLine(pen, StartPoint, EndPoint);
            }
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            // Проверка, находится ли точка достаточно близко к линии
            const int tolerance = 5;
            
            float dx = EndPoint.X - StartPoint.X;
            float dy = EndPoint.Y - StartPoint.Y;
            
            if (dx == 0 && dy == 0) // Точка
                return Math.Abs(point.X - StartPoint.X) <= tolerance && 
                       Math.Abs(point.Y - StartPoint.Y) <= tolerance;
            
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float d = Math.Abs((point.X - StartPoint.X) * dy - (point.Y - StartPoint.Y) * dx) / length;
            
            if (d <= tolerance)
            {
                // Проверка, находится ли точка между начальной и конечной точками линии
                float dotProduct = ((point.X - StartPoint.X) * dx + (point.Y - StartPoint.Y) * dy) / (dx * dx + dy * dy);
                return dotProduct >= 0 && dotProduct <= 1;
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new LineShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Класс для прямоугольников
    public class RectangleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetBounds();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillRectangle(brush, rect);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawRectangle(pen, rect);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            // Проверяем, находится ли точка рядом с границей прямоугольника
            Rectangle rect = GetBounds();
            rect.Inflate(5, 5);
            Rectangle innerRect = GetBounds();
            innerRect.Inflate(-5, -5);
            
            return rect.Contains(point) && !innerRect.Contains(point);
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new RectangleShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Класс для эллипсов
    public class EllipseShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetBounds();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillEllipse(brush, rect);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawEllipse(pen, rect);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            // Проверка, находится ли точка рядом с границей эллипса
            Rectangle rect = GetBounds();
            int a = rect.Width / 2;
            int b = rect.Height / 2;
            
            if (a == 0 || b == 0)
                return false;
            
            Point center = new Point(rect.X + a, rect.Y + b);
            double normalizedX = (double)(point.X - center.X) / a;
            double normalizedY = (double)(point.Y - center.Y) / b;
            double distance = normalizedX * normalizedX + normalizedY * normalizedY;
            
            return Math.Abs(distance - 1.0) <= 0.1; // Допуск для выбора границы эллипса
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new EllipseShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Класс для треугольников
    public class TriangleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Point[] points = GetTrianglePoints();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillPolygon(brush, points);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawPolygon(pen, points);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetTrianglePoints()
        {
            // Создаем треугольник с вершиной наверху и двумя точками внизу
            Rectangle bounds = GetBounds();
            int centerX = bounds.X + bounds.Width / 2;
            
            Point[] points = new Point[3];
            points[0] = new Point(centerX, bounds.Y); // Вершина
            points[1] = new Point(bounds.X, bounds.Y + bounds.Height); // Левый нижний угол
            points[2] = new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height); // Правый нижний угол
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetTrianglePoints();
            
            // Проверяем, находится ли точка рядом с любой из сторон треугольника
            const int tolerance = 5;
            
            for (int i = 0; i < 3; i++)
            {
                Point p1 = points[i];
                Point p2 = points[(i + 1) % 3];
                
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                
                if (dx == 0 && dy == 0) continue;
                
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                float d = Math.Abs((point.X - p1.X) * dy - (point.Y - p1.Y) * dx) / length;
                
                if (d <= tolerance)
                {
                    // Проверка, находится ли точка между начальной и конечной точками отрезка
                    float dotProduct = ((point.X - p1.X) * dx + (point.Y - p1.Y) * dy) / (dx * dx + dy * dy);
                    if (dotProduct >= 0 && dotProduct <= 1)
                        return true;
                }
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new TriangleShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Класс для кругов
    public class CircleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetCircleBounds();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillEllipse(brush, rect);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawEllipse(pen, rect);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        // Новый метод для получения границ круга
        private Rectangle GetCircleBounds()
        {
            // Вычисляем центр как начальную точку
            Point center = StartPoint;
            
            // Вычисляем радиус как расстояние между начальной и конечной точкой
            double radius = Math.Sqrt(
                Math.Pow(EndPoint.X - StartPoint.X, 2) + 
                Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            
            // Создаем квадрат с центром в StartPoint и размером 2*radius
            return new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                (int)(radius * 2),
                (int)(radius * 2));
        }

        public override bool Contains(Point point)
        {
            // Используем центр и радиус для проверки
            Point center = StartPoint;
            double radius = Math.Sqrt(
                Math.Pow(EndPoint.X - StartPoint.X, 2) + 
                Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            
            if (radius == 0)
                return false;
            
            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
            
            return Math.Abs(distance - radius) <= 5; // Допуск 5 пикселей
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new CircleShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            return GetCircleBounds();
        }
    }

    // Класс для многоугольников
    public class PolygonShape : Shape
    {
        public int Sides { get; set; }

        public PolygonShape(int sides = 5)
        {
            Sides = Math.Max(3, sides); // Минимум 3 стороны
        }

        public override void Draw(Graphics g)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillPolygon(brush, points);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawPolygon(pen, points);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetPolygonPoints()
        {
            // Используем прямоугольник от начального угла до конечной точки
            // точно так же, как и у других фигур (эллипс, прямоугольник)
            Rectangle rect = GetBounds();
            
            // Тот же подход, что и с рисованием эллипса - вписываем в прямоугольник
            // между StartPoint и EndPoint
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2;
            
            Point[] points = new Point[Sides];
            
            for (int i = 0; i < Sides; i++)
            {
                double angle = 2 * Math.PI * i / Sides - Math.PI / 2; // Начинаем с верхней точки
                int x = (int)(centerX + radius * Math.Cos(angle));
                int y = (int)(centerY + radius * Math.Sin(angle));
                points[i] = new Point(x, y);
            }
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetPolygonPoints();
            
            // Проверяем, находится ли точка рядом с любой из сторон многоугольника
            const int tolerance = 5;
            
            for (int i = 0; i < points.Length; i++)
            {
                Point p1 = points[i];
                Point p2 = points[(i + 1) % points.Length];
                
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                
                if (dx == 0 && dy == 0) continue;
                
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                float d = Math.Abs((point.X - p1.X) * dy - (point.Y - p1.Y) * dx) / length;
                
                if (d <= tolerance)
                {
                    // Проверка, находится ли точка между начальной и конечной точками отрезка
                    float dotProduct = ((point.X - p1.X) * dx + (point.Y - p1.Y) * dy) / (dx * dx + dy * dy);
                    if (dotProduct >= 0 && dotProduct <= 1)
                        return true;
                }
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new PolygonShape(Sides)
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Новый класс для правильных многоугольников, которые будут рисоваться как эллипс - от угла
    public class RegularPolygonShape : Shape
    {
        public int Sides { get; set; }

        public RegularPolygonShape(int sides = 5)
        {
            Sides = Math.Max(3, sides); // Минимум 3 стороны
        }

        public override void Draw(Graphics g)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled)
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillPolygon(brush, points);
                }
            }
            
            using (Pen pen = new Pen(Color, PenWidth))
            {
                g.DrawPolygon(pen, points);
            }
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetPolygonPoints()
        {
            Rectangle rect = GetBounds();
            
            // Используем вписанный многоугольник в эллипс, который находится в прямоугольнике
            // Это обеспечит поведение как у эллипса - от угла, а не от центра
            float centerX = rect.X + rect.Width / 2f;
            float centerY = rect.Y + rect.Height / 2f;
            float radiusX = rect.Width / 2f;
            float radiusY = rect.Height / 2f;
            
            Point[] points = new Point[Sides];
            
            for (int i = 0; i < Sides; i++)
            {
                // Для многоугольника, вписанного в эллипс
                double angle = 2 * Math.PI * i / Sides - Math.PI / 2; // Начинаем с верхней точки
                float x = centerX + radiusX * (float)Math.Cos(angle);
                float y = centerY + radiusY * (float)Math.Sin(angle);
                points[i] = new Point((int)x, (int)y);
            }
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetPolygonPoints();
            
            // Проверяем, находится ли точка рядом с любой из сторон многоугольника
            const int tolerance = 5;
            
            for (int i = 0; i < points.Length; i++)
            {
                Point p1 = points[i];
                Point p2 = points[(i + 1) % points.Length];
                
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                
                if (dx == 0 && dy == 0) continue;
                
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                float d = Math.Abs((point.X - p1.X) * dy - (point.Y - p1.Y) * dx) / length;
                
                if (d <= tolerance)
                {
                    // Проверка, находится ли точка между начальной и конечной точками отрезка
                    float dotProduct = ((point.X - p1.X) * dx + (point.Y - p1.Y) * dy) / (dx * dx + dy * dy);
                    if (dotProduct >= 0 && dotProduct <= 1)
                        return true;
                }
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public override Shape Clone()
        {
            return new RegularPolygonShape(Sides)
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
        }

        public override Rectangle GetBounds()
        {
            int x = Math.Min(StartPoint.X, EndPoint.X);
            int y = Math.Min(StartPoint.Y, EndPoint.Y);
            int width = Math.Abs(EndPoint.X - StartPoint.X);
            int height = Math.Abs(EndPoint.Y - StartPoint.Y);
            
            return new Rectangle(x, y, width, height);
        }
    }

    // Класс для кривой Безье
    public class BezierShape : Shape
    {
        // Список сегментов кривой Безье
        public class BezierSegment
        {
            public Point StartPoint { get; set; }
            public Point EndPoint { get; set; }
            public Point ControlPoint1 { get; set; }
            public Point ControlPoint2 { get; set; }

            public BezierSegment(Point start, Point end)
            {
                StartPoint = start;
                EndPoint = end;
                // По умолчанию контрольные точки находятся на линии между начальной и конечной точками,
                // чтобы кривая Безье была прямой линией до изменения контрольных точек пользователем
                ControlPoint1 = new Point(
                    (start.X * 2 + end.X) / 3,
                    (start.Y * 2 + end.Y) / 3);
                ControlPoint2 = new Point(
                    (start.X + end.X * 2) / 3,
                    (start.Y + end.Y * 2) / 3);
            }
        }

        // Константы для размера контрольных точек
        private const int CONTROL_POINT_SIZE = 8;
        private const int ANCHOR_POINT_SIZE = 10;

        public List<BezierSegment> Segments { get; private set; } = new List<BezierSegment>();
        
        // Текущий редактируемый сегмент
        public BezierSegment CurrentSegment { get; set; }
        
        // Флаги для отслеживания состояния редактирования
        public bool IsEndPointSet { get; set; } = false;
        public bool IsControlPoint1Set { get; set; } = false;
        public bool IsControlPoint2Set { get; set; } = false;

        // Текущая точка, которую перетаскивает пользователь
        public enum DragPoint { None, Start, End, Control1, Control2 }
        private DragPoint currentDragPoint = DragPoint.None;
        private int currentDragSegmentIndex = -1;

        public override void Draw(Graphics g)
        {
            // Рисуем все завершенные сегменты
            using (Pen pen = new Pen(Color, PenWidth))
            {
                foreach (var segment in Segments)
                {
                    g.DrawBezier(pen, segment.StartPoint, segment.ControlPoint1, segment.ControlPoint2, segment.EndPoint);
                }
            }
            
            // Рисуем текущий редактируемый сегмент, если он есть
            if (CurrentSegment != null)
            {
                using (Pen pen = new Pen(Color, PenWidth))
                {
                    g.DrawBezier(pen, CurrentSegment.StartPoint, CurrentSegment.ControlPoint1, 
                                CurrentSegment.ControlPoint2, CurrentSegment.EndPoint);
                }
            }

            // Рисуем контрольные точки и линии, если фигура выбрана или если сейчас редактируется
            if (Selected || CurrentSegment != null)
            {
                using (SolidBrush startPointBrush = new SolidBrush(Color.Blue))
                using (SolidBrush endPointBrush = new SolidBrush(Color.Green))
                using (SolidBrush controlPointBrush = new SolidBrush(Color.Red))
                using (Pen controlLinePen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    foreach (var segment in Segments)
                    {
                        // Рисуем контрольные линии
                        g.DrawLine(controlLinePen, segment.StartPoint, segment.ControlPoint1);
                        g.DrawLine(controlLinePen, segment.EndPoint, segment.ControlPoint2);
                        
                        // Рисуем контрольные точки
                        g.FillEllipse(startPointBrush, segment.StartPoint.X - ANCHOR_POINT_SIZE/2, 
                                    segment.StartPoint.Y - ANCHOR_POINT_SIZE/2, ANCHOR_POINT_SIZE, ANCHOR_POINT_SIZE);
                        g.FillEllipse(endPointBrush, segment.EndPoint.X - ANCHOR_POINT_SIZE/2, 
                                    segment.EndPoint.Y - ANCHOR_POINT_SIZE/2, ANCHOR_POINT_SIZE, ANCHOR_POINT_SIZE);
                        g.FillEllipse(controlPointBrush, segment.ControlPoint1.X - CONTROL_POINT_SIZE/2, 
                                    segment.ControlPoint1.Y - CONTROL_POINT_SIZE/2, CONTROL_POINT_SIZE, CONTROL_POINT_SIZE);
                        g.FillEllipse(controlPointBrush, segment.ControlPoint2.X - CONTROL_POINT_SIZE/2, 
                                    segment.ControlPoint2.Y - CONTROL_POINT_SIZE/2, CONTROL_POINT_SIZE, CONTROL_POINT_SIZE);
                    }
                    
                    // Рисуем контрольные точки для текущего сегмента, если он есть
                    if (CurrentSegment != null)
                    {
                        g.DrawLine(controlLinePen, CurrentSegment.StartPoint, CurrentSegment.ControlPoint1);
                        g.DrawLine(controlLinePen, CurrentSegment.EndPoint, CurrentSegment.ControlPoint2);
                        
                        g.FillEllipse(startPointBrush, CurrentSegment.StartPoint.X - ANCHOR_POINT_SIZE/2, 
                                    CurrentSegment.StartPoint.Y - ANCHOR_POINT_SIZE/2, ANCHOR_POINT_SIZE, ANCHOR_POINT_SIZE);
                        g.FillEllipse(endPointBrush, CurrentSegment.EndPoint.X - ANCHOR_POINT_SIZE/2, 
                                    CurrentSegment.EndPoint.Y - ANCHOR_POINT_SIZE/2, ANCHOR_POINT_SIZE, ANCHOR_POINT_SIZE);
                        g.FillEllipse(controlPointBrush, CurrentSegment.ControlPoint1.X - CONTROL_POINT_SIZE/2, 
                                    CurrentSegment.ControlPoint1.Y - CONTROL_POINT_SIZE/2, CONTROL_POINT_SIZE, CONTROL_POINT_SIZE);
                        g.FillEllipse(controlPointBrush, CurrentSegment.ControlPoint2.X - CONTROL_POINT_SIZE/2, 
                                    CurrentSegment.ControlPoint2.Y - CONTROL_POINT_SIZE/2, CONTROL_POINT_SIZE, CONTROL_POINT_SIZE);
                    }
                }
            }

            // Дополнительно рисуем селекторную рамку, если выбрана
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        // Проверка, находится ли курсор над контрольной точкой
        public bool IsOverControlPoint(Point mousePoint, out int segmentIndex, out DragPoint pointType)
        {
            segmentIndex = -1;
            pointType = DragPoint.None;
            
            // Проверяем точки в завершенных сегментах
            for (int i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];
                
                // Проверяем начальную точку
                if (IsPointNear(mousePoint, segment.StartPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Start;
                    return true;
                }
                
                // Проверяем конечную точку
                if (IsPointNear(mousePoint, segment.EndPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.End;
                    return true;
                }
                
                // Проверяем контрольную точку 1
                if (IsPointNear(mousePoint, segment.ControlPoint1, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Control1;
                    return true;
                }
                
                // Проверяем контрольную точку 2
                if (IsPointNear(mousePoint, segment.ControlPoint2, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Control2;
                    return true;
                }
            }
            
            // Проверяем точки в текущем редактируемом сегменте
            if (CurrentSegment != null)
            {
                // Проверяем начальную точку
                if (IsPointNear(mousePoint, CurrentSegment.StartPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Start;
                    return true;
                }
                
                // Проверяем конечную точку
                if (IsPointNear(mousePoint, CurrentSegment.EndPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.End;
                    return true;
                }
                
                // Проверяем контрольную точку 1
                if (IsPointNear(mousePoint, CurrentSegment.ControlPoint1, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Control1;
                    return true;
                }
                
                // Проверяем контрольную точку 2
                if (IsPointNear(mousePoint, CurrentSegment.ControlPoint2, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Control2;
                    return true;
                }
            }
            
            return false;
        }
        
        // Начать перетаскивание контрольной точки
        public void StartDraggingControlPoint(int segmentIndex, DragPoint pointType)
        {
            currentDragSegmentIndex = segmentIndex;
            currentDragPoint = pointType;
        }
        
        // Остановить перетаскивание
        public void StopDragging()
        {
            currentDragSegmentIndex = -1;
            currentDragPoint = DragPoint.None;
        }
        
        // Перетащить выбранную контрольную точку
        public void DragControlPoint(Point newPosition)
        {
            if (currentDragPoint == DragPoint.None || currentDragSegmentIndex < 0)
                return;
                
            BezierSegment segment;
            if (currentDragSegmentIndex < Segments.Count)
                segment = Segments[currentDragSegmentIndex];
            else if (CurrentSegment != null)
                segment = CurrentSegment;
            else
                return;
                
            switch (currentDragPoint)
            {
                case DragPoint.Start:
                    segment.StartPoint = newPosition;
                    break;
                case DragPoint.End:
                    segment.EndPoint = newPosition;
                    break;
                case DragPoint.Control1:
                    segment.ControlPoint1 = newPosition;
                    break;
                case DragPoint.Control2:
                    segment.ControlPoint2 = newPosition;
                    break;
            }
            
            // Обновляем StartPoint и EndPoint для совместимости с базовым классом
            if (currentDragSegmentIndex == 0 && currentDragPoint == DragPoint.Start)
                StartPoint = newPosition;
                
            if (currentDragSegmentIndex == Segments.Count - 1 && currentDragPoint == DragPoint.End)
                EndPoint = newPosition;
        }
        
        // Проверка, находится ли точка рядом с другой точкой
        private bool IsPointNear(Point p1, Point p2, int radius)
        {
            return Math.Abs(p1.X - p2.X) <= radius && Math.Abs(p1.Y - p2.Y) <= radius;
        }

        public override bool Contains(Point point)
        {
            // Сначала проверяем, находится ли точка рядом с контрольной точкой
            int segmentIndex;
            DragPoint pointType;
            if (IsOverControlPoint(point, out segmentIndex, out pointType))
                return true;
                
            // Проверка, находится ли точка достаточно близко к любому сегменту кривой Безье
            const int STEPS = 50; // Количество сегментов для проверки
            const double TOLERANCE = 5.0; // Допуск в пикселях

            // Проверяем все сегменты
            foreach (var segment in Segments)
            {
                // Находим точки на кривой Безье и проверяем расстояние
                for (int i = 0; i < STEPS; i++)
                {
                    double t1 = (double)i / STEPS;
                    double t2 = (double)(i + 1) / STEPS;
                    
                    Point p1 = CalculateBezierPoint(segment, t1);
                    Point p2 = CalculateBezierPoint(segment, t2);
                    
                    // Проверяем расстояние от точки до отрезка
                    double dist = DistanceToLine(point, p1, p2);
                    if (dist <= TOLERANCE)
                        return true;
                }
            }
            
            // Проверяем текущий редактируемый сегмент
            if (CurrentSegment != null)
            {
                for (int i = 0; i < STEPS; i++)
                {
                    double t1 = (double)i / STEPS;
                    double t2 = (double)(i + 1) / STEPS;
                    
                    Point p1 = CalculateBezierPoint(CurrentSegment, t1);
                    Point p2 = CalculateBezierPoint(CurrentSegment, t2);
                    
                    double dist = DistanceToLine(point, p1, p2);
                    if (dist <= TOLERANCE)
                        return true;
                }
            }
            
            return false;
        }

        // Вычисление точки на кривой Безье для заданного параметра t (0..1)
        private Point CalculateBezierPoint(BezierSegment segment, double t)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;
            
            double x = uuu * segment.StartPoint.X + 3 * uu * t * segment.ControlPoint1.X + 
                       3 * u * tt * segment.ControlPoint2.X + ttt * segment.EndPoint.X;
            double y = uuu * segment.StartPoint.Y + 3 * uu * t * segment.ControlPoint1.Y + 
                       3 * u * tt * segment.ControlPoint2.Y + ttt * segment.EndPoint.Y;
            
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        // Вычисление расстояния от точки до отрезка
        private double DistanceToLine(Point p, Point start, Point end)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            
            if (dx == 0 && dy == 0) // Точка
                return Math.Sqrt(Math.Pow(p.X - start.X, 2) + Math.Pow(p.Y - start.Y, 2));
            
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float d = Math.Abs((p.X - start.X) * dy - (p.Y - start.Y) * dx) / length;
            
            // Проверяем, находится ли проекция точки на линии
            float dotProduct = ((p.X - start.X) * dx + (p.Y - start.Y) * dy) / (dx * dx + dy * dy);
            
            if (dotProduct < 0) // Ближайшая точка - начало линии
                return Math.Sqrt(Math.Pow(p.X - start.X, 2) + Math.Pow(p.Y - start.Y, 2));
            else if (dotProduct > 1) // Ближайшая точка - конец линии
                return Math.Sqrt(Math.Pow(p.X - end.X, 2) + Math.Pow(p.Y - end.Y, 2));
            else // Ближайшая точка находится на линии
                return d;
        }

        public override void Move(int deltaX, int deltaY)
        {
            // Перемещаем все точки во всех сегментах
            foreach (var segment in Segments)
            {
                segment.StartPoint = new Point(segment.StartPoint.X + deltaX, segment.StartPoint.Y + deltaY);
                segment.EndPoint = new Point(segment.EndPoint.X + deltaX, segment.EndPoint.Y + deltaY);
                segment.ControlPoint1 = new Point(segment.ControlPoint1.X + deltaX, segment.ControlPoint1.Y + deltaY);
                segment.ControlPoint2 = new Point(segment.ControlPoint2.X + deltaX, segment.ControlPoint2.Y + deltaY);
            }
            
            if (CurrentSegment != null)
            {
                CurrentSegment.StartPoint = new Point(CurrentSegment.StartPoint.X + deltaX, CurrentSegment.StartPoint.Y + deltaY);
                CurrentSegment.EndPoint = new Point(CurrentSegment.EndPoint.X + deltaX, CurrentSegment.EndPoint.Y + deltaY);
                CurrentSegment.ControlPoint1 = new Point(CurrentSegment.ControlPoint1.X + deltaX, CurrentSegment.ControlPoint1.Y + deltaY);
                CurrentSegment.ControlPoint2 = new Point(CurrentSegment.ControlPoint2.X + deltaX, CurrentSegment.ControlPoint2.Y + deltaY);
            }
            
            // Обновляем StartPoint и EndPoint для совместимости с базовым классом
            if (Segments.Count > 0)
            {
                StartPoint = Segments[0].StartPoint;
                if (CurrentSegment != null)
                    EndPoint = CurrentSegment.EndPoint;
                else if (Segments.Count > 0)
                    EndPoint = Segments[Segments.Count - 1].EndPoint;
            }
            else if (CurrentSegment != null)
            {
                StartPoint = CurrentSegment.StartPoint;
                EndPoint = CurrentSegment.EndPoint;
            }
        }

        public override Shape Clone()
        {
            BezierShape clone = new BezierShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
            
            // Копируем все сегменты
            foreach (var segment in this.Segments)
            {
                BezierSegment clonedSegment = new BezierSegment(segment.StartPoint, segment.EndPoint);
                clonedSegment.ControlPoint1 = segment.ControlPoint1;
                clonedSegment.ControlPoint2 = segment.ControlPoint2;
                clone.Segments.Add(clonedSegment);
            }
            
            // Копируем текущий сегмент, если он есть
            if (this.CurrentSegment != null)
            {
                clone.CurrentSegment = new BezierSegment(this.CurrentSegment.StartPoint, this.CurrentSegment.EndPoint);
                clone.CurrentSegment.ControlPoint1 = this.CurrentSegment.ControlPoint1;
                clone.CurrentSegment.ControlPoint2 = this.CurrentSegment.ControlPoint2;
                clone.IsEndPointSet = this.IsEndPointSet;
                clone.IsControlPoint1Set = this.IsControlPoint1Set;
                clone.IsControlPoint2Set = this.IsControlPoint2Set;
            }
            
            return clone;
        }

        public override Rectangle GetBounds()
        {
            if (Segments.Count == 0 && CurrentSegment == null)
                return new Rectangle(0, 0, 0, 0);
            
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            
            // Учитываем все точки всех сегментов
            foreach (var segment in Segments)
            {
                minX = Math.Min(minX, Math.Min(Math.Min(segment.StartPoint.X, segment.EndPoint.X), 
                                         Math.Min(segment.ControlPoint1.X, segment.ControlPoint2.X)));
                minY = Math.Min(minY, Math.Min(Math.Min(segment.StartPoint.Y, segment.EndPoint.Y), 
                                         Math.Min(segment.ControlPoint1.Y, segment.ControlPoint2.Y)));
                maxX = Math.Max(maxX, Math.Max(Math.Max(segment.StartPoint.X, segment.EndPoint.X), 
                                         Math.Max(segment.ControlPoint1.X, segment.ControlPoint2.X)));
                maxY = Math.Max(maxY, Math.Max(Math.Max(segment.StartPoint.Y, segment.EndPoint.Y), 
                                         Math.Max(segment.ControlPoint1.Y, segment.ControlPoint2.Y)));
            }
            
            // Учитываем текущий сегмент, если он есть
            if (CurrentSegment != null)
            {
                minX = Math.Min(minX, Math.Min(Math.Min(CurrentSegment.StartPoint.X, CurrentSegment.EndPoint.X), 
                                         Math.Min(CurrentSegment.ControlPoint1.X, CurrentSegment.ControlPoint2.X)));
                minY = Math.Min(minY, Math.Min(Math.Min(CurrentSegment.StartPoint.Y, CurrentSegment.EndPoint.Y), 
                                         Math.Min(CurrentSegment.ControlPoint1.Y, CurrentSegment.ControlPoint2.Y)));
                maxX = Math.Max(maxX, Math.Max(Math.Max(CurrentSegment.StartPoint.X, CurrentSegment.EndPoint.X), 
                                         Math.Max(CurrentSegment.ControlPoint1.X, CurrentSegment.ControlPoint2.X)));
                maxY = Math.Max(maxY, Math.Max(Math.Max(CurrentSegment.StartPoint.Y, CurrentSegment.EndPoint.Y), 
                                         Math.Max(CurrentSegment.ControlPoint1.Y, CurrentSegment.ControlPoint2.Y)));
            }
            
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        
        // Добавление нового сегмента к кривой
        public void AddSegment(Point startPoint, Point endPoint)
        {
            BezierSegment segment = new BezierSegment(startPoint, endPoint);
            Segments.Add(segment);
            
            // Обновляем StartPoint и EndPoint для совместимости с базовым классом
            if (Segments.Count == 1)
                StartPoint = startPoint;
            
            EndPoint = endPoint;
        }
        
        // Завершение редактирования текущего сегмента и добавление его в список
        public void FinishCurrentSegment()
        {
            if (CurrentSegment != null && IsControlPoint2Set)
            {
                Segments.Add(CurrentSegment);
                EndPoint = CurrentSegment.EndPoint;
            }
        }
    }

    // Класс для ломаной линии
    public class PolylineShape : Shape
    {
        // Список точек ломаной линии
        public List<Point> Points { get; private set; } = new List<Point>();

        public override void Draw(Graphics g)
        {
            // Рисуем ломаную линию, если есть хотя бы 2 точки
            if (Points.Count >= 2)
            {
                using (Pen pen = new Pen(Color, PenWidth))
                {
                    g.DrawLines(pen, Points.ToArray());
                }
            }
            
            if (Selected)
            {
                DrawSelectionHandles(g);
                
                // Показываем узлы ломаной линии
                using (SolidBrush vertexBrush = new SolidBrush(Color.Blue))
                {
                    foreach (Point p in Points)
                    {
                        g.FillEllipse(vertexBrush, p.X - 3, p.Y - 3, 6, 6);
                    }
                }
            }
        }

        public override bool Contains(Point point)
        {
            const int tolerance = 5;
            
            // Проверяем расстояние до каждого сегмента линии
            for (int i = 0; i < Points.Count - 1; i++)
            {
                Point p1 = Points[i];
                Point p2 = Points[i + 1];
                
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                
                if (dx == 0 && dy == 0) continue;
                
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                float d = Math.Abs((point.X - p1.X) * dy - (point.Y - p1.Y) * dx) / length;
                
                if (d <= tolerance)
                {
                    // Проверка, находится ли точка между начальной и конечной точками отрезка
                    float dotProduct = ((point.X - p1.X) * dx + (point.Y - p1.Y) * dy) / (dx * dx + dy * dy);
                    if (dotProduct >= 0 && dotProduct <= 1)
                        return true;
                }
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            // Перемещаем все точки
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(Points[i].X + deltaX, Points[i].Y + deltaY);
            }
            
            // Также обновляем StartPoint и EndPoint для совместимости
            if (Points.Count > 0)
            {
                StartPoint = Points[0];
                if (Points.Count > 1)
                    EndPoint = Points[Points.Count - 1];
            }
        }

        public override Shape Clone()
        {
            PolylineShape clone = new PolylineShape
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                Color = this.Color,
                PenWidth = this.PenWidth,
                Selected = this.Selected
            };
            
            // Копируем все точки
            foreach (Point p in this.Points)
            {
                clone.Points.Add(p);
            }
            
            return clone;
        }

        public override Rectangle GetBounds()
        {
            if (Points.Count == 0)
                return new Rectangle(0, 0, 0, 0);
            
            // Находим границы, учитывая все точки
            int minX = Points[0].X;
            int minY = Points[0].Y;
            int maxX = Points[0].X;
            int maxY = Points[0].Y;
            
            foreach (Point p in Points)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }
            
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        
        // Добавление новой точки в ломаную
        public void AddPoint(Point point)
        {
            Points.Add(point);
            
            // Обновляем StartPoint и EndPoint для совместимости
            if (Points.Count == 1)
                StartPoint = point;
            
            EndPoint = point;
        }
    }
} 