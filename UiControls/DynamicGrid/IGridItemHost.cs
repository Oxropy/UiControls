using System.ComponentModel;

namespace UiControls.DynamicGrid;

public interface IGridItemHost : INotifyPropertyChanged
{
    GridItem GridItem { get; }
}