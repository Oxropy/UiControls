using System.Windows.Media;
using UiControls;
using UiControls.DynamicGrid.ViewModel;

namespace UiControlsExamples.DynamicGrid;

public class DefaultCellViewModel: ObservableObject, IGridItemHost
{
    public required GridItem GridItem { get; init; }
    public Brush Brush { get; }

    public DefaultCellViewModel()
    {
        var random = new Random();
        Brush = new SolidColorBrush(Color.FromArgb(255, (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
    }
}