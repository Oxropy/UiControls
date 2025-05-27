using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace UiControls;

public sealed class DynamicGrid : Grid
{
    private enum RowPosition
    {
        Above,
        Below,
        Top,
        Bottom
    }

    private enum ColumnPosition
    {
        ToLeft,
        ToRight,
        Left,
        Right
    }

    public static readonly DependencyProperty MinRowsProperty = DependencyProperty.Register(
        nameof(MinRows), typeof(int), typeof(DynamicGrid), new PropertyMetadata(1), IsIntValueGreaterThanZero);

    public static readonly DependencyProperty MinColumnsProperty = DependencyProperty.Register(
        nameof(MinColumns), typeof(int), typeof(DynamicGrid), new PropertyMetadata(1), IsIntValueGreaterThanZero);

    public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
        nameof(MaxRows), typeof(int), typeof(DynamicGrid), new PropertyMetadata(10), IsIntValueGreaterThanZero);

    public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
        nameof(MaxColumns), typeof(int), typeof(DynamicGrid), new PropertyMetadata(10), IsIntValueGreaterThanZero);
    
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        "IsSelected", typeof(bool), typeof(DynamicGrid), new PropertyMetadata(false));
    static DynamicGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid), new FrameworkPropertyMetadata(typeof(DynamicGrid)));
    }

    public event EventHandler<CellChangedEventArgs>? CellChanged;
    public Func<UIElement>? GetDefaultCell { get; set; }
    
    public DynamicGrid()
    {
        MouseRightButtonDown += Grid_OnMouseRightButtonDown;
    }

    public int MinRows
    {
        get => (int)GetValue(MinRowsProperty);
        set => SetValue(MinRowsProperty, value);
    }

    public int MinColumns
    {
        get => (int)GetValue(MinColumnsProperty);
        set => SetValue(MinColumnsProperty, value);
    }

    public int MaxRows
    {
        get => (int)GetValue(MaxRowsProperty);
        set => SetValue(MaxRowsProperty, value);
    }

    public int MaxColumns
    {
        get => (int)GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    public static bool GetIsSelected(UIElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        
        return (bool)element.GetValue(IsSelectedProperty);
    }
    
    public static void SetIsSelected(UIElement element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        
        element.SetValue(IsSelectedProperty, value);
    }
    
    private void AddRowAbove_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            AddRow(element, RowPosition.Above);
    }
    
    private void AddRowBelow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            AddRow(element, RowPosition.Below);
    }
    
    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            RemoveRow(element);
    }

    private void AddColumnToLeft_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            AddColumn(element, ColumnPosition.ToLeft);
    }
    
    private void AddColumnToRight_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            AddColumn(element, ColumnPosition.ToRight);
    }
    
    private void RemoveColumn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: UIElement element }) 
            RemoveColumn(element);
    }

    private void MergeCells_Click(object sender, RoutedEventArgs e)
    {
        var selectableGridCells = GetSelectedCells();
        var (topLeft, bottomRight) = GetSelectedSquare(selectableGridCells);

        UIElement? cellToExpand = null;
        if (sender is MenuItem { Tag: UIElement element })
        {
            var row = GetRow(element);
            var column = GetColumn(element);
            if (topLeft.X <= column && topLeft.Y <= row && bottomRight.X >= column && bottomRight.Y >= row)
            {
                cellToExpand = element;
            }
        }

        if (cellToExpand is null)
        {
            cellToExpand = selectableGridCells.FirstOrDefault(c => GetRow(c) == topLeft.Y && GetColumn(c) == topLeft.X);
            if (cellToExpand is null)
            {
                return;
            }
        }

        MergeCells(topLeft, bottomRight, cellToExpand);
    }

    private void MergeCells(Point topLeft, Point bottomRight, UIElement elementToExpand)
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            UIElement element = Children[i];
            if (element == elementToExpand)
                continue;

            var row = GetRow(element);
            var column = GetColumn(element);
            if (row < topLeft.Y || row > bottomRight.Y || column < topLeft.X || column > bottomRight.X) continue;
            
            Children.Remove(element);
            OnCellChanged(CellsChangeType.Remove, element, row, column);
        }

        var originalRow = GetRow(elementToExpand);
        var originalColumn = GetColumn(elementToExpand);

        SetRow(elementToExpand, topLeft.Y);
        SetColumn(elementToExpand, topLeft.X);
        SetRowSpan(elementToExpand, bottomRight.Y - topLeft.Y + 1);
        SetColumnSpan(elementToExpand, bottomRight.X - topLeft.X + 1);
        
        OnCellChanged(CellsChangeType.SizeIncreased, elementToExpand, originalRow, originalColumn);
        
        SetIsSelected(elementToExpand, false);
    }

    private void SplitMerge_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: UIElement element })
        {
            return;
        }

        var rowIndex = GetRow(element);
        var columnIndex = GetColumn(element);
        var rowSpan = GetRowSpan(element);
        var columnSpan = GetColumnSpan(element);
        
        SetRowSpan(element, 1);
        SetColumnSpan(element, 1);
        
        OnCellChanged(CellsChangeType.SizeDecreased, element, rowIndex, columnIndex);

        for (var row = 0; row <= rowSpan; row++)
        {
            for (var column = 0; column <= columnSpan; column++)
            {
                if (row == 0 && column == 0)
                {
                    continue;
                }
                
                AddEmptyCell(rowIndex + row, columnIndex + column);
            }
        }
    }

    private void SplitCellHorizontal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: UIElement element })
        {
            return;
        }

        var row = GetRow(element);
        var columnIndex = GetColumn(element);
        var rowSpan = GetRowSpan(element);
        var columnSpan = GetColumnSpan(element);
        var newColumnSpan = columnSpan / 2;
        
        SetColumnSpan(element, newColumnSpan);
        
        OnCellChanged(CellsChangeType.SizeDecreased, element, row, columnIndex);
       
        for (var column = 1; column <= columnSpan; column++)
        {
            AddEmptyCell(row, GetColumn(element) + column, rowSpan, newColumnSpan);
        }
    }
    
    private void SplitCellVertical_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: UIElement element })
        {
            return;
        }

        var rowIndex = GetRow(element);
        var column = GetColumn(element);
        var rowSpan = GetRowSpan(element);
        var columnSpan = GetColumnSpan(element);
        var newRowSpan = rowSpan / 2;

        SetRowSpan(element, newRowSpan);
        
        OnCellChanged(CellsChangeType.SizeDecreased, element, rowIndex, column);
       
        for (var row = 1; row <= rowSpan; row++)
        {
            AddEmptyCell(GetRow(element) + row, column, newRowSpan, columnSpan);
        }
    }
    
    private void Grid_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement clickedElement)
        {
            clickedElement.ContextMenu = AssignContextMenu(clickedElement);
            clickedElement.ContextMenu.IsOpen = true;
        }
    }
    
    private ContextMenu AssignContextMenu(UIElement element)
    {
        var contextMenu = new ContextMenu();
        AddSelectMenuItem(element, contextMenu);
        AddRowMenuItems(contextMenu, element);
        AddColumnMenuItems(contextMenu, element);
        AddMergeMenuItem(contextMenu, element);
        AddSplitMenuItem(contextMenu, element);

        return contextMenu;
    }

    private static void AddSelectMenuItem(UIElement element, ContextMenu contextMenu)
    {
        var isSelected = GetIsSelected(element);
        var selectItem = new MenuItem { Header = isSelected ? "Unselect" : "Select", Tag = element};
        selectItem.Click += (_, _) => SetIsSelected(element, !GetIsSelected(element));
        contextMenu.Items.Add(selectItem);
    }

    private void AddRowMenuItems(ContextMenu contextMenu, UIElement element)
    {
        if (RowDefinitions.Count == MinRows && MinRows == MaxRows)
        {
            return;
        }

        if (RowDefinitions.Count < MaxRows)
        {
            var addRowItem = new MenuItem { Header = "Add Row" };
            var addRowAboveItem = new MenuItem { Header = "Above", Tag = element };
            addRowAboveItem.Click += AddRowAbove_Click;
            var addRowBelowItem = new MenuItem { Header = "Below", Tag = element };
            addRowBelowItem.Click += AddRowBelow_Click;
            addRowItem.Items.Add(addRowAboveItem);
            addRowItem.Items.Add(addRowBelowItem);
            contextMenu.Items.Add(addRowItem);
        }
        
        if (RowDefinitions.Count > MinRows)
        {
            var removeRowItem = new MenuItem { Header = "Remove Row", Tag = element };
            removeRowItem.Click += RemoveRow_Click;
            contextMenu.Items.Add(removeRowItem);
        }
    }

    private void AddColumnMenuItems(ContextMenu contextMenu, UIElement element)
    {
        if (ColumnDefinitions.Count == MinColumns && MinColumns == MaxColumns)
        {
            return;
        }

        if (ColumnDefinitions.Count < MaxColumns)
        {
            var addColumnItem = new MenuItem { Header = "Add Column" };
            var addColumnAboveItem = new MenuItem { Header = "Left", Tag = element };
            addColumnAboveItem.Click += AddColumnToLeft_Click;
            var addColumnBelowItem = new MenuItem { Header = "Right", Tag = element };
            addColumnBelowItem.Click += AddColumnToRight_Click;
            addColumnItem.Items.Add(addColumnAboveItem);
            addColumnItem.Items.Add(addColumnBelowItem);
            contextMenu.Items.Add(addColumnItem);
        }

        if (ColumnDefinitions.Count > MinColumns)
        {
            var removeColumnItem = new MenuItem { Header = "Remove Column", Tag = element };
            removeColumnItem.Click += RemoveColumn_Click;
            contextMenu.Items.Add(removeColumnItem);
        }
    }

    private void AddMergeMenuItem(ContextMenu contextMenu, UIElement element)
    {
        if (GetSelectedCells().Count < 2)
        {
            return;
        }
        
        var mergeItem = new MenuItem { Header = "Merge selected Cells", Tag = element };
        mergeItem.Click += MergeCells_Click;
        contextMenu.Items.Add(mergeItem);
    }

    private void AddSplitMenuItem(ContextMenu contextMenu, UIElement element)
    {
        var hasRowSpan = GetRowSpan(element) > 1;
        var hasColumnSpan = GetColumnSpan(element) > 1;
        if (!hasRowSpan && !hasColumnSpan )
        {
            return;
        }
        
        var splitItem = new MenuItem { Header = "Split Cell" };
        var splitMerge = new MenuItem { Header = "Split Merge", Tag = element };
        splitMerge.Click += SplitMerge_Click;
        splitItem.Items.Add(splitMerge);
        
        if (hasColumnSpan)
        {
            var splitHorizontalItem = new MenuItem { Header = "Split Horizontal", Tag = element };
            splitHorizontalItem.Click += SplitCellHorizontal_Click;
            splitItem.Items.Add(splitHorizontalItem);
        }

        if (hasRowSpan)
        {
            var splitVerticalItem = new MenuItem { Header = "Split Vertical", Tag = element };
            splitVerticalItem.Click += SplitCellVertical_Click;
            splitItem.Items.Add(splitVerticalItem);
        }
        
        contextMenu.Items.Add(splitItem);
    }

    private List<UIElement> GetSelectedCells()
    {
        return Children.OfType<UIElement>().Where(GetIsSelected).ToList();
    }

    private static (Point topLeft, Point bottomRight) GetSelectedSquare(IList<UIElement> cells)
    {
        var positionData = cells.Select(c => (Row: GetRow(c), Column: GetColumn(c), RowSpan: GetRowSpan(c), ColumnSpan: GetColumnSpan(c))).ToList();
        if (positionData.Count == 0)
        {
            return (new Point(-1, -1), new Point(-1, -1));
        }
        
        var rowStart = positionData.Select(position => position.Row).Min();
        var rowEnd = positionData.Select(position => position.Row + position.RowSpan - 1).Max();
        var columnStart = positionData.Select(position => position.Column).Min();
        var columnEnd = positionData.Select(position => position.Column + position.ColumnSpan - 1).Max();

        return (new Point(columnStart, rowStart), new Point(columnEnd, rowEnd));
    }

    private void AddEmptyCell(int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        var cell = GetDefaultCell?.Invoke();
        if (cell == null) return;

        SetRow(cell, row);
        SetRowSpan(cell, rowSpan);
        SetColumn(cell, column);
        SetColumnSpan(cell, columnSpan);
        Children.Add(cell);
        OnCellChanged(CellsChangeType.Add, cell, row, column);
    }

    private static bool IsIntValueGreaterThanZero(object value)
    {
        return (int)value > 0;
    }
    
    #region Add/Remove Row

    private void AddRow(UIElement element, RowPosition position)
    {
        var rowIndex = position switch
        {
            RowPosition.Above => GetRow(element),
            RowPosition.Below => GetRow(element) + GetRowSpan(element),
            RowPosition.Top => 0,
            RowPosition.Bottom => RowDefinitions.Count,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        AddRow(rowIndex);
    }

    private void AddRow(int rowIndex)
    {
        InsertNewRowDefinition(rowIndex);
        ShiftGridCellsDown(rowIndex);
        InsertRowWithBorderBackground(rowIndex);
    }

    private void InsertNewRowDefinition(int rowIndex)
    {
        var newRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
        RowDefinitions.Insert(rowIndex, newRow);
    }

    private void InsertRowWithBorderBackground(int rowIndex)
    {
        for (int col = 0; col < ColumnDefinitions.Count; col++)
        {
            AddEmptyCell(rowIndex, col);
        }
    }

    private void ShiftGridCellsDown(int startIndex)
    {
        foreach (UIElement element in Children)
        {
            int row = GetRow(element);
            if (row >= startIndex)
            {
                SetRow(element, row + 1);
            }
        }
    }
    
    private void RemoveRow(UIElement element)
    {
        var rowIndex = GetRow(element);
        RemoveChildrenInRow(rowIndex);

        RowDefinitions.RemoveAt(rowIndex);

        ShiftGridCellsUp(rowIndex);
    }

    private void RemoveChildrenInRow(int rowIndex)
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            UIElement element = Children[i];
            if (GetRow(element) != rowIndex) continue;

            var column = GetColumn(element);
            Children.Remove(element);
            OnCellChanged(CellsChangeType.SizeDecreased, element, rowIndex, column);
        }
    }

    private void ShiftGridCellsUp(int rowIndex)
    {
        foreach (UIElement element in Children)
        {
            int currentRow = GetRow(element);
            if (currentRow > rowIndex)
            {
                SetRow(element, currentRow - 1);
            }
        }
    }

    #endregion
    
    #region Add/Remove Column

    private void AddColumn(UIElement element, ColumnPosition position)
    {
        var columnIndex = position switch
        {
            ColumnPosition.ToLeft => GetColumn(element),
            ColumnPosition.ToRight => GetColumn(element) + GetColumnSpan(element),
            ColumnPosition.Left => 0,
            ColumnPosition.Right => ColumnDefinitions.Count,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        AddColumn(columnIndex);
    }

    private void AddColumn(int columnIndex)
    {
        InsertNewColumnDefinition(columnIndex);
        ShiftGridCellsRight(columnIndex);
        InsertColumnWithBorderBackground(columnIndex);
    }

    private void InsertNewColumnDefinition(int rowIndex)
    {
        var newColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
        ColumnDefinitions.Insert(rowIndex, newColumn);
    }

    private void InsertColumnWithBorderBackground(int columnIndex)
    {
        int rowCount = RowDefinitions.Count;
        for (int row = 0; row < rowCount; row++)
        {
            AddEmptyCell(row, columnIndex);
        }
    }

    private void ShiftGridCellsRight(int startIndex)
    {
        foreach (UIElement element in Children)
        {
            int column = GetColumn(element);
            if (column >= startIndex)
            {
                SetColumn(element, column + 1);
            }
        }
    }
    
    private void RemoveColumn(UIElement element)
    {
        var columnIndex = GetColumn(element);
        RemoveChildrenInColumn(columnIndex);

        ColumnDefinitions.RemoveAt(columnIndex);

        ShiftGridCellsLeft(columnIndex);
    }

    private void RemoveChildrenInColumn(int columnIndex)
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            UIElement element = Children[i];
            if (GetColumn(element) != columnIndex) continue;

            var row = GetRow(element);
            Children.Remove(element);
            OnCellChanged(CellsChangeType.Remove, element, row, columnIndex);
        }
    }

    private void ShiftGridCellsLeft(int columnIndex)
    {
        foreach (UIElement element in Children)
        {
            int currentColumn = GetColumn(element);
            if (currentColumn > columnIndex)
            {
                SetColumn(element, currentColumn - 1);
            }
        }
    }

    #endregion

    private void OnCellChanged(CellsChangeType changeType, UIElement element, int rowIndex, int columnIndex)
    {
        object? dataContext = null;
        if (element is FrameworkElement frameworkElement)
        {
            dataContext = frameworkElement.DataContext;
        }
        
        OnCellChanged(new CellChangedEventArgs(changeType, new Point(rowIndex, columnIndex), dataContext));
    }
    
    private void OnCellChanged(CellChangedEventArgs e)
    {
        CellChanged?.Invoke(this, e);
    }
}