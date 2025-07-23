namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public record ContextMenuList : ContextMenuBase
{
    internal ContextMenuList() {}

    public required List<IContextMenu> Items { get; init; }
}