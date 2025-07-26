using System.Windows;
using System.Windows.Controls;

namespace UiControls.DropOverlay.View;

public class DropOverlayTemplate : ContentControl
{
    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        nameof(Position), typeof(DropOverlayPosition), typeof(DropOverlayTemplate), new PropertyMetadata(default(DropOverlayPosition)));

    public DropOverlayPosition Position
    {
        get => (DropOverlayPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }
}