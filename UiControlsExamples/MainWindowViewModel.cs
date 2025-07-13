using System.Windows.Media;
using UiControls;
using UiControls.DynamicGrid;

namespace UiControlsExamples;

public class MainWindowViewModel : ObservableObject, IDynamicGridManagerHost
{
    public DynamicGridManager DynamicGridManager { get; } = new(v => new DefaultCellViewModel
        { GridItem = new GridItem { Row = v.row, Column = v.column, RowSpan = v.rowSpan, ColumnSpan = v.columnSpan } });

    public void SetDefaultCells()
    {
        DynamicGridManager.AddItems(
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Red), GridItem = new GridItem { Row = 0, Column = 0 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Blue), GridItem = new GridItem { Row = 0, Column = 1 } },
            // new ColoredCellViewModel
            //     { Brush = new SolidColorBrush(Colors.Orange), GridItem = new GridItem { Row = 0, Column = 2 } },
            // new ColoredCellViewModel
            //     { Brush = new SolidColorBrush(Colors.Green), GridItem = new GridItem { Row = 1, Column = 0 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Yellow), GridItem = new GridItem { Row = 1, Column = 1 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Purple), GridItem = new GridItem { Row = 1, Column = 2 } },
            // new ColoredCellViewModel
            //     { Brush = new SolidColorBrush(Colors.Cyan), GridItem = new GridItem { Row = 2, Column = 0 } },
            // new ColoredCellViewModel
            //     { Brush = new SolidColorBrush(Colors.Magenta), GridItem = new GridItem { Row = 2, Column = 1 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Bisque), GridItem = new GridItem { Row = 2, Column = 2 } });
    }
}