using System.Windows.Media;

namespace UiControls.DropOverlay.ViewModel;

public class ImageDropZoneViewModel : DropZoneViewModel
{
    public required string ImagePath { get; init; }
    public required double ImageWidth { get; init; }
    public required double ImageHeight { get; init; }
    public required double ImageRotation { get; init; }
    
    public Color Background { get; init; } = Colors.Transparent;
    public Color Border { get; init; } = Colors.Transparent;
    public double CornerRadius { get; init; }
    public Brush BackgroundBrush => new SolidColorBrush(Background);
    public Brush BorderBrush => new SolidColorBrush(Border);
}