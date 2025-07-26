using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace UiControls.DropOverlay.View;

public class DropOverlayControl : Canvas
{
    public static readonly DependencyProperty TemplatesProperty = DependencyProperty.Register(
        nameof(Templates), typeof(Collection<DropOverlayTemplate>), typeof(DropOverlayControl), new PropertyMetadata(new Collection<DropOverlayTemplate>()));

    public Collection<DropOverlayTemplate> Templates
    {
        get => (Collection<DropOverlayTemplate>)GetValue(TemplatesProperty);
        set => SetValue(TemplatesProperty, value);
    }
    
    private readonly Dictionary<DropOverlayPosition, DropOverlayRectangle> _overlayRectangles = new();

    public DropOverlayControl()
    {
        IsHitTestVisible = false;

        foreach (var position in Enum.GetValues<DropOverlayPosition>())
        {
            if (position == DropOverlayPosition.Unknown) continue;
            _overlayRectangles[position] = new DropOverlayRectangle(position);
        }
    }

    public void Show(FrameworkElement target)
    {
        foreach (var template in Templates)
        {
            if (_overlayRectangles.TryGetValue(template.Position, out var rectangle))
            {
                rectangle.Content = template.Content;
            }
        }

        foreach (var rectangle in _overlayRectangles.Values)
        {
            rectangle.AddToCanvas(this);
        }


        UpdatePosition(target);
    }

    public void Hide()
    {
        foreach (var rectangle in _overlayRectangles.Values)
        {
            rectangle.RemoveFromCanvas(this);
        }
    }

    public DropOverlayPosition GetDropPosition(Point position, FrameworkElement target)
    {
        // Convert the position to be relative to the overlay canvas
        Point canvasPosition = target.TranslatePoint(position, this);

        // Check each rectangle
        foreach (var rectangle in _overlayRectangles)
        {
            if (rectangle.Value.Bounds.Contains(canvasPosition))
            {
                return rectangle.Key;
            }
        }

        return DropOverlayPosition.Unknown;
    }
    
    private void UpdatePosition(FrameworkElement target)
    {
        // Get the position of the target relative to the overlay canvas
        Point targetPos = target.TranslatePoint(new Point(0, 0), this);

        const double smallSize = 0.1;
        const double middleSize = 0.25;
        const double largeSize = 0.8;

        var dropDropOverlayTop = _overlayRectangles[DropOverlayPosition.Top];
        dropDropOverlayTop.Width = target.ActualWidth * largeSize;
        dropDropOverlayTop.Height = target.ActualHeight * smallSize;
        dropDropOverlayTop.Left = targetPos.X + (target.ActualWidth - dropDropOverlayTop.Width) / 2;
        dropDropOverlayTop.Top = targetPos.Y;

        var dropDropOverlayRight = _overlayRectangles[DropOverlayPosition.Right];
        dropDropOverlayRight.Width = target.ActualWidth * smallSize;
        dropDropOverlayRight.Height = target.ActualHeight * largeSize;
        dropDropOverlayRight.Left = targetPos.X + target.ActualWidth - dropDropOverlayRight.Width;
        dropDropOverlayRight.Top = targetPos.Y + (target.ActualHeight - dropDropOverlayRight.Height) / 2;

        var dropDropOverlayBottom = _overlayRectangles[DropOverlayPosition.Bottom];
        dropDropOverlayBottom.Width = target.ActualWidth * largeSize;
        dropDropOverlayBottom.Height = target.ActualHeight * smallSize;
        dropDropOverlayBottom.Left = targetPos.X + (target.ActualWidth - dropDropOverlayBottom.Width) / 2;
        dropDropOverlayBottom.Top = targetPos.Y + target.ActualHeight - dropDropOverlayBottom.Height;

        var dropDropOverlayLeft = _overlayRectangles[DropOverlayPosition.Left];
        dropDropOverlayLeft.Width = target.ActualWidth * smallSize;
        dropDropOverlayLeft.Height = target.ActualHeight * largeSize;
        dropDropOverlayLeft.Left = targetPos.X;
        dropDropOverlayLeft.Top = targetPos.Y + (target.ActualHeight - dropDropOverlayLeft.Height) / 2;

        var dropDropOverlayCenter = _overlayRectangles[DropOverlayPosition.Center];
        dropDropOverlayCenter.Width = target.ActualWidth * middleSize;
        dropDropOverlayCenter.Height = target.ActualHeight * middleSize;
        dropDropOverlayCenter.Left = targetPos.X + target.ActualWidth / 2 - dropDropOverlayCenter.Width / 2;
        dropDropOverlayCenter.Top = targetPos.Y + target.ActualHeight / 2 - dropDropOverlayCenter.Height / 2;
    }
}