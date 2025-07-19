namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public record ContextMenuBase : IContextMenu
{
    public required string Header { get; init; }
}