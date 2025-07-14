using System.Windows.Media;
using UiControls;
using UiControls.DynamicGrid;

namespace UiControlsExamples.DynamicGrid;

public class DynamicGridShowcaseViewModel : ObservableObject, IDynamicGridManagerHost
{
    public DynamicGridShowcaseViewModel()
    {
        AddCommand = new RelayCommand(Add, CanAddRemoveExecute);
        RemoveCommand = new RelayCommand(Remove, CanAddRemoveExecute);
        ResetCommand = new RelayCommand(Reset);
    }

    private bool CanAddRemoveExecute(object? arg)
    {
        return !string.IsNullOrWhiteSpace(XPosition) 
               && !string.IsNullOrWhiteSpace(YPosition) 
               && int.TryParse(XPosition, out _) 
               && int.TryParse(YPosition, out _);
    }

    private void Add(object? obj)
    {
        if (!int.TryParse(XPosition, out var x) || !int.TryParse(YPosition, out var y)) 
            return;

        var color = Colors.DeepPink;
        if (!string.IsNullOrWhiteSpace(ColorHash))
        {
            color = (Color)ColorConverter.ConvertFromString(ColorHash);
        }
        
        DynamicGridManager.AddItem(new ColoredCellViewModel
            { Brush = new SolidColorBrush(color), GridItem = new GridItem { Row = y, Column = x } });
    }

    private void Remove(object? obj)
    {
        if (!int.TryParse(XPosition, out var x) || !int.TryParse(YPosition, out var y)) 
            return;
        
        var item = DynamicGridManager.Items.FirstOrDefault(i => i.GridItem.Row == y && i.GridItem.Column == x);
        if (item is null)
            return;
            
        DynamicGridManager.RemoveItem(item);
    }

    private void Reset(object? obj)
    {
        DynamicGridManager.ResetItems();
    }

    public DynamicGridManager DynamicGridManager { get; } = new(v => new DefaultCellViewModel
        { GridItem = new GridItem { Row = v.row, Column = v.column, RowSpan = v.rowSpan, ColumnSpan = v.columnSpan } });

    public RelayCommand AddCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public RelayCommand ResetCommand { get; }

    public string? XPosition
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                AddCommand.RaiseCanExecuteChanged();
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }
    } = "0";

    public string? YPosition
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                AddCommand.RaiseCanExecuteChanged();
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }
    } = "0";

    public string? ColorHash
    {
        get;
        set => SetField(ref field, value);
    } = "#FFFF1493";

    public void SetDefaultCells()
    {
        DynamicGridManager.AddItems(
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Red), GridItem = new GridItem { Row = 0, Column = 0 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Blue), GridItem = new GridItem { Row = 0, Column = 1 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Orange), GridItem = new GridItem { Row = 0, Column = 2 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Green), GridItem = new GridItem { Row = 1, Column = 0 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Yellow), GridItem = new GridItem { Row = 1, Column = 1 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Purple), GridItem = new GridItem { Row = 1, Column = 2 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Cyan), GridItem = new GridItem { Row = 2, Column = 0 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Magenta), GridItem = new GridItem { Row = 2, Column = 1 } },
            new ColoredCellViewModel
                { Brush = new SolidColorBrush(Colors.Bisque), GridItem = new GridItem { Row = 2, Column = 2 } });
    }
}