using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DragDropDemo;

public partial class MainWindow
{
    private static readonly SolidColorBrush DefaultOverlayBrush = new(Color.FromArgb(128, 0, 255, 0));
    private static readonly SolidColorBrush HoverOverlayBrush = new(Color.FromArgb(128, 255, 165, 0));

    private static readonly DropPosition[] Positions =
    [
        DropPosition.Top,
        DropPosition.Right,
        DropPosition.Bottom,
        DropPosition.Left,
        DropPosition.Center
    ];

    private Point _startPoint;
    private Rectangle? _dropOverlayTop;
    private Rectangle? _dropOverlayRight;
    private Rectangle? _dropOverlayBottom;
    private Rectangle? _dropOverlayLeft;
    private Rectangle? _dropOverlayCenter;
    private UIElement? _currentTarget;
    private readonly Canvas _overlayCanvas;

    public MainWindow()
    {
        InitializeComponent();

        // Create a canvas that covers the entire target grid
        _overlayCanvas = new Canvas
        {
            IsHitTestVisible = false
        };

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

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        _currentTarget = sender as UIElement;
        if (_currentTarget is not null)
        {
            ShowDropOverlay();
        }
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
        UpdateDropOverlayPosition();
        
        // Get the mouse position relative to the current target
        if (_currentTarget is FrameworkElement target)
        {
            Point position = e.GetPosition(target);
            DropPosition dropPosition = DetermineDropPosition(position, target);
        
            // Reset all rectangles to the default color
            _dropOverlayTop!.Fill = DefaultOverlayBrush;
            _dropOverlayRight!.Fill = DefaultOverlayBrush;
            _dropOverlayBottom!.Fill = DefaultOverlayBrush;
            _dropOverlayLeft!.Fill = DefaultOverlayBrush;
            _dropOverlayCenter!.Fill = DefaultOverlayBrush;

            // Highlight the appropriate rectangle based on position
            Rectangle? rectangleToHighlight = dropPosition switch
            {
                DropPosition.Top => _dropOverlayTop,
                DropPosition.Right => _dropOverlayRight,
                DropPosition.Bottom => _dropOverlayBottom,
                DropPosition.Left => _dropOverlayLeft,
                DropPosition.Center => _dropOverlayCenter,
                _ => null
            };

            if (rectangleToHighlight != null)
            {
                rectangleToHighlight.Fill = HoverOverlayBrush;
            }
        }

        e.Effects = DragDropEffects.Move;
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
            var position = e.GetPosition(border);
            var dropPosition = DetermineDropPosition(position, border);
            if (dropPosition == DropPosition.Unknown)
            {
                RemoveDropOverlay();
                return;
            }

            // Update the target element with the dropped item
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = $"{draggedText} ({dropPosition})";
            }
        }

        RemoveDropOverlay();
    }

    private void ShowDropOverlay()
    {
        _dropOverlayTop ??= AddDropOverlay();
        _dropOverlayRight ??= AddDropOverlay();
        _dropOverlayBottom ??= AddDropOverlay();
        _dropOverlayLeft ??= AddDropOverlay();
        _dropOverlayCenter ??= AddDropOverlay();

        UpdateDropOverlayPosition();
    }

    private Rectangle AddDropOverlay()
    {
        var rectangle = new Rectangle
        {
            Fill = DefaultOverlayBrush,
            IsHitTestVisible = false
        };

        _overlayCanvas.Children.Add(rectangle);
        return rectangle;
    }
    
    private void UpdateOverlayRectangleSize(Rectangle rectangle, double width, double height)
    {
        rectangle.Width = width;
        rectangle.Height = height;
    }

    private void UpdateDropOverlayPosition()
    {
        if (_dropOverlayTop == null || _dropOverlayRight == null || _dropOverlayBottom == null ||
            _dropOverlayLeft == null || _dropOverlayCenter == null ||
            _currentTarget is not FrameworkElement target) return;

        // Get the position of the target relative to the overlay canvas
        Point targetPos = target.TranslatePoint(new Point(0, 0), _overlayCanvas);

        const double smallSize = 0.05;
        const double middleSize = 0.25;
        const double largeSize = 0.75;

        // Position the overlay based on which quadrant the mouse is in
        UpdateOverlayRectangleSize(_dropOverlayTop, target.ActualWidth * largeSize, target.ActualHeight * smallSize);
        Canvas.SetLeft(_dropOverlayTop, targetPos.X + (target.ActualWidth - _dropOverlayTop.Width) / 2);
        Canvas.SetTop(_dropOverlayTop, targetPos.Y);

        UpdateOverlayRectangleSize(_dropOverlayRight, target.ActualWidth * smallSize, target.ActualHeight * largeSize);
        Canvas.SetLeft(_dropOverlayRight, targetPos.X + target.ActualWidth - _dropOverlayRight.Width);
        Canvas.SetTop(_dropOverlayRight, targetPos.Y + (target.ActualHeight - _dropOverlayRight.Height) / 2);
        
        UpdateOverlayRectangleSize(_dropOverlayBottom, target.ActualWidth * largeSize, target.ActualHeight * smallSize);
        Canvas.SetLeft(_dropOverlayBottom, targetPos.X + (target.ActualWidth - _dropOverlayBottom.Width) / 2);
        Canvas.SetTop(_dropOverlayBottom, targetPos.Y + target.ActualHeight - _dropOverlayBottom.Height);
        
        UpdateOverlayRectangleSize(_dropOverlayLeft, target.ActualWidth * smallSize, target.ActualHeight * largeSize);
        Canvas.SetLeft(_dropOverlayLeft, targetPos.X);
        Canvas.SetTop(_dropOverlayLeft, targetPos.Y + (target.ActualHeight - _dropOverlayLeft.Height) / 2);
        
        UpdateOverlayRectangleSize(_dropOverlayCenter, target.ActualWidth * middleSize, target.ActualHeight * middleSize);
        Canvas.SetLeft(_dropOverlayCenter, targetPos.X + target.ActualWidth / 2 - _dropOverlayCenter.Width / 2);
        Canvas.SetTop(_dropOverlayCenter, targetPos.Y + target.ActualHeight / 2 - _dropOverlayCenter.Height / 2);
    }

    private void RemoveDropOverlay()
    {
        void RemoveAndCleanup(ref Rectangle? rectangle)
        {
            if (rectangle != null)
            {
                _overlayCanvas.Children.Remove(rectangle);
                rectangle = null;
            }
        }

        RemoveAndCleanup(ref _dropOverlayTop);
        RemoveAndCleanup(ref _dropOverlayRight);
        RemoveAndCleanup(ref _dropOverlayBottom);
        RemoveAndCleanup(ref _dropOverlayLeft);
        RemoveAndCleanup(ref _dropOverlayCenter);
    }
    
    private DropPosition DetermineDropPosition(Point position, FrameworkElement target)
    {
        // Convert the position to be relative to the overlay canvas
        Point canvasPosition = target.TranslatePoint(position, _overlayCanvas);

        // Check each rectangle
        Rectangle?[] overlayRectangles =
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
            
            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);
        
            Rect bounds = new(
                left, 
                top, 
                rectangle.Width, 
                rectangle.Height
            );

            if (bounds.Contains(canvasPosition))
            {
                return Positions[i];
            }
        }

        // Default to Center if somehow not over any rectangle
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
}