namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public sealed record ContextMenuStart<T>
{
    public required T ViewModel { get; init; }
    public required List<IContextMenu> Items { get; init; }
}