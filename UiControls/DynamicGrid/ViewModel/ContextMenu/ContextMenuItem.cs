namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public sealed record ContextMenuItem : ContextMenuBase
{
    public required RelayCommand Command { get; init; }
    public required object? CommandParameter { get; init; }
}