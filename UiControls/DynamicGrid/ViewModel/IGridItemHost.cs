using System.ComponentModel;

namespace UiControls.DynamicGrid.ViewModel;

public interface IGridItemHost : INotifyPropertyChanged
{
    GridItem GridItem { get; }
}