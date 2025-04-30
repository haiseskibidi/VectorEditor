using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace VectorEditor
{
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
                
                Color dashColor = Color.Blue;
                int dashLen = 5;
                
                for (int x = rect.Left; x < rect.Right; x += 2 * dashLen)
                {
                    DrawLine(g, x, rect.Top, Math.Min(x + dashLen, rect.Right), rect.Top, dashColor, 1);
                }
                
                for (int y = rect.Top; y < rect.Bottom; y += 2 * dashLen)
                {
                    DrawLine(g, rect.Right, y, rect.Right, Math.Min(y + dashLen, rect.Bottom), dashColor, 1);
                }
                
                for (int x = rect.Right; x > rect.Left; x -= 2 * dashLen)
                {
                    DrawLine(g, x, rect.Bottom, Math.Max(x - dashLen, rect.Left), rect.Bottom, dashColor, 1);
                }
                
                for (int y = rect.Bottom; y > rect.Top; y -= 2 * dashLen)
                {
                    DrawLine(g, rect.Left, y, rect.Left, Math.Max(y - dashLen, rect.Top), dashColor, 1);
                }
            }
        }

        public abstract Rectangle GetBounds();

        protected void DrawPixel(Graphics g, int x, int y, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, x, y, 1, 1);
            }
        }
        
        protected void DrawLine(Graphics g, int x0, int y0, int x1, int y1, Color color, int penWidth)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                for (int w = -penWidth / 2; w <= penWidth / 2; w++)
                {
                    for (int h = -penWidth / 2; h <= penWidth / 2; h++)
                    {
                        DrawPixel(g, x0 + w, y0 + h, color);
                    }
                }
                
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
        
        protected void FillRectangle(Graphics g, Rectangle rect, Color color)
        {
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    DrawPixel(g, x, y, color);
                }
            }
        }
        
        protected void DrawRectangle(Graphics g, Rectangle rect, Color color, int penWidth)
        {
            DrawLine(g, rect.Left, rect.Top, rect.Right - 1, rect.Top, color, penWidth);
            DrawLine(g, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1, color, penWidth);
            DrawLine(g, rect.Right - 1, rect.Bottom - 1, rect.Left, rect.Bottom - 1, color, penWidth);
            DrawLine(g, rect.Left, rect.Bottom - 1, rect.Left, rect.Top, color, penWidth);
        }
        
        protected void DrawEllipse(Graphics g, Rectangle rect, Color color, int penWidth)
        {
            int a = rect.Width / 2;
            int b = rect.Height / 2;
            int centerX = rect.X + a;
            int centerY = rect.Y + b;
            
            if (a <= 0 || b <= 0) return;
            
            long a2 = a * a;
            long b2 = b * b;
            
            long x = 0;
            long y = b;
            long sigma = 2 * b2 + a2 * (1 - 2 * b);
            
            while (b2 * x <= a2 * y)
            {
                DrawEllipsePoints(g, centerX, centerY, (int)x, (int)y, color, penWidth);
                
                if (sigma >= 0)
                {
                    sigma += 4 * a2 * (1 - y);
                    y--;
                }
                
                sigma += b2 * (4 * x + 6);
                x++;
            }
            
            x = a;
            y = 0;
            sigma = 2 * a2 + b2 * (1 - 2 * a);
            
            while (a2 * y <= b2 * x)
            {
                DrawEllipsePoints(g, centerX, centerY, (int)x, (int)y, color, penWidth);
                
                if (sigma >= 0)
                {
                    sigma += 4 * b2 * (1 - x);
                    x--;
                }
                
                sigma += a2 * (4 * y + 6);
                y++;
            }
        }
        
        private void DrawEllipsePoints(Graphics g, int cx, int cy, int x, int y, Color color, int penWidth)
        {
            for (int px = -penWidth/2; px <= penWidth/2; px++)
            {
                for (int py = -penWidth/2; py <= penWidth/2; py++)
                {
                    DrawPixel(g, cx + x + px, cy + y + py, color);
                    DrawPixel(g, cx - x + px, cy + y + py, color);
                    DrawPixel(g, cx + x + px, cy - y + py, color);
                    DrawPixel(g, cx - x + px, cy - y + py, color);
                }
            }
        }
        
        protected void FillEllipse(Graphics g, Rectangle rect, Color color)
        {
            int a = rect.Width / 2;
            int b = rect.Height / 2;
            int centerX = rect.X + a;
            int centerY = rect.Y + b;
            
            if (a <= 0 || b <= 0) return;
            
            long a2 = a * a;
            long b2 = b * b;
            
            if (a > b) {
                for (int y = -b; y <= b; y++) {
                    int xLimit = (int)Math.Sqrt(a2 * (1 - (y * y) / (double)b2));
                    
                    for (int x = -xLimit; x <= xLimit; x++) {
                        DrawPixel(g, centerX + x, centerY + y, color);
                    }
                }
            } 
            else {
                for (int x = -a; x <= a; x++) {
                    int yLimit = (int)Math.Sqrt(b2 * (1 - (x * x) / (double)a2));
                    
                    for (int y = -yLimit; y <= yLimit; y++) {
                        DrawPixel(g, centerX + x, centerY + y, color);
                    }
                }
            }
        }

        protected void DrawPolygon(Graphics g, Point[] points, Color color, int penWidth)
        {
            if (points.Length < 2)
                return;
                
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(g, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, color, penWidth);
            }
            
            DrawLine(g, points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, color, penWidth);
        }
        
        protected void FillPolygon(Graphics g, Point[] points, Color color)
        {
            if (points.Length < 3)
                return;
                
            int minX = points[0].X;
            int minY = points[0].Y;
            int maxX = points[0].X;
            int maxY = points[0].Y;
            
            for (int i = 1; i < points.Length; i++)
            {
                minX = Math.Min(minX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxX = Math.Max(maxX, points[i].X);
                maxY = Math.Max(maxY, points[i].Y);
            }
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsPointInPolygon(new Point(x, y), points))
                    {
                        DrawPixel(g, x, y, color);
                    }
                }
            }
        }
        
        protected bool IsPointInPolygon(Point p, Point[] polygon)
        {
            int n = polygon.Length;
            bool inside = false;
            
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y)) &&
                    (p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            
            return inside;
        }

        protected void DrawBezierCurve(Graphics g, Point p0, Point p1, Point p2, Point p3, Color color, int penWidth)
        {
            DrawBezierRecursive(g, p0, p1, p2, p3, color, penWidth, 0);
        }
        
        private void DrawBezierRecursive(Graphics g, Point p0, Point p1, Point p2, Point p3, Color color, int penWidth, int level)
        {
            const int MAX_LEVEL = 10;
            const double DISTANCE_TOLERANCE = 2.0;
            
            if (level > MAX_LEVEL)
            {
                DrawLine(g, p0.X, p0.Y, p3.X, p3.Y, color, penWidth);
                return;
            }
            
            Point p01 = new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            Point p12 = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            Point p23 = new Point((p2.X + p3.X) / 2, (p2.Y + p3.Y) / 2);
            
            Point p012 = new Point((p01.X + p12.X) / 2, (p01.Y + p12.Y) / 2);
            Point p123 = new Point((p12.X + p23.X) / 2, (p12.Y + p23.Y) / 2);
            
            Point p0123 = new Point((p012.X + p123.X) / 2, (p012.Y + p123.Y) / 2);
            
            double d1 = PointLineDistance(p1, p0, p3);
            double d2 = PointLineDistance(p2, p0, p3);
            
            if (d1 <= DISTANCE_TOLERANCE && d2 <= DISTANCE_TOLERANCE)
            {
                DrawLine(g, p0.X, p0.Y, p3.X, p3.Y, color, penWidth);
                return;
            }
            
            DrawBezierRecursive(g, p0, p01, p012, p0123, color, penWidth, level + 1);
            DrawBezierRecursive(g, p0123, p123, p23, p3, color, penWidth, level + 1);
        }
        
        private double PointLineDistance(Point p, Point lineStart, Point lineEnd)
        {
            if (lineStart.X == lineEnd.X && lineStart.Y == lineEnd.Y)
                return Math.Sqrt(Math.Pow(p.X - lineStart.X, 2) + Math.Pow(p.Y - lineStart.Y, 2));
                
            double normalLength = Math.Sqrt(Math.Pow(lineEnd.X - lineStart.X, 2) + Math.Pow(lineEnd.Y - lineStart.Y, 2));
            return Math.Abs((p.X - lineStart.X) * (lineEnd.Y - lineStart.Y) - (p.Y - lineStart.Y) * (lineEnd.X - lineStart.X)) / normalLength;
        }

        protected void DrawControlPoint(Graphics g, Point center, int size, Color color)
        {
            int halfSize = size / 2;
            Rectangle rect = new Rectangle(center.X - halfSize, center.Y - halfSize, size, size);
            FillEllipse(g, rect, color);
        }
        
        protected void DrawDashedLine(Graphics g, int x0, int y0, int x1, int y1, Color color, int penWidth)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            bool isDash = true;
            int dashLength = 5;
            int dashCount = 0;
            
            while (true)
            {
                if (isDash)
                {
                    for (int w = -penWidth / 2; w <= penWidth / 2; w++)
                    {
                        for (int h = -penWidth / 2; h <= penWidth / 2; h++)
                        {
                            DrawPixel(g, x0 + w, y0 + h, color);
                        }
                    }
                }
                
                dashCount++;
                if (dashCount >= dashLength)
                {
                    dashCount = 0;
                    isDash = !isDash;
                }
                
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
    
    public class LineShape : Shape
    {
        public override void Draw(Graphics g)
        {
            DrawLine(g, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            const int tolerance = 5;
            
            float dx = EndPoint.X - StartPoint.X;
            float dy = EndPoint.Y - StartPoint.Y;
            
            if (dx == 0 && dy == 0)
                return Math.Abs(point.X - StartPoint.X) <= tolerance && 
                       Math.Abs(point.Y - StartPoint.Y) <= tolerance;
            
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float d = Math.Abs((point.X - StartPoint.X) * dy - (point.Y - StartPoint.Y) * dx) / length;
            
            if (d <= tolerance)
            {
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

    public class RectangleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetBounds();
            
            if (IsFilled)
            {
                FillRectangle(g, rect, FillColor);
            }
            
            DrawRectangle(g, rect, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            Rectangle rect = GetBounds();
            
            if (rect.Contains(point))
            {
                if (IsFilled)
                {
                    return true;
                }
                
                const int tolerance = 5;
                Rectangle innerRect = rect;
                innerRect.Inflate(-tolerance, -tolerance);
                
                return !innerRect.Contains(point);
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

    public class EllipseShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetBounds();
            
            if (IsFilled)
            {
                FillEllipse(g, rect, FillColor);
            }
            
            DrawEllipse(g, rect, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            Rectangle rect = GetBounds();
            int a = rect.Width / 2;
            int b = rect.Height / 2;
            
            if (a == 0 || b == 0)
                return false;
            
            Point center = new Point(rect.X + a, rect.Y + b);
            
            double normalizedX = (double)(point.X - center.X) / a;
            double normalizedY = (double)(point.Y - center.Y) / b;
            double distance = normalizedX * normalizedX + normalizedY * normalizedY;
            
            if (IsFilled && distance <= 1.0)
            {
                return true;
            }
            
            return Math.Abs(distance - 1.0) <= 0.1;
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

    public class TriangleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Point[] points = GetTrianglePoints();
            
            if (IsFilled)
            {
                FillPolygon(g, points, FillColor);
            }
            
            DrawPolygon(g, points, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetTrianglePoints()
        {
            Rectangle bounds = GetBounds();
            int centerX = bounds.X + bounds.Width / 2;
            
            Point[] points = new Point[3];
            points[0] = new Point(centerX, bounds.Y);
            points[1] = new Point(bounds.X, bounds.Y + bounds.Height);
            points[2] = new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height);
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetTrianglePoints();
            
            if (IsFilled && IsPointInPolygon(point, points))
            {
                return true;
            }
            
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

    public class CircleShape : Shape
    {
        public override void Draw(Graphics g)
        {
            Rectangle rect = GetCircleBounds();
            
            if (IsFilled)
            {
                FillEllipse(g, rect, FillColor);
            }
            
            DrawEllipse(g, rect, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Rectangle GetCircleBounds()
        {
            double radius = Math.Sqrt(
                Math.Pow(EndPoint.X - StartPoint.X, 2) + 
                Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            
            int intRadius = (int)Math.Ceiling(radius);
            
            int left = StartPoint.X - intRadius;
            int top = StartPoint.Y - intRadius;
            int diameter = intRadius * 2;
            
            return new Rectangle(left, top, diameter, diameter);
        }

        public override bool Contains(Point point)
        {
            Point center = StartPoint;
            double radius = Math.Sqrt(
                Math.Pow(EndPoint.X - StartPoint.X, 2) + 
                Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            
            if (radius == 0)
                return false;
            
            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
            
            if (IsFilled && distance <= radius)
            {
                return true;
            }
            
            return Math.Abs(distance - radius) <= 5;
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

    public class PolygonShape : Shape
    {
        public int Sides { get; set; }

        public PolygonShape(int sides = 5)
        {
            Sides = Math.Max(3, sides);
        }

        public override void Draw(Graphics g)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled)
            {
                FillPolygon(g, points, FillColor);
            }
            
            DrawPolygon(g, points, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetPolygonPoints()
        {
            Rectangle rect = GetBounds();
            
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2;
            
            Point[] points = new Point[Sides];
            
            for (int i = 0; i < Sides; i++)
            {
                double angle = 2 * Math.PI * i / Sides - Math.PI / 2;
                int x = (int)(centerX + radius * Math.Cos(angle));
                int y = (int)(centerY + radius * Math.Sin(angle));
                points[i] = new Point(x, y);
            }
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled && IsPointInPolygon(point, points))
            {
                return true;
            }
            
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

    public class RegularPolygonShape : Shape
    {
        public int Sides { get; set; }

        public RegularPolygonShape(int sides = 5)
        {
            Sides = Math.Max(3, sides);
        }

        public override void Draw(Graphics g)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled)
            {
                FillPolygon(g, points, FillColor);
            }
            
            DrawPolygon(g, points, Color, PenWidth);
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private Point[] GetPolygonPoints()
        {
            Rectangle rect = GetBounds();
            
            float centerX = rect.X + rect.Width / 2f;
            float centerY = rect.Y + rect.Height / 2f;
            float radiusX = rect.Width / 2f;
            float radiusY = rect.Height / 2f;
            
            Point[] points = new Point[Sides];
            
            for (int i = 0; i < Sides; i++)
            {
                double angle = 2 * Math.PI * i / Sides - Math.PI / 2;
                float x = centerX + radiusX * (float)Math.Cos(angle);
                float y = centerY + radiusY * (float)Math.Sin(angle);
                points[i] = new Point((int)x, (int)y);
            }
            
            return points;
        }

        public override bool Contains(Point point)
        {
            Point[] points = GetPolygonPoints();
            
            if (IsFilled && IsPointInPolygon(point, points))
            {
                return true;
            }
            
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

    public class BezierShape : Shape
    {
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
                ControlPoint1 = new Point(
                    (start.X * 2 + end.X) / 3,
                    (start.Y * 2 + end.Y) / 3);
                ControlPoint2 = new Point(
                    (start.X + end.X * 2) / 3,
                    (start.Y + end.Y * 2) / 3);
            }
        }

        private const int CONTROL_POINT_SIZE = 8;
        private const int ANCHOR_POINT_SIZE = 10;

        public List<BezierSegment> Segments { get; private set; } = new List<BezierSegment>();
        
        public BezierSegment CurrentSegment { get; set; }
        
        public bool IsEndPointSet { get; set; } = false;
        public bool IsControlPoint1Set { get; set; } = false;
        public bool IsControlPoint2Set { get; set; } = false;

        public enum DragPoint { None, Start, End, Control1, Control2 }
        private DragPoint currentDragPoint = DragPoint.None;
        private int currentDragSegmentIndex = -1;

        public override void Draw(Graphics g)
            {
                foreach (var segment in Segments)
                {
                DrawBezierSegment(g, segment, Color, PenWidth);
            }
            
            if (CurrentSegment != null)
            {
                DrawBezierSegment(g, CurrentSegment, Color, PenWidth);
            }

            if (Selected || CurrentSegment != null)
                {
                    foreach (var segment in Segments)
                    {
                    DrawDashedLine(g, segment.StartPoint.X, segment.StartPoint.Y, 
                             segment.ControlPoint1.X, segment.ControlPoint1.Y, Color.Gray, 1);
                    DrawDashedLine(g, segment.EndPoint.X, segment.EndPoint.Y, 
                             segment.ControlPoint2.X, segment.ControlPoint2.Y, Color.Gray, 1);
                    
                    DrawControlPoint(g, segment.StartPoint, ANCHOR_POINT_SIZE, Color.Blue);
                    DrawControlPoint(g, segment.EndPoint, ANCHOR_POINT_SIZE, Color.Green);
                    DrawControlPoint(g, segment.ControlPoint1, CONTROL_POINT_SIZE, Color.Red);
                    DrawControlPoint(g, segment.ControlPoint2, CONTROL_POINT_SIZE, Color.Red);
                }
                
                    if (CurrentSegment != null)
                    {
                    DrawDashedLine(g, CurrentSegment.StartPoint.X, CurrentSegment.StartPoint.Y, 
                             CurrentSegment.ControlPoint1.X, CurrentSegment.ControlPoint1.Y, Color.Gray, 1);
                    DrawDashedLine(g, CurrentSegment.EndPoint.X, CurrentSegment.EndPoint.Y, 
                             CurrentSegment.ControlPoint2.X, CurrentSegment.ControlPoint2.Y, Color.Gray, 1);
                    
                    DrawControlPoint(g, CurrentSegment.StartPoint, ANCHOR_POINT_SIZE, Color.Blue);
                    DrawControlPoint(g, CurrentSegment.EndPoint, ANCHOR_POINT_SIZE, Color.Green);
                    DrawControlPoint(g, CurrentSegment.ControlPoint1, CONTROL_POINT_SIZE, Color.Red);
                    DrawControlPoint(g, CurrentSegment.ControlPoint2, CONTROL_POINT_SIZE, Color.Red);
                }
            }

            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        private void DrawBezierSegment(Graphics g, BezierSegment segment, Color color, int penWidth)
        {
            const int steps = 50;
            
            Point prevPoint = segment.StartPoint;
            
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                Point currentPoint = CalculateBezierPoint(segment, t);
                
                DrawLine(g, prevPoint.X, prevPoint.Y, currentPoint.X, currentPoint.Y, color, penWidth);
                
                prevPoint = currentPoint;
            }
        }

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

        public bool IsOverControlPoint(Point mousePoint, out int segmentIndex, out DragPoint pointType)
        {
            segmentIndex = -1;
            pointType = DragPoint.None;
            
            for (int i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];
                
                if (IsPointNear(mousePoint, segment.StartPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Start;
                    return true;
                }
                
                if (IsPointNear(mousePoint, segment.EndPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.End;
                    return true;
                }
                
                if (IsPointNear(mousePoint, segment.ControlPoint1, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Control1;
                    return true;
                }
                
                if (IsPointNear(mousePoint, segment.ControlPoint2, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = i;
                    pointType = DragPoint.Control2;
                    return true;
                }
            }
            
            if (CurrentSegment != null)
            {
                if (IsPointNear(mousePoint, CurrentSegment.StartPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Start;
                    return true;
                }
                
                if (IsPointNear(mousePoint, CurrentSegment.EndPoint, ANCHOR_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.End;
                    return true;
                }
                
                if (IsPointNear(mousePoint, CurrentSegment.ControlPoint1, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Control1;
                    return true;
                }
                
                if (IsPointNear(mousePoint, CurrentSegment.ControlPoint2, CONTROL_POINT_SIZE/2))
                {
                    segmentIndex = Segments.Count;
                    pointType = DragPoint.Control2;
                    return true;
                }
            }
            
            return false;
        }
        
        public void StartDraggingControlPoint(int segmentIndex, DragPoint pointType)
        {
            currentDragSegmentIndex = segmentIndex;
            currentDragPoint = pointType;
        }
        
        public void StopDragging()
        {
            currentDragSegmentIndex = -1;
            currentDragPoint = DragPoint.None;
        }
        
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
            
            if (currentDragSegmentIndex == 0 && currentDragPoint == DragPoint.Start)
                StartPoint = newPosition;
                
            if (currentDragSegmentIndex == Segments.Count - 1 && currentDragPoint == DragPoint.End)
                EndPoint = newPosition;
        }
        
        private bool IsPointNear(Point p1, Point p2, int radius)
        {
            return Math.Abs(p1.X - p2.X) <= radius && Math.Abs(p1.Y - p2.Y) <= radius;
        }

        public override bool Contains(Point point)
        {
            int segmentIndex;
            DragPoint pointType;
            if (IsOverControlPoint(point, out segmentIndex, out pointType))
                return true;
                
            const int STEPS = 50;
            const double TOLERANCE = 5.0;

            foreach (var segment in Segments)
            {
                for (int i = 0; i < STEPS; i++)
                {
                    double t1 = (double)i / STEPS;
                    double t2 = (double)(i + 1) / STEPS;
                    
                    Point p1 = CalculateBezierPoint(segment, t1);
                    Point p2 = CalculateBezierPoint(segment, t2);
                    
                    double dist = DistanceToLine(point, p1, p2);
                    if (dist <= TOLERANCE)
                        return true;
                }
            }
            
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

        private double DistanceToLine(Point p, Point start, Point end)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            
            if (dx == 0 && dy == 0)
                return Math.Sqrt(Math.Pow(p.X - start.X, 2) + Math.Pow(p.Y - start.Y, 2));
            
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float d = Math.Abs((p.X - start.X) * dy - (p.Y - start.Y) * dx) / length;
            
            float dotProduct = ((p.X - start.X) * dx + (p.Y - start.Y) * dy) / (dx * dx + dy * dy);
            
            if (dotProduct < 0)
                return Math.Sqrt(Math.Pow(p.X - start.X, 2) + Math.Pow(p.Y - start.Y, 2));
            else if (dotProduct > 1)
                return Math.Sqrt(Math.Pow(p.X - end.X, 2) + Math.Pow(p.Y - end.Y, 2));
            else
                return d;
        }

        public override void Move(int deltaX, int deltaY)
        {
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
            
            foreach (var segment in this.Segments)
            {
                BezierSegment clonedSegment = new BezierSegment(segment.StartPoint, segment.EndPoint);
                clonedSegment.ControlPoint1 = segment.ControlPoint1;
                clonedSegment.ControlPoint2 = segment.ControlPoint2;
                clone.Segments.Add(clonedSegment);
            }
            
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
        
        public void AddSegment(Point startPoint, Point endPoint)
        {
            BezierSegment segment = new BezierSegment(startPoint, endPoint);
            Segments.Add(segment);
            
            if (Segments.Count == 1)
                StartPoint = startPoint;
            
            EndPoint = endPoint;
        }
        
        public void FinishCurrentSegment()
        {
            if (CurrentSegment != null && IsControlPoint2Set)
            {
                Segments.Add(CurrentSegment);
                EndPoint = CurrentSegment.EndPoint;
            }
        }
    }

    public class PolylineShape : Shape
    {
        public List<Point> Points { get; private set; } = new List<Point>();

        public override void Draw(Graphics g)
        {
            if (Points.Count < 2)
                return;
                
            for (int i = 0; i < Points.Count - 1; i++)
            {
                DrawLine(g, Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Color, PenWidth);
            }
            
            foreach (var point in Points)
            {
                DrawControlPoint(g, point, 6, Color.Red);
            }
            
            if (Selected)
            {
                DrawSelectionHandles(g);
            }
        }

        public override bool Contains(Point point)
        {
            const int tolerance = 5;
            
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
                    float dotProduct = ((point.X - p1.X) * dx + (point.Y - p1.Y) * dy) / (dx * dx + dy * dy);
                    if (dotProduct >= 0 && dotProduct <= 1)
                        return true;
                }
            }
            
            return false;
        }

        public override void Move(int deltaX, int deltaY)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(Points[i].X + deltaX, Points[i].Y + deltaY);
            }
            
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
        
        public void AddPoint(Point point)
        {
            Points.Add(point);
            
            if (Points.Count == 1)
                StartPoint = point;
            
            EndPoint = point;
        }
    }
} 