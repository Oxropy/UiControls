using System.Collections.Specialized;
using System.Drawing;
using UiControls.DynamicGrid.View;
using UiControls.DynamicGrid.ViewModel.ContextMenu;

namespace UiControls.DynamicGrid.ViewModel;

public sealed class DynamicGridManager : ObservableObject
{
    private readonly Func<(int row, int column, int rowSpan, int columnSpan), IGridItemHost> _getDefaultCell;
    private readonly ContextMenuBuilder _contextMenuBuilder;
    private readonly GridItemHostSyncList _items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    public DynamicGridManager(Func<(int row, int column, int rowSpan, int columnSpan), IGridItemHost> getDefaultCell)
    {
        _getDefaultCell = getDefaultCell;
        _contextMenuBuilder = new ContextMenuBuilder(this);
        _items.CollectionChanged += OnItemsCollectionChanged;

        SelectCommand = new RelayCommand(SelectCell);
        AddRowAboveCommand = new RelayCommand(p => AddRow((p, RowPosition.Above)), CanAddRowExecute);
        AddRowBelowCommand = new RelayCommand(p => AddRow((p, RowPosition.Below)), CanAddRowExecute);
        AddColumnToLeftCommand = new RelayCommand(p => AddColumn((p, ColumnPosition.ToLeft)), CanAddColumnExecute);
        AddColumnToRightCommand = new RelayCommand(p => AddColumn((p, ColumnPosition.ToRight)), CanAddColumnExecute);
        RemoveRowCommand = new RelayCommand(RemoveRow, CanRemoveRowExecute);
        RemoveColumnCommand = new RelayCommand(RemoveColumn, CanRemoveColumnExecute);
        MergeCellsCommand = new RelayCommand(MergeCells, CanMergeCellsExecute);
        SplitMergeCommand = new RelayCommand(SplitMerge, CanSplitMergeExecute);
        SplitCellVerticalCommand = new RelayCommand(SplitCellVertical, CanSplitCellVerticalExecute);
        SplitCellHorizontalCommand = new RelayCommand(SplitCellHorizontal, CanSplitCellHorizontalExecute);
        CreateContextMenuCommand = new RelayCommand(CreateContextMenu);
    }

    public RelayCommand SelectCommand { get; }
    public RelayCommand AddRowAboveCommand { get; }
    public RelayCommand AddRowBelowCommand { get; }
    public RelayCommand RemoveRowCommand { get; }
    public RelayCommand AddColumnToLeftCommand { get; }
    public RelayCommand AddColumnToRightCommand { get; }
    public RelayCommand RemoveColumnCommand { get; }
    public RelayCommand MergeCellsCommand { get; }
    public RelayCommand SplitMergeCommand { get; }
    public RelayCommand SplitCellVerticalCommand { get; }
    public RelayCommand SplitCellHorizontalCommand { get; }
    public RelayCommand CreateContextMenuCommand { get; }

    public int RowDefinitionsCount { get; private set; } = 1;
    public int ColumnDefinitionsCount { get; private set; } = 1;
    
    public int MaxRows
    {
        get;
        init =>
            field = value <= ushort.MaxValue
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Max rows must be less than or equal to 65535");
    } = 10;

    public int MaxColumns
    {
        get;
        init =>
            field = value <= ushort.MaxValue
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Max columns must be less than or equal to 65535");
    } = 10;

    public int MinRows
    {
        get;
        init =>
            field = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value, "Min rows must be greater than 0");
    } = 1;

    public int MinColumns
    {
        get;
        init =>
            field = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value, "Min columns must be greater than 0");
    } = 1;

    public IReadOnlyList<IGridItemHost> Items => _items.AsReadOnly();

    public ContextMenuStart<IGridItemHost>? ContextMenuStart
    {
        get;
        private set => SetField(ref field, value);
    }
    
    public void AddItem(IGridItemHost gridItemHost)
    {
        ValidateGridItem(gridItemHost);

        var existentItemAtPosition = _items.FirstOrDefault(i => i.GridItem == gridItemHost.GridItem);
        if (existentItemAtPosition is not null)
            _items.Remove(existentItemAtPosition);

        _items.Add(gridItemHost);
        FillGridGaps();
        _items.Sync();
    }
    
    public void AddItems(params IList<IGridItemHost> gridItemHosts)
    {
        ValidateGridItems(gridItemHosts);
        
        foreach (var i in _items.Where(gridItemHosts.Contains).ToList()) 
            _items.Remove(i);
        
        _items.AddRange(gridItemHosts);
        FillGridGaps();
        _items.Sync();
    }
    
    public void RemoveItem(IGridItemHost gridItemHost)
    {
        _items.Remove(gridItemHost);
        FillGridGaps();
        _items.Sync();
    }
    
    public void ResetItems()
    {
        _items.Clear();
        FillGridGaps();
        _items.Sync();
    }
    
    private void ValidateGridItem(IGridItemHost gridItemHost)
    {
        if (gridItemHost.GridItem.Row + 1 < MinRows || gridItemHost.GridItem.Row + gridItemHost.GridItem.RowSpan > MaxRows)
        {
            throw new ArgumentOutOfRangeException(nameof(gridItemHost), "Row is out of bounds");
        }
        
        if (gridItemHost.GridItem.Column + 1 < MinColumns || gridItemHost.GridItem.Column + gridItemHost.GridItem.ColumnSpan > MaxColumns)
        {
            throw new ArgumentOutOfRangeException(nameof(gridItemHost), "Column is out of bounds");
        }
    }
    
    private void ValidateGridItems(IEnumerable<IGridItemHost> gridItemHost)
    {
        foreach (var item in gridItemHost)
        {
            ValidateGridItem(item);
        }
    }
    
    private bool IsInvalidRowConfiguration => RowDefinitionsCount != MinRows || MinRows != MaxRows;
    private bool IsInvalidColumnConfiguration => ColumnDefinitionsCount != MinColumns || MinColumns != MaxColumns;

    private bool CanAddRowExecute(object? parameter) => IsInvalidRowConfiguration && RowDefinitionsCount < MaxRows;
    private bool CanAddColumnExecute(object? parameter) => IsInvalidColumnConfiguration && ColumnDefinitionsCount < MaxColumns;
    private bool CanRemoveRowExecute(object? parameter) => IsInvalidRowConfiguration && RowDefinitionsCount > MinRows;
    private bool CanRemoveColumnExecute(object? parameter) => IsInvalidColumnConfiguration && ColumnDefinitionsCount > MinColumns;
    private bool CanMergeCellsExecute(object? parameter) => _items.Count(i => i.GridItem.IsSelected) > 1;
    private static bool CanSplitMergeExecute(object? parameter) => parameter is GridItem item && (item.ColumnSpan > 1 || item.RowSpan > 1);
    private static bool CanSplitCellVerticalExecute(object? parameter) => parameter is GridItem { ColumnSpan: > 1 };
    private static bool CanSplitCellHorizontalExecute(object? parameter) => parameter is GridItem { RowSpan: > 1 };

    private static void SelectCell(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }
        
        gridItem.IsSelected = !gridItem.IsSelected;
    }
    
    private void AddRow(object? parameter)
    {
        if (parameter is not (GridItem gridItem, RowPosition position))
        {
            return;
        }

        int row = position switch
        {
            RowPosition.Above => gridItem.Row,
            RowPosition.Below => gridItem.Row + 1,
            RowPosition.Top => 0,
            RowPosition.Bottom => RowDefinitionsCount,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
        
        // Shift existing items down
        foreach (var item in _items.Where(i => i.GridItem.Row >= row))
        {
            item.GridItem.Row++;
        }
        
        for (int col = 0; col < ColumnDefinitionsCount; col++)
        {
            _items.Add(_getDefaultCell((row, col, 1, 1)));
        }

        RowDefinitionsCount++;
        _items.Sync();
    }

    private void RemoveRow(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }

        foreach (var item in _items.Where(i => i.GridItem.Row < gridItem.Row && i.GridItem.Row + i.GridItem.RowSpan > gridItem.Row))
        {
            item.GridItem.RowSpan--;
        }
        
        foreach (var item in _items.Where(i => i.GridItem.Row == gridItem.Row).ToList())
        {
            if (item.GridItem.RowSpan == 1)
            {
                _items.Remove(item);
            }
            else
            {
                item.GridItem.RowSpan--;
            }
        }
        
        // Shift remaining items up
        foreach (var item in _items.Where(i => i.GridItem.Row > gridItem.Row))
        {
            item.GridItem.Row--;
        }

        RowDefinitionsCount--;
        _items.Sync();
    }

    private void AddColumn(object? parameter)
    {
        if (parameter is not (GridItem gridItem, ColumnPosition position))
        {
            return;
        }

        int column = position switch
        {
            ColumnPosition.ToLeft => gridItem.Column,
            ColumnPosition.ToRight => gridItem.Column + 1,
            ColumnPosition.Left => 0,
            ColumnPosition.Right => ColumnDefinitionsCount,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
        
        // Shift existing items right
        foreach (var item in _items.Where(i => i.GridItem.Column >= column))
        {
            item.GridItem.Column++;
        }

        for (int row = 0; row < RowDefinitionsCount; row++)
        {
            _items.Add(_getDefaultCell((row, column, 1, 1)));
        }

        ColumnDefinitionsCount++;
        _items.Sync();
    }

    private void RemoveColumn(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }
        
        foreach (var item in _items.Where(i => i.GridItem.Column < gridItem.Column && i.GridItem.Column + i.GridItem.ColumnSpan > gridItem.Column))
        {
            item.GridItem.ColumnSpan--;
        }
        
        foreach (var item in _items.Where(i => i.GridItem.Column == gridItem.Column).ToList())
        {
            if (item.GridItem.ColumnSpan == 1)
            {
                _items.Remove(item);
            }
            else
            {
                item.GridItem.ColumnSpan--;
            }
        }
        
        // Shift remaining items left
        foreach (var item in _items.Where(i => i.GridItem.Column > gridItem.Column))
        {
            item.GridItem.Column--;
        }

        ColumnDefinitionsCount--;
        _items.Sync();
    }

    private void MergeCells(object? parameter)
    {
        var selectableGridCells = _items.Where(i => i.GridItem.IsSelected).Select(i => i.GridItem).ToList();
        var (topLeft, bottomRight) = GetSelectedSquare(selectableGridCells);

        GridItem? cellToExpand = null;
        if (parameter is GridItem gridItem)
        {
            var row = gridItem.Row;
            var column = gridItem.Column;
            if (topLeft.X <= column && topLeft.Y <= row && bottomRight.X >= column && bottomRight.Y >= row)
            {
                cellToExpand = gridItem;
            }
        }

        if (cellToExpand is null)
        {
            cellToExpand = selectableGridCells.FirstOrDefault(c => c.Row == topLeft.Y && c.Column == topLeft.X);
            if (cellToExpand is null)
            {
                return;
            }
        }

        MergeCells(topLeft, bottomRight, cellToExpand);
    }
    
    private void MergeCells(Point topLeft, Point bottomRight, GridItem elementToExpand)
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            IGridItemHost element = _items[i];
            if (element.GridItem == elementToExpand)
                continue;

            var row = element.GridItem.Row;
            var column = element.GridItem.Column;
            if (row < topLeft.Y || row > bottomRight.Y || column < topLeft.X || column > bottomRight.X) continue;

            _items.Remove(element);
        }

        elementToExpand.Row = topLeft.Y;
        elementToExpand.Column = topLeft.X;
        elementToExpand.RowSpan = bottomRight.Y - topLeft.Y + 1;
        elementToExpand.ColumnSpan = bottomRight.X - topLeft.X + 1;

        elementToExpand.IsSelected = false;
        
        _items.Sync();
    }
    
    private static (Point topLeft, Point bottomRight) GetSelectedSquare(IList<GridItem> gridItems)
    {
        if (gridItems.Count == 0)
        {
            return (new Point(-1, -1), new Point(-1, -1));
        }

        var rowStart = gridItems.Select(position => position.Row).Min();
        var rowEnd = gridItems.Select(position => position.Row + position.RowSpan - 1).Max();
        var columnStart = gridItems.Select(position => position.Column).Min();
        var columnEnd = gridItems.Select(position => position.Column + position.ColumnSpan - 1).Max();

        return (new Point(columnStart, rowStart), new Point(columnEnd, rowEnd));
    }
    
    private void SplitMerge(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }
        
        for (var row = 0; row < gridItem.RowSpan; row++)
        {
            for (var column = 0; column < gridItem.ColumnSpan; column++)
            {
                if (row == 0 && column == 0)
                {
                    continue;
                }
                
                _items.Add(_getDefaultCell((gridItem.Row + row, gridItem.Column + column, 1, 1)));
            }
        }
        
        gridItem.RowSpan = 1;
        gridItem.ColumnSpan = 1;
        
        _items.Sync();
    }

    private void SplitCellHorizontal(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }

        for (var row = 1; row < gridItem.RowSpan; row++)
        {
            _items.Add(_getDefaultCell((gridItem.Row + row, gridItem.Column, 1, gridItem.ColumnSpan)));
        }

        gridItem.RowSpan = 1;
        
        _items.Sync();
    }

    private void SplitCellVertical(object? parameter)
    {
        if (parameter is not GridItem gridItem)
        {
            return;
        }
        
        for (var column = 1; column < gridItem.ColumnSpan; column++)
        {
            _items.Add(_getDefaultCell((gridItem.Row, gridItem.Column + column, gridItem.RowSpan, 1)));
        }
        
        gridItem.ColumnSpan = 1;
        
        _items.Sync();
    }
    
    private void CreateContextMenu(object? obj)
    {
        if (obj is not GridItemRoutedEventArgs eventArgs)
            return;
        
        ContextMenuStart = _contextMenuBuilder.Build(eventArgs.GridItemHost);
    }
    
    private void FillGridGaps()
    {
        UpdateRowDefinitionCount();
        UpdateColumnDefinitionCount();
        InsertDefaultCellsInGaps(GetOccupiedPositions());
    }

    private void UpdateRowDefinitionCount()
    {
        RowDefinitionsCount = _items.Count == 0
            ? MinRows
            : Math.Clamp(_items.Max(i => i.GridItem.Row + i.GridItem.RowSpan), MinRows, MaxRows);
    }

    private void UpdateColumnDefinitionCount()
    {
        ColumnDefinitionsCount = _items.Count == 0
            ? MinColumns
            : Math.Clamp(_items.Max(i => i.GridItem.Column + i.GridItem.ColumnSpan), MinColumns, MaxColumns);
    }

    private HashSet<int> GetOccupiedPositions()
    {
        // Integer encoding for position (row << 16 | column)
        var occupiedPositions = new HashSet<int>();

        // Mark occupied positions
        foreach (var item in _items)
        {
            var gridItem = item.GridItem;
            for (int r = gridItem.Row; r < gridItem.Row + gridItem.RowSpan; r++)
            {
                for (int c = gridItem.Column; c < gridItem.Column + gridItem.ColumnSpan; c++)
                {
                    occupiedPositions.Add((r << 16) | c);
                }
            }
        }

        return occupiedPositions;
    }

    private void InsertDefaultCellsInGaps(HashSet<int> occupiedPositions)
    {
        // Find and collect gaps
        for (int row = 0; row < RowDefinitionsCount; row++)
        {
            for (int column = 0; column < ColumnDefinitionsCount; column++)
            {
                if (!occupiedPositions.Contains((row << 16) | column))
                {
                    _items.Add(_getDefaultCell((row, column, 1, 1)));
                }
            }
        }
    }
    
    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
}