using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UiControls;
using UiControls.DropOverlay;
using UiControls.DropOverlay.ViewModel;
using UiControls.DynamicGrid.ViewModel;
using UiControlsExamples.DynamicGrid;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace UiControlsExamples.DynamicGridDragOverlay;

public class DynamicGridDragOverlayViewModel : ObservableObject, IDynamicGridManagerHost, IDropOverlayHost
{
    public DynamicGridDragOverlayViewModel()
    {
        AddCommand = new RelayCommand(Add, CanAddRemoveExecute);
        RemoveCommand = new RelayCommand(Remove, CanAddRemoveExecute);
        ResetCommand = new RelayCommand(Reset);
        HandleDropCommand = new RelayCommand(HandleDrop);
        ShouldShowOverlayCommand = new RelayCommand(_ => { }, ShouldShowOverlayCanExecute);

        ObservableCollection<DropZoneViewModel> dropZoneViewModels = CreateDragOverlayZones();
        DropOverlayViewModel = new DropOverlayViewModel
        {
            DropZones = dropZoneViewModels,
            DefaultDropZone = dropZoneViewModels[0]
        };
    }

    public DynamicGridManager DynamicGridManager { get; } = new(v => new DefaultCellViewModel
        { GridItem = new GridItem { Row = v.row, Column = v.column, RowSpan = v.rowSpan, ColumnSpan = v.columnSpan } });

    public DropOverlayViewModel DropOverlayViewModel { get; }
    
    public RelayCommand AddCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public RelayCommand ResetCommand { get; }
    public ICommand HandleDropCommand { get; }
    public ICommand ShouldShowOverlayCommand { get; }

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
    
    private bool CanAddRemoveExecute(object? arg)
    {
        return !string.IsNullOrWhiteSpace(XPosition) 
               && !string.IsNullOrWhiteSpace(YPosition) 
               && int.TryParse(XPosition, out _) 
               && int.TryParse(YPosition, out _);
    }

    private void Add(object? obj)
    {
        if (!int.TryParse(XPosition, out int x) || !int.TryParse(YPosition, out int y)) 
            return;

        Color color = Colors.DeepPink;
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
    
    private void HandleDrop(object? parameter)
    {
        if (parameter is not DropEventArgs dropEventArgs)
            return;

        object? sender = dropEventArgs.Sender;
        DragEventArgs? e = dropEventArgs.DragEventArgs;
        DropZoneViewModel? dropZone = dropEventArgs.DropZone;

        if (sender is not FrameworkElement { DataContext: IGridItemHost gridItemHost } 
            || dropZone == null 
            || e?.Data.GetData(DataFormats.Text) is not string draggedText) 
            return;
        
        dropZone.Command?.Execute(gridItemHost.GridItem);
        GridItem gridItem = dropZone.Identifier switch
        {
            "Top" => gridItemHost.GridItem with
            {
                Row = gridItemHost.GridItem.Row - 1, RowSpan = 1, ColumnSpan = 1
            },
            "Right" => gridItemHost.GridItem with
            {
                Column = gridItemHost.GridItem.Column + 1, RowSpan = 1, ColumnSpan = 1
            },
            "Bottom" => gridItemHost.GridItem with
            {
                Row = gridItemHost.GridItem.Row + 1, RowSpan = 1, ColumnSpan = 1
            },
            "Left" => gridItemHost.GridItem with
            {
                Column = gridItemHost.GridItem.Column - 1, RowSpan = 1, ColumnSpan = 1
            },
            "Center" => gridItemHost.GridItem with { },
            _ => throw new ArgumentOutOfRangeException(nameof(DropZoneViewModel.Identifier))
        };

        DynamicGridManager.AddItem(new ColoredCellViewModel
        {
            Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(draggedText)), GridItem = gridItem
        });
    }
    
    private static bool ShouldShowOverlayCanExecute(object? arg)
    {
        if (arg is not DropEventArgs { DragEventArgs: { } dragEventArgs })
            return false;

        return (dragEventArgs.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey;
    }
    
    private static ObservableCollection<DropZoneViewModel> CreateDragOverlayZones()
    {
        return
        [
            new TextDropZoneViewModel
            {
                RelativeX = 0.375,
                RelativeY = 0.375,
                RelativeWidth = 0.25,
                RelativeHeight = 0.25,
                Identifier = "Center",
                Text = "",
                Background = Colors.LightBlue,
                Foreground = Colors.Black,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.1,
                RelativeY = 0,
                RelativeWidth = 0.8,
                RelativeHeight = 0.1,
                Identifier = "Top",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 0,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.9,
                RelativeY = 0.1,
                RelativeWidth = 0.1,
                RelativeHeight = 0.8,
                Identifier = "Right",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 90,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.1,
                RelativeY = 0.9,
                RelativeWidth = 0.8,
                RelativeHeight = 0.1,
                Identifier = "Bottom",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 180,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0,
                RelativeY = 0.1,
                RelativeWidth = 0.1,
                RelativeHeight = 0.8,
                Identifier = "Left",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 270,
                Background = Colors.LightBlue,
                CornerRadius = 5
            }
        ];
    }
}