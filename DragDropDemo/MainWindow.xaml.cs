using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DragDropDemo;

public partial class MainWindow
{
    private static readonly Color DefaultColor = Color.FromArgb(128, 0, 255, 0);
    private static readonly Color HoverColor = Color.FromArgb(128, 255, 165, 0);

    private static readonly DropPosition[] Positions =
    [
        DropPosition.Top,
        DropPosition.Right,
        DropPosition.Bottom,
        DropPosition.Left,
        DropPosition.Center
    ];

    private readonly Canvas _overlayCanvas;
    private readonly OverlayRectangle _dropOverlayTop;
    private readonly OverlayRectangle _dropOverlayRight;
    private readonly OverlayRectangle _dropOverlayBottom;
    private readonly OverlayRectangle _dropOverlayLeft;
    private readonly OverlayRectangle _dropOverlayCenter;
    private Point _startPoint;
    private UIElement? _currentTarget;

    public MainWindow()
    {
        InitializeComponent();

        // Create a canvas that covers the entire target grid
        _overlayCanvas = new Canvas
        {
            IsHitTestVisible = false
        };

        _dropOverlayTop = new OverlayRectangle(DropPosition.Top, DefaultColor, HoverColor);
        _dropOverlayRight = new OverlayRectangle(DropPosition.Right, DefaultColor, HoverColor);
        _dropOverlayBottom = new OverlayRectangle(DropPosition.Bottom, DefaultColor, HoverColor);
        _dropOverlayLeft = new OverlayRectangle(DropPosition.Left, DefaultColor, HoverColor);
        _dropOverlayCenter = new OverlayRectangle(DropPosition.Center, DefaultColor, HoverColor);

        InitializeOverlayCanvas();
        InitializeDragDrop();
    }

    private void InitializeOverlayCanvas()
    {
        // Insert the canvas right after the TargetGrid in the visual tree
        if (TargetGrid.Parent is Grid parentGrid)
        {
            parentGrid.Children.Add(_overlayCanvas);
            Grid.SetColumn(_overlayCanvas, 1); // Same column as TargetGrid

            // Bind the canvas size to the TargetGrid size
            _overlayCanvas.SetBinding(WidthProperty, new Binding(nameof(ActualWidth)) { Source = TargetGrid });
            _overlayCanvas.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = TargetGrid });
        }
    }

    private void InitializeDragDrop()
    {
        // Setup TreeView drag events
        SourceTreeView.PreviewMouseLeftButtonDown += (_, e) => _startPoint = e.GetPosition(null);

        SourceTreeView.PreviewMouseMove += OnSourceTreeViewOnPreviewMouseMove;

        // Setup Grid drop events
        foreach (UIElement element in TargetGrid.Children)
        {
            if (element is Border border)
            {
                border.AllowDrop = true;
                border.DragEnter += Border_DragEnter;
                border.DragOver += Border_DragOver;
                border.DragLeave += Border_DragLeave;
                border.Drop += Border_Drop;
            }
        }

        return;

        void OnSourceTreeViewOnPreviewMouseMove(object _, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - _startPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem is not null)
            {
                DragDrop.DoDragDrop(treeViewItem, treeViewItem.Header, DragDropEffects.Move);
            }
        }
    }

    private static bool IsControlPressed(DragEventArgs e)
    {
        return (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey;
    }

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        _currentTarget = sender as UIElement;
        if (_currentTarget is not null && !IsControlPressed(e))
        {
            ShowDropOverlay();
        }
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
        if (!IsControlPressed(e))
        {
            RemoveDropOverlay();
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            return;
        }

        // Get the mouse position relative to the current target
        if (_currentTarget is FrameworkElement target)
        {
            ShowDropOverlay();

            Point position = e.GetPosition(target);
            DropPosition dropPosition = DetermineDropPosition(position, target);

            // Highlight the appropriate rectangle based on position
            OverlayRectangle? rectangleToHighlight = dropPosition switch
            {
                DropPosition.Top => _dropOverlayTop,
                DropPosition.Right => _dropOverlayRight,
                DropPosition.Bottom => _dropOverlayBottom,
                DropPosition.Left => _dropOverlayLeft,
                DropPosition.Center => _dropOverlayCenter,
                _ => null
            };

            rectangleToHighlight?.SetHoverBrush();

            // Allow dropping if we're over a valid drop position
            e.Effects = dropPosition != DropPosition.Unknown ? DragDropEffects.Move : DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Border_DragLeave(object sender, DragEventArgs e)
    {
        RemoveDropOverlay();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.GetData(DataFormats.Text) is string draggedText)
        {
            string dropText = draggedText;
            if (IsControlPressed(e))
            {
                var position = e.GetPosition(border);
                var dropPosition = DetermineDropPosition(position, border);
                if (dropPosition == DropPosition.Unknown)
                {
                    RemoveDropOverlay();
                    return;
                }

                dropText = $"{draggedText} ({dropPosition})";
            }
            
            // Update the target element with the dropped item
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = dropText;
            }
        }

        RemoveDropOverlay();
    }

    private void ShowDropOverlay()
    {
        _dropOverlayTop.AddToCanvas(_overlayCanvas);
        _dropOverlayRight.AddToCanvas(_overlayCanvas);
        _dropOverlayBottom.AddToCanvas(_overlayCanvas);
        _dropOverlayLeft.AddToCanvas(_overlayCanvas);
        _dropOverlayCenter.AddToCanvas(_overlayCanvas);

        UpdateDropOverlayPosition();
    }

    private void UpdateDropOverlayPosition()
    {
        if (_currentTarget is not FrameworkElement target) return;

        // Get the position of the target relative to the overlay canvas
        Point targetPos = target.TranslatePoint(new Point(0, 0), _overlayCanvas);

        const double smallSize = 0.1;
        const double middleSize = 0.25;
        const double largeSize = 0.8;

        // Position the overlay based on which quadrant the mouse is in
        _dropOverlayTop.Width = target.ActualWidth * largeSize;
        _dropOverlayTop.Height = target.ActualHeight * smallSize;
        _dropOverlayTop.Left = targetPos.X + (target.ActualWidth - _dropOverlayTop.Width) / 2;
        _dropOverlayTop.Top = targetPos.Y;
        _dropOverlayTop.SetDefaultBrush();

        _dropOverlayRight.Width = target.ActualWidth * smallSize;
        _dropOverlayRight.Height = target.ActualHeight * largeSize;
        _dropOverlayRight.Left = targetPos.X + target.ActualWidth - _dropOverlayRight.Width;
        _dropOverlayRight.Top = targetPos.Y + (target.ActualHeight - _dropOverlayRight.Height) / 2;
        _dropOverlayRight.SetDefaultBrush();

        _dropOverlayBottom.Width = target.ActualWidth * largeSize;
        _dropOverlayBottom.Height = target.ActualHeight * smallSize;
        _dropOverlayBottom.Left = targetPos.X + (target.ActualWidth - _dropOverlayBottom.Width) / 2;
        _dropOverlayBottom.Top = targetPos.Y + target.ActualHeight - _dropOverlayBottom.Height;
        _dropOverlayBottom.SetDefaultBrush();

        _dropOverlayLeft.Width = target.ActualWidth * smallSize;
        _dropOverlayLeft.Height = target.ActualHeight * largeSize;
        _dropOverlayLeft.Left = targetPos.X;
        _dropOverlayLeft.Top = targetPos.Y + (target.ActualHeight - _dropOverlayLeft.Height) / 2;
        _dropOverlayLeft.SetDefaultBrush();

        _dropOverlayCenter.Width = target.ActualWidth * middleSize;
        _dropOverlayCenter.Height = target.ActualHeight * middleSize;
        _dropOverlayCenter.Left = targetPos.X + target.ActualWidth / 2 - _dropOverlayCenter.Width / 2;
        _dropOverlayCenter.Top = targetPos.Y + target.ActualHeight / 2 - _dropOverlayCenter.Height / 2;
        _dropOverlayCenter.SetDefaultBrush();
    }

    private void RemoveDropOverlay()
    {
        _dropOverlayTop.RemoveFromCanvas(_overlayCanvas);
        _dropOverlayRight.RemoveFromCanvas(_overlayCanvas);
        _dropOverlayBottom.RemoveFromCanvas(_overlayCanvas);
        _dropOverlayLeft.RemoveFromCanvas(_overlayCanvas);
        _dropOverlayCenter.RemoveFromCanvas(_overlayCanvas);
    }

    private DropPosition DetermineDropPosition(Point position, FrameworkElement target)
    {
        // Convert the position to be relative to the overlay canvas
        Point canvasPosition = target.TranslatePoint(position, _overlayCanvas);

        // Check each rectangle
        OverlayRectangle?[] overlayRectangles =
        [
            _dropOverlayTop,
            _dropOverlayRight,
            _dropOverlayBottom,
            _dropOverlayLeft,
            _dropOverlayCenter
        ];

        for (int i = 0; i < overlayRectangles.Length; i++)
        {
            var rectangle = overlayRectangles[i];
            if (rectangle is null) continue;

            if (rectangle.Bounds.Contains(canvasPosition))
            {
                return Positions[i];
            }
        }

        return DropPosition.Unknown;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        do
        {
            switch (current)
            {
                case null:
                    return null;
                case T t:
                    return t;
                default:
                    current = VisualTreeHelper.GetParent(current);
                    break;
            }
        } while (current != null);

        return null;
    }

    private enum DropPosition
    {
        Unknown,
        Top,
        Right,
        Bottom,
        Left,
        Center
    }

    private class OverlayRectangle
    {
        private readonly Rectangle _rectangle;
        private readonly Brush _defaultBrush;
        private readonly Brush _hoverBrush;
        private bool _isShowing;

        public OverlayRectangle(DropPosition dropPosition, Color defaultColor, Color hoverColor)
        {
            _rectangle = new Rectangle
            {
                IsHitTestVisible = false
            };

            switch (dropPosition)
            {
                case DropPosition.Unknown:
                    _defaultBrush = new SolidColorBrush(defaultColor);
                    _hoverBrush = new SolidColorBrush(hoverColor);
                    return;
                case DropPosition.Top:
                case DropPosition.Right:
                case DropPosition.Bottom:
                case DropPosition.Left:
                    _defaultBrush = GetEdgeBrush(dropPosition, defaultColor);
                    _hoverBrush = GetEdgeBrush(dropPosition, hoverColor);
                    return;
                case DropPosition.Center:
                    _defaultBrush = GetCenterBrush(defaultColor);
                    _hoverBrush = GetCenterBrush(hoverColor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dropPosition), dropPosition, null);
            }
        }

        public double Left
        {
            private get => Canvas.GetLeft(_rectangle);
            set => Canvas.SetLeft(_rectangle, value);
        }

        public double Top
        {
            private get => Canvas.GetTop(_rectangle);
            set => Canvas.SetTop(_rectangle, value);
        }

        public double Width
        {
            get => _rectangle.Width;
            set => _rectangle.Width = value;
        }

        public double Height
        {
            get => _rectangle.Height;
            set => _rectangle.Height = value;
        }

        public Rect Bounds => new(Left, Top, Width, Height);

        public void AddToCanvas(Canvas canvas)
        {
            if (_isShowing) return;

            _isShowing = true;
            canvas.Children.Add(_rectangle);
        }

        public void RemoveFromCanvas(Canvas canvas)
        {
            canvas.Children.Remove(_rectangle);
            _isShowing = false;
        }

        public void SetDefaultBrush()
        {
            _rectangle.Fill = _defaultBrush;
        }

        public void SetHoverBrush()
        {
            _rectangle.Fill = _hoverBrush;
        }

        private static LinearGradientBrush GetEdgeBrush(DropPosition dropPosition, Color color)
        {
            var angle = dropPosition switch
            {
                DropPosition.Top => 90,
                DropPosition.Right => 180,
                DropPosition.Bottom => 270,
                DropPosition.Left => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(dropPosition), dropPosition, null)
            };

            return CreateInwardGradientBrush(color, angle);
        }

        private static LinearGradientBrush CreateInwardGradientBrush(Color startColor, double angle)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            // Add gradient stops (solid on the outside, transparent on the inside)
            brush.GradientStops.Add(new GradientStop(startColor, 0.0)); // Solid
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Transparent

            // Rotate the brush
            brush.RelativeTransform = new RotateTransform(angle, 0.5, 0.5);

            return brush;
        }

        private static RadialGradientBrush GetCenterBrush(Color centerColor)
        {
            // Create a radial gradient brush for the center
            var centerBrush = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };

            // Solid in the center
            centerBrush.GradientStops.Add(new GradientStop(centerColor, 0.0));
            // Transparent on edges
            centerBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
            return centerBrush;
        }
    }
}