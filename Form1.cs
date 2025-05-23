using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace VectorEditor;

public partial class Form1 : Form
{
    private List<Shape> shapes = new List<Shape>();
    private Shape currentShape;
    private Shape selectedShape;
    private Point lastMousePosition = Point.Empty;
    private bool isDrawing = false;
    private bool isMoving = false;
    private bool isDraggingShape = false;
    private DrawingMode currentMode = DrawingMode.Select;
    private Color currentColor = Color.Black;
    private Color currentFillColor = Color.White;
    private bool isFilled = false;
    private int currentPenWidth = 2;
    private int polygonSides = 5;
    private bool isPolylineDrawing = false;
    private bool isBezierDrawing = false;
    private bool isDraggingBezierPoint = false;
    private BezierShape.DragPoint currentBezierDragPoint = BezierShape.DragPoint.None;
    private int currentBezierDragSegmentIndex = -1;

    public Form1()
    {
        InitializeComponent();
        SetupUI();
        
        this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
        
        this.KeyPreview = true;
        this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        
        this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
    }

    private void SetupUI()
    {
        this.DoubleBuffered = true;
        this.Text = "Векторный редактор";
        this.Size = new Size(1000, 700);
        this.BackColor = Color.White;

        ToolStrip toolStrip = new ToolStrip();
        toolStrip.Dock = DockStyle.Top;

        ToolStripButton selectButton = new ToolStripButton("Выбрать");
        selectButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            currentMode = DrawingMode.Select; 
        };
        selectButton.Checked = true;

        ToolStripButton lineButton = new ToolStripButton("Линия");
        lineButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Line; 
        };

        ToolStripButton rectangleButton = new ToolStripButton("Прямоугольник");
        rectangleButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Rectangle; 
        };

        ToolStripButton ellipseButton = new ToolStripButton("Эллипс");
        ellipseButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Ellipse; 
        };

        ToolStripButton triangleButton = new ToolStripButton("Треугольник");
        triangleButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Triangle; 
        };

        ToolStripButton circleButton = new ToolStripButton("Круг");
        circleButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Circle; 
        };

        ToolStripButton polygonButton = new ToolStripButton("Многоугольник");
        polygonButton.Click += (s, e) => 
        {
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            Form polygonForm = new Form
            {
                Text = "Выберите количество сторон",
                Size = new Size(300, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label sidesLabel = new Label
            {
                Text = "Количество сторон:",
                Location = new Point(20, 20),
                Size = new Size(150, 20)
            };

            NumericUpDown sidesUpDown = new NumericUpDown
            {
                Minimum = 3,
                Maximum = 20,
                Value = polygonSides,
                Location = new Point(170, 20),
                Size = new Size(80, 20)
            };

            Button okButton = new Button
            {
                Text = "ОК",
                DialogResult = DialogResult.OK,
                Location = new Point(100, 60),
                Size = new Size(80, 30)
            };

            polygonForm.Controls.Add(sidesLabel);
            polygonForm.Controls.Add(sidesUpDown);
            polygonForm.Controls.Add(okButton);
            polygonForm.AcceptButton = okButton;

            if (polygonForm.ShowDialog() == DialogResult.OK)
            {
                polygonSides = (int)sidesUpDown.Value;
                currentMode = DrawingMode.Polygon;
            }
        };

        ToolStripButton bezierButton = new ToolStripButton("Кривая Безье");
        bezierButton.Click += (s, e) => { 
            if (currentMode != DrawingMode.Bezier) {
                FinishBezierDrawingIfActive();
            }
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Bezier; 
        };

        ToolStripButton polylineButton = new ToolStripButton("Ломаная линия");
        polylineButton.Click += (s, e) => { 
            FinishBezierDrawingIfActive();
            if (selectedShape != null) {
                selectedShape.Selected = false;
                selectedShape = null;
            }
            currentMode = DrawingMode.Polyline; 
        };

        ToolStripButton colorButton = new ToolStripButton();
        colorButton.ToolTipText = "Цвет линии";
        colorButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        colorButton.Image = CreateColorButtonImage(currentColor, false);
        colorButton.ImageTransparentColor = Color.Magenta;
        colorButton.Size = new Size(24, 24);
        colorButton.Click += (s, e) =>
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = currentColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentColor = colorDialog.Color;
                colorButton.Image = CreateColorButtonImage(currentColor, false);
                
                if (selectedShape != null)
                {
                    selectedShape.Color = currentColor;
                    Invalidate();
                }
            }
        };

        ToolStripButton fillEnabledButton = new ToolStripButton("Заливка");
        fillEnabledButton.CheckOnClick = true;
        fillEnabledButton.Checked = isFilled;
        fillEnabledButton.Click += (s, e) =>
        {
            isFilled = fillEnabledButton.Checked;
            
            if (selectedShape != null)
            {
                selectedShape.IsFilled = isFilled;
                Invalidate();
            }
        };

        ToolStripButton fillColorButton = new ToolStripButton();
        fillColorButton.ToolTipText = "Цвет заливки";
        fillColorButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        fillColorButton.Image = CreateColorButtonImage(currentFillColor, true);
        fillColorButton.ImageTransparentColor = Color.Magenta;
        fillColorButton.Size = new Size(24, 24);
        fillColorButton.Click += (s, e) =>
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = currentFillColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentFillColor = colorDialog.Color;
                fillColorButton.Image = CreateColorButtonImage(currentFillColor, true);
                
                if (selectedShape != null)
                {
                    selectedShape.FillColor = currentFillColor;
                    Invalidate();
                }
            }
        };

        ToolStripButton deleteButton = new ToolStripButton("Удалить");
        deleteButton.Click += (s, e) =>
        {
            if (selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                Invalidate();
            }
        };

        toolStrip.Items.AddRange(new ToolStripItem[]
        {
            selectButton, lineButton, rectangleButton, ellipseButton,
            triangleButton, circleButton, polygonButton,
            bezierButton, polylineButton,
            new ToolStripSeparator(),
            colorButton, fillEnabledButton, fillColorButton,
            new ToolStripSeparator(),
            deleteButton
        });

        ToolStripLabel widthLabel = new ToolStripLabel("Толщина линии:");
        ToolStripComboBox widthComboBox = new ToolStripComboBox();
        for (int i = 1; i <= 10; i++)
        {
            widthComboBox.Items.Add(i.ToString());
        }
        widthComboBox.SelectedIndex = currentPenWidth - 1;
        widthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        widthComboBox.Width = 50;
        widthComboBox.SelectedIndexChanged += (s, e) =>
        {
            currentPenWidth = int.Parse(widthComboBox.SelectedItem.ToString());
            if (selectedShape != null)
            {
                selectedShape.PenWidth = currentPenWidth;
                Invalidate();
            }
        };

        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(widthLabel);
        toolStrip.Items.Add(widthComboBox);

        StatusStrip statusStrip = new StatusStrip();
        ToolStripStatusLabel coordinatesLabel = new ToolStripStatusLabel("Координаты: 0, 0");
        statusStrip.Items.Add(coordinatesLabel);

        this.MouseMove += (s, e) =>
        {
            coordinatesLabel.Text = $"Координаты: {e.X}, {e.Y}";
        };

        this.Controls.Add(toolStrip);
        this.Controls.Add(statusStrip);

        this.Paint += Form1_Paint;
        this.MouseDown += Form1_MouseDown;
        this.MouseMove += Form1_MouseMove;
        this.MouseUp += Form1_MouseUp;
    }

    private Image CreateColorButtonImage(Color color, bool isFill)
    {
        Bitmap bmp = new Bitmap(16, 16);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);
            
            if (isFill)
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, 1, 1, 14, 14);
                }
                using (Pen pen = new Pen(Color.Black, 1))
                {
                    g.DrawRectangle(pen, 1, 1, 14, 14);
                }
            }
            else
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 16, 16),
                    Color.Red, Color.Blue, 
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, 1, 1, 14, 14);
                }
                
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, 6, 6, 4, 4);
                }
                
                using (Pen pen = new Pen(Color.Black, 1))
                {
                    g.DrawRectangle(pen, 1, 1, 14, 14);
                }
            }
        }
        return bmp;
    }

    private void Form1_Paint(object sender, PaintEventArgs e)
    {
        foreach (Shape shape in shapes)
        {
            shape.Draw(e.Graphics);
        }

        if (isDrawing && currentShape != null)
        {
            currentShape.Draw(e.Graphics);
        }
    }

    private void Form1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            lastMousePosition = e.Location;

            if (currentMode == DrawingMode.Select)
            {
                bool bezierPointFound = false;
                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    if (shapes[i] is BezierShape)
                    {
                        BezierShape bezierShape = (BezierShape)shapes[i];
                        int segmentIndex;
                        BezierShape.DragPoint pointType;
                        
                        if (bezierShape.IsOverControlPoint(e.Location, out segmentIndex, out pointType))
                        {
                            isDraggingBezierPoint = true;
                            currentBezierDragPoint = pointType;
                            currentBezierDragSegmentIndex = segmentIndex;
                            
                            if (selectedShape != null)
                                selectedShape.Selected = false;
                            
                            selectedShape = bezierShape;
                            selectedShape.Selected = true;
                            
                            bezierShape.StartDraggingControlPoint(segmentIndex, pointType);
                            bezierPointFound = true;
                            break;
                        }
                    }
                }
                
                if (!bezierPointFound)
                {
                    bool found = false;
                    for (int i = shapes.Count - 1; i >= 0; i--)
                    {
                        if (shapes[i].Contains(e.Location))
                        {
                            if (selectedShape != null)
                                selectedShape.Selected = false;
                            
                            selectedShape = shapes[i];
                            selectedShape.Selected = true;
                            isMoving = true;
                            isDraggingShape = true;
                            found = true;
                            break;
                        }
                    }

                    if (!found && selectedShape != null)
                    {
                        selectedShape.Selected = false;
                        selectedShape = null;
                    }
                }
            }
            else if (currentMode == DrawingMode.Polyline)
            {
                if (!isPolylineDrawing)
                {
                    isPolylineDrawing = true;
                    isDrawing = true;
                    currentShape = new PolylineShape();
                    ((PolylineShape)currentShape).AddPoint(e.Location);
                    ((PolylineShape)currentShape).AddPoint(e.Location);
                    currentShape.Color = currentColor;
                    currentShape.PenWidth = currentPenWidth;
                    shapes.Add(currentShape);
                }
                else
                {
                    ((PolylineShape)currentShape).AddPoint(e.Location);
                }
            }
            else if (currentMode == DrawingMode.Bezier)
            {
                if (!isBezierDrawing)
                {
                    isBezierDrawing = true;
                    isDrawing = true;
                    currentShape = new BezierShape();
                    ((BezierShape)currentShape).CurrentSegment = new BezierShape.BezierSegment(e.Location, e.Location);
                    currentShape.StartPoint = e.Location;
                    currentShape.EndPoint = e.Location;
                    currentShape.Color = currentColor;
                    currentShape.PenWidth = currentPenWidth;
                    shapes.Add(currentShape);
                }
                else
                {
                    BezierShape bezierShape = (BezierShape)currentShape;
                    
                    if (!bezierShape.IsEndPointSet)
                    {
                        bezierShape.CurrentSegment.EndPoint = e.Location;
                        bezierShape.IsEndPointSet = true;
                    }
                    else if (!bezierShape.IsControlPoint1Set)
                    {
                        bezierShape.CurrentSegment.ControlPoint1 = e.Location;
                        bezierShape.IsControlPoint1Set = true;
                    }
                    else if (!bezierShape.IsControlPoint2Set)
                    {
                        bezierShape.CurrentSegment.ControlPoint2 = e.Location;
                        bezierShape.IsControlPoint2Set = true;
                        
                        bezierShape.FinishCurrentSegment();
                        
                        Point lastPoint = bezierShape.CurrentSegment.EndPoint;
                        bezierShape.CurrentSegment = new BezierShape.BezierSegment(lastPoint, lastPoint);
                        bezierShape.IsEndPointSet = false;
                        bezierShape.IsControlPoint1Set = false;
                        bezierShape.IsControlPoint2Set = false;
                    }
                }
            }
            else
            {
                isDrawing = true;
                
                switch (currentMode)
                {
                    case DrawingMode.Line:
                        currentShape = new LineShape();
                        break;
                    case DrawingMode.Rectangle:
                        currentShape = new RectangleShape();
                        break;
                    case DrawingMode.Ellipse:
                        currentShape = new EllipseShape();
                        break;
                    case DrawingMode.Triangle:
                        currentShape = new TriangleShape();
                        break;
                    case DrawingMode.Circle:
                        currentShape = new CircleShape();
                        break;
                    case DrawingMode.Polygon:
                        currentShape = new RegularPolygonShape(polygonSides);
                        break;
                }

                if (currentShape != null)
                {
                    currentShape.StartPoint = e.Location;
                    currentShape.EndPoint = e.Location;
                    currentShape.Color = currentColor;
                    currentShape.PenWidth = currentPenWidth;
                    currentShape.IsFilled = isFilled;
                    currentShape.FillColor = currentFillColor;
                }
            }

            Invalidate();
        }
        else if (e.Button == MouseButtons.Right)
        {
            if (currentMode == DrawingMode.Polyline && isPolylineDrawing)
            {
                isPolylineDrawing = false;
                isDrawing = false;
                currentShape = null;
                Invalidate();
            }
            else if (currentMode == DrawingMode.Bezier && isBezierDrawing)
            {
                BezierShape bezierShape = (BezierShape)currentShape;
                
                if (bezierShape.IsControlPoint2Set)
                {
                    bezierShape.FinishCurrentSegment();
                }
                else
                {
                    bezierShape.CurrentSegment = null;
                }
                
                if (bezierShape.Segments.Count == 0)
                {
                    shapes.Remove(currentShape);
                }
                
                isBezierDrawing = false;
                isDrawing = false;
                currentShape = null;
                Invalidate();
            }
        }
    }

    private void Form1_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDraggingBezierPoint && selectedShape != null && selectedShape is BezierShape)
        {
            ((BezierShape)selectedShape).DragControlPoint(e.Location);
            Invalidate();
        }
        else if (isDrawing && currentShape != null)
        {
            if (currentMode == DrawingMode.Polyline && isPolylineDrawing)
            {
                PolylineShape polyline = (PolylineShape)currentShape;
                if (polyline.Points.Count > 0)
                {
                    polyline.Points[polyline.Points.Count - 1] = e.Location;
                    polyline.EndPoint = e.Location;
                }
            }
            else if (currentMode == DrawingMode.Bezier && isBezierDrawing)
            {
                BezierShape bezierShape = (BezierShape)currentShape;
                
                if (!bezierShape.IsEndPointSet)
                {
                    bezierShape.CurrentSegment.EndPoint = e.Location;
                }
                else if (!bezierShape.IsControlPoint1Set)
                {
                    bezierShape.CurrentSegment.ControlPoint1 = e.Location;
                }
                else if (!bezierShape.IsControlPoint2Set)
                {
                    bezierShape.CurrentSegment.ControlPoint2 = e.Location;
                }
            }
            else
            {
                currentShape.EndPoint = e.Location;
            }
            Invalidate();
        }
        else if (isMoving && selectedShape != null)
        {
            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;
            
            selectedShape.Move(deltaX, deltaY);
            
            lastMousePosition = e.Location;
            Invalidate();
        }
    }

    private void Form1_MouseUp(object sender, MouseEventArgs e)
    {
        lastMousePosition = e.Location;
        
        if (currentMode == DrawingMode.Select)
        {
            if (isDraggingBezierPoint)
            {
                isDraggingBezierPoint = false;
                if (selectedShape != null && selectedShape is BezierShape)
                {
                    ((BezierShape)selectedShape).StopDragging();
                }
            }
            else if (isDrawing && currentShape != null)
            {
                if (currentMode != DrawingMode.Polyline && currentMode != DrawingMode.Bezier)
                {
                    currentShape.EndPoint = e.Location;
                    shapes.Add(currentShape);
                    currentShape = null;
                    isDrawing = false;
                }
            }
            else if (isMoving)
            {
                isMoving = false;
                isDraggingShape = false;
            }
            
            if (isDraggingShape)
            {
                isDraggingShape = false;
            }
        }
        else
        {
            if (isDrawing && currentShape != null)
            {
                if (currentMode != DrawingMode.Polyline && currentMode != DrawingMode.Bezier)
                {
                    currentShape.EndPoint = e.Location;
                    shapes.Add(currentShape);
                    currentShape = null;
                    isDrawing = false;
                }
            }
        }
        
        if (isDraggingBezierPoint)
        {
            isDraggingBezierPoint = false;
            currentBezierDragSegmentIndex = -1;
        }

        Invalidate();
    }

    private void FinishBezierDrawingIfActive()
    {
        if (isBezierDrawing)
        {
            BezierShape bezierShape = (BezierShape)currentShape;
            
            if (bezierShape.IsControlPoint2Set)
            {
                bezierShape.FinishCurrentSegment();
            }
            else
            {
                bezierShape.CurrentSegment = null;
            }
            
            if (bezierShape.Segments.Count == 0)
            {
                shapes.Remove(currentShape);
            }
            
            isBezierDrawing = false;
            isDrawing = false;
            currentShape = null;
            Invalidate();
        }
    }

    private void Form1_MouseWheel(object sender, MouseEventArgs e)
    {
        if (selectedShape != null && currentMode == DrawingMode.Select)
        {
            int index = shapes.IndexOf(selectedShape);
            
            if (index != -1)
            {
                if (e.Delta < 0)
                {
                    MoveShapeUp(index);
                }
                else if (e.Delta > 0)
                {
                    MoveShapeDown(index);
                }
                
                Invalidate();
            }
        }
    }
    
    private void MoveShapeUp(int index)
    {
        if (index >= shapes.Count - 1)
            return;
            
        Shape shape = shapes[index];
        shapes.RemoveAt(index);
        shapes.Insert(index + 1, shape);
    }
    
    private void MoveShapeDown(int index)
    {
        if (index <= 0)
            return;
            
        Shape shape = shapes[index];
        shapes.RemoveAt(index);
        shapes.Insert(index - 1, shape);
    }

    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) && selectedShape != null)
        {
            shapes.Remove(selectedShape);
            selectedShape = null;
            Invalidate();
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        Shape.ClearPixelCache();
    }

    public enum DrawingMode
    {
        Select,
        Line,
        Rectangle,
        Ellipse,
        Triangle,
        Circle,
        Polygon,
        Bezier,
        Polyline
    }
}
