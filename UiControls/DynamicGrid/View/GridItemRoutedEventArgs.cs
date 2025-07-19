using System.Windows;
using UiControls.DynamicGrid.ViewModel;

namespace UiControls.DynamicGrid.View;

public class GridItemRoutedEventArgs : RoutedEventArgs
{
    public IGridItemHost GridItemHost { get; }

    public GridItemRoutedEventArgs(RoutedEvent routedEvent, IGridItemHost gridItemHost) 
        : base(routedEvent)
    {
        GridItemHost = gridItemHost;
    }
}