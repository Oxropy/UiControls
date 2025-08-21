using System.Collections.ObjectModel;

namespace UiControls.DropOverlay.ViewModel;

public class DropOverlayViewModel : ObservableObject
{
    public required ObservableCollection<DropZoneViewModel> DropZones { get; init; }
    public DropZoneViewModel? DefaultDropZone { get; init; }
}