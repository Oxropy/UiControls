namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public record ContextMenuList : ContextMenuBase, IIsVisible
{
    internal ContextMenuList() {}

    public bool IsVisible { get; private init; }

    public required List<IContextMenu> Items
    {
        get => field;
        init
        {
            field = value; 
            IsVisible = Items.Any(i => i is IIsVisible { IsVisible: true });
        }
    }
}