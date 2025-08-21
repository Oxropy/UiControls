using System.Windows.Input;

namespace UiControls.DropOverlay.ViewModel;

public interface IDropOverlayHost
{
    DropOverlayViewModel DropOverlayViewModel { get; }
    ICommand HandleDropCommand { get; }
    ICommand ShouldShowOverlayCommand { get; }
}