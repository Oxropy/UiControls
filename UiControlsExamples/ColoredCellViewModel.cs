using System.Windows.Media;
using UiControls;
using UiControls.DynamicGrid;

namespace UiControlsExamples;

public class ColoredCellViewModel: ObservableObject, IGridItemHost
{
    public required GridItem GridItem { get; init; }

    public required Brush Brush { get; init; }
}