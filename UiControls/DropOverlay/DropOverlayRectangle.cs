using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UiControls.DropOverlay;

public class DropOverlayRectangle : DropOverlayTemplate
{
    private readonly Color _defaultColor;
    private readonly Color _hoverColor;
    private bool _isShowing;

    public DropOverlayRectangle(DropOverlayPosition dropOverlayPosition, Color defaultColor, Color hoverColor)
    {
        _defaultColor = defaultColor;
        _hoverColor = hoverColor;
        Position = dropOverlayPosition;

        if (Content == null)
        {
            Background = new SolidColorBrush(defaultColor);
        }
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

    public void SetDefaultBrush()
    {
        if (Content is null)
        {
            Background = new SolidColorBrush(_defaultColor);
        }
    }

    public void SetHoverBrush()
    {
        if (Content is null)
        {
            Background = new SolidColorBrush(_hoverColor);
        }
    }
}