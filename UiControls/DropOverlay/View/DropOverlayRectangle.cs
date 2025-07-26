using System.Windows;
using System.Windows.Controls;

namespace UiControls.DropOverlay.View;

public class DropOverlayRectangle : DropOverlayTemplate
{
    private bool _isShowing;

    public DropOverlayRectangle(DropOverlayPosition dropOverlayPosition)
    {
        Position = dropOverlayPosition;
    }

    public double Left
    {
        private get => Canvas.GetLeft(this);
        set => Canvas.SetLeft(this, value);
    }

    public double Top
    {
        private get => Canvas.GetTop(this);
        set => Canvas.SetTop(this, value);
    }

    public Rect Bounds => new(Left, Top, Width, Height);

    public void AddToCanvas(Canvas canvas)
    {
        if (_isShowing) return;

        _isShowing = true;
        canvas.Children.Add(this);
    }

    public void RemoveFromCanvas(Canvas canvas)
    {
        canvas.Children.Remove(this);
        _isShowing = false;
    }
}