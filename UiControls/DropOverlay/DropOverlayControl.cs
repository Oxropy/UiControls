using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UiControls.DropOverlay;

public class DropOverlayControl : Canvas
{
    private static readonly Color DefaultColor = Color.FromArgb(128, 0, 255, 0);
    private static readonly Color HoverColor = Color.FromArgb(128, 255, 165, 0);

    private static readonly DropOverlayPosition[] Positions =
    [
        DropOverlayPosition.Top,
        DropOverlayPosition.Right,
        DropOverlayPosition.Bottom,
        DropOverlayPosition.Left,
        DropOverlayPosition.Center
    ];

    private readonly DropOverlayRectangle _dropDropOverlayTop;
    private readonly DropOverlayRectangle _dropDropOverlayRight;
    private readonly DropOverlayRectangle _dropDropOverlayBottom;
    private readonly DropOverlayRectangle _dropDropOverlayLeft;
    private readonly DropOverlayRectangle _dropDropOverlayCenter;

    public DropOverlayControl()
    {
        IsHitTestVisible = false;

        _dropDropOverlayTop = new DropOverlayRectangle(DropOverlayPosition.Top, DefaultColor, HoverColor);
        _dropDropOverlayRight = new DropOverlayRectangle(DropOverlayPosition.Right, DefaultColor, HoverColor);
        _dropDropOverlayBottom = new DropOverlayRectangle(DropOverlayPosition.Bottom, DefaultColor, HoverColor);
        _dropDropOverlayLeft = new DropOverlayRectangle(DropOverlayPosition.Left, DefaultColor, HoverColor);
        _dropDropOverlayCenter = new DropOverlayRectangle(DropOverlayPosition.Center, DefaultColor, HoverColor);
    }

    public void Show(FrameworkElement target)
    {
        _dropDropOverlayTop.AddToCanvas(this);
        _dropDropOverlayRight.AddToCanvas(this);
        _dropDropOverlayBottom.AddToCanvas(this);
        _dropDropOverlayLeft.AddToCanvas(this);
        _dropDropOverlayCenter.AddToCanvas(this);

        UpdatePosition(target);
    }

    public void Hide()
    {
        _dropDropOverlayTop.RemoveFromCanvas(this);
        _dropDropOverlayRight.RemoveFromCanvas(this);
        _dropDropOverlayBottom.RemoveFromCanvas(this);
        _dropDropOverlayLeft.RemoveFromCanvas(this);
        _dropDropOverlayCenter.RemoveFromCanvas(this);
    }

    public DropOverlayPosition GetDropPosition(Point position, FrameworkElement target)
    {
        // Convert the position to be relative to the overlay canvas
        Point canvasPosition = target.TranslatePoint(position, this);

        // Check each rectangle
        DropOverlayRectangle?[] overlayRectangles =
        [
            _dropDropOverlayTop,
            _dropDropOverlayRight,
            _dropDropOverlayBottom,
            _dropDropOverlayLeft,
            _dropDropOverlayCenter
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

        return DropOverlayPosition.Unknown;
    }

    public void UpdateHighlight(DropOverlayPosition dropOverlayPosition)
    {
        SetDefaultBrush();

        DropOverlayRectangle? rectangleToHighlight = dropOverlayPosition switch
        {
            DropOverlayPosition.Top => _dropDropOverlayTop,
            DropOverlayPosition.Right => _dropDropOverlayRight,
            DropOverlayPosition.Bottom => _dropDropOverlayBottom,
            DropOverlayPosition.Left => _dropDropOverlayLeft,
            DropOverlayPosition.Center => _dropDropOverlayCenter,
            _ => null
        };

        rectangleToHighlight?.SetHoverBrush();
    }

    private void SetDefaultBrush()
    {
        _dropDropOverlayTop.SetDefaultBrush();
        _dropDropOverlayRight.SetDefaultBrush();
        _dropDropOverlayBottom.SetDefaultBrush();
        _dropDropOverlayLeft.SetDefaultBrush();
        _dropDropOverlayCenter.SetDefaultBrush();
    }

    private void UpdatePosition(FrameworkElement target)
    {
        // Get the position of the target relative to the overlay canvas
        Point targetPos = target.TranslatePoint(new Point(0, 0), this);

        const double smallSize = 0.1;
        const double middleSize = 0.25;
        const double largeSize = 0.8;

        _dropDropOverlayTop.Width = target.ActualWidth * largeSize;
        _dropDropOverlayTop.Height = target.ActualHeight * smallSize;
        _dropDropOverlayTop.Left = targetPos.X + (target.ActualWidth - _dropDropOverlayTop.Width) / 2;
        _dropDropOverlayTop.Top = targetPos.Y;
        _dropDropOverlayTop.SetDefaultBrush();

        _dropDropOverlayRight.Width = target.ActualWidth * smallSize;
        _dropDropOverlayRight.Height = target.ActualHeight * largeSize;
        _dropDropOverlayRight.Left = targetPos.X + target.ActualWidth - _dropDropOverlayRight.Width;
        _dropDropOverlayRight.Top = targetPos.Y + (target.ActualHeight - _dropDropOverlayRight.Height) / 2;
        _dropDropOverlayRight.SetDefaultBrush();

        _dropDropOverlayBottom.Width = target.ActualWidth * largeSize;
        _dropDropOverlayBottom.Height = target.ActualHeight * smallSize;
        _dropDropOverlayBottom.Left = targetPos.X + (target.ActualWidth - _dropDropOverlayBottom.Width) / 2;
        _dropDropOverlayBottom.Top = targetPos.Y + target.ActualHeight - _dropDropOverlayBottom.Height;
        _dropDropOverlayBottom.SetDefaultBrush();

        _dropDropOverlayLeft.Width = target.ActualWidth * smallSize;
        _dropDropOverlayLeft.Height = target.ActualHeight * largeSize;
        _dropDropOverlayLeft.Left = targetPos.X;
        _dropDropOverlayLeft.Top = targetPos.Y + (target.ActualHeight - _dropDropOverlayLeft.Height) / 2;
        _dropDropOverlayLeft.SetDefaultBrush();

        _dropDropOverlayCenter.Width = target.ActualWidth * middleSize;
        _dropDropOverlayCenter.Height = target.ActualHeight * middleSize;
        _dropDropOverlayCenter.Left = targetPos.X + target.ActualWidth / 2 - _dropDropOverlayCenter.Width / 2;
        _dropDropOverlayCenter.Top = targetPos.Y + target.ActualHeight / 2 - _dropDropOverlayCenter.Height / 2;
        _dropDropOverlayCenter.SetDefaultBrush();
    }
}