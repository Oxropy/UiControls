namespace UiControls.DynamicGrid.ViewModel.ContextMenu;

public sealed record ContextMenuEmpty :IContextMenu , IIsVisible
{
    public bool IsVisible => false;
}