using System.Windows;
using System.Windows.Input;

namespace UiControls.DropOverlay.ViewModel;

public abstract class DropZoneViewModel : ObservableObject
{
    public required string Identifier { get; init; }

    public double Left
    {
        get;
        private set => SetField(ref field, value);
    }

    public double Top
    {
        get;
        private set => SetField(ref field, value);
    }

    public double Width
    {
        get;
        private set => SetField(ref field, value);
    }

    public double Height
    {
        get;
        private set => SetField(ref field, value);
    }

    public double RelativeX
    {
        protected get;
        init => field = Math.Clamp(value, 0, 1);
    }

    public double RelativeY
    {
        protected get;
        init => field = Math.Clamp(value, 0, 1);
    }

    public double RelativeWidth
    {
        protected get;
        init => field = Math.Clamp(value, 0, 1);
    }

    public double RelativeHeight
    {
        protected get;
        init => field = Math.Clamp(value, 0, 1);
    }

    public ICommand? Command { get; init; }

    public void UpdatePosition(Point targetPosition, double targetWidth, double targetHeight)
    {
        Width = targetWidth * RelativeWidth;
        Height = targetHeight * RelativeHeight;
        Left = targetPosition.X + (targetWidth * RelativeX);
        Top = targetPosition.Y + (targetHeight * RelativeY);
    }
}