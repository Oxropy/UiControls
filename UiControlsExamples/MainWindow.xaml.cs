using System.Windows.Controls;
using System.Windows.Media;
using UiControls.DynamicGrid;

namespace UiControlsExamples;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    // how can the viewmodel decide where which cell goes?
    // do i create a user control?
    // how can the style be changed from an implementer
    // add flag to add grid splitter in-between cells

    public MainWindow()
    {
        InitializeComponent();
        DynamicGrid.GetDefaultCell = () => new Border
        {
            Background = new SolidColorBrush(Colors.Gray)
        };
    }

    private void DynamicGrid_OnCellChanged(object? sender, CellChangedEventArgs e)
    {
        switch (e.ChangeType)
        {
            case CellsChangeType.Add:
                Console.WriteLine($"Cell at {e.Position.X}/{e.Position.Y} with DataContext {DataContext} was added");
                break;
            case CellsChangeType.Remove:
                Console.WriteLine($"Cell at {e.Position.X}/{e.Position.Y} with DataContext {DataContext} was removed");
                break;
            case CellsChangeType.SizeIncreased:
                Console.WriteLine($"Cell size at {e.Position.X}/{e.Position.Y} with DataContext {DataContext} was increased");
                break;
            case CellsChangeType.SizeDecreased:
                Console.WriteLine($"Cell size at {e.Position.X}/{e.Position.Y} with DataContext {DataContext} was decreased");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}