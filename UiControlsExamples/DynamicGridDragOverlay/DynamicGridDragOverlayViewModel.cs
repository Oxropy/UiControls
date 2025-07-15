using System.Windows.Media;
using UiControls;
using UiControls.DynamicGrid;
using UiControlsExamples.DynamicGrid;

namespace UiControlsExamples.DynamicGridDragOverlay;

public class DynamicGridDragOverlayViewModel : ObservableObject, IDynamicGridManagerHost
{
    public DynamicGridDragOverlayViewModel()
    {
        AddCommand = new RelayCommand(Add, CanAddRemoveExecute);
        RemoveCommand = new RelayCommand(Remove, CanAddRemoveExecute);
        ResetCommand = new RelayCommand(Reset);
        
        DynamicGridManager.ResetItems();
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
}