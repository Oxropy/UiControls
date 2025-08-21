using System.Windows;
using UiControls.DropOverlay.ViewModel;

namespace UiControls.DropOverlay;

public class DropEventArgs
{
    public object? Sender { get; init; }
    public DragEventArgs? DragEventArgs { get; init; }
    public DropZoneViewModel? DropZone { get; init; }
}