using System.Windows.Input;

namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public sealed record ContextMenuItem : ContextMenuBase, IIsVisible
{
    public bool IsVisible => Command.CanExecute(CommandParameter);
    public required RelayCommand Command { get; init; }
    public required object? CommandParameter { get; init; }
}