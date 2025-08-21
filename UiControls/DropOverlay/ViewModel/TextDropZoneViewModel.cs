using System.Windows.Media;

namespace UiControls.DropOverlay.ViewModel;

public class TextDropZoneViewModel : DropZoneViewModel
{
    public required string Text { get; init; }
    public Color Foreground { get; init; } = Colors.Black;
    public Color Background { get; init; } = Colors.Transparent;
    public Color Border { get; init; } = Colors.Transparent;
    public double CornerRadius { get; init; }
    
    public Brush ForegroundBrush => new SolidColorBrush(Foreground);
    public Brush BackgroundBrush => new SolidColorBrush(Background);
    public Brush BorderBrush => new SolidColorBrush(Border);
}