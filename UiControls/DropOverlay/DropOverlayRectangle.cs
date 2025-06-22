using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UiControls.DropOverlay;

public class DropOverlayRectangle
{
    private readonly Rectangle _rectangle;
    private readonly Brush _defaultBrush;
    private readonly Brush _hoverBrush;
    private bool _isShowing;

    public DropOverlayRectangle(DropOverlayPosition dropOverlayPosition, Color defaultColor, Color hoverColor)
    {
        _rectangle = new Rectangle
        {
            IsHitTestVisible = false
        };

        switch (dropOverlayPosition)
        {
            case DropOverlayPosition.Unknown:
                _defaultBrush = new SolidColorBrush(defaultColor);
                _hoverBrush = new SolidColorBrush(hoverColor);
                return;
            case DropOverlayPosition.Top:
            case DropOverlayPosition.Right:
            case DropOverlayPosition.Bottom:
            case DropOverlayPosition.Left:
                _defaultBrush = GetEdgeBrush(dropOverlayPosition, defaultColor);
                _hoverBrush = GetEdgeBrush(dropOverlayPosition, hoverColor);
                return;
            case DropOverlayPosition.Center:
                _defaultBrush = GetCenterBrush(defaultColor);
                _hoverBrush = GetCenterBrush(hoverColor);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dropOverlayPosition), dropOverlayPosition, null);
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

    private static LinearGradientBrush GetEdgeBrush(DropOverlayPosition dropOverlayPosition, Color color)
    {
        var angle = dropOverlayPosition switch
        {
            DropOverlayPosition.Top => 90,
            DropOverlayPosition.Right => 180,
            DropOverlayPosition.Bottom => 270,
            DropOverlayPosition.Left => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(dropOverlayPosition), dropOverlayPosition, null)
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