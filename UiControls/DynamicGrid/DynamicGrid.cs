using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UiControls.DynamicGrid;

public sealed class DynamicGrid : Grid
{
    private readonly Dictionary<Type, Type> _viewRegistry = new();
    private readonly Dictionary<IGridItemHost, FrameworkElement> _viewModelViewMapping = new();
    
    internal static readonly DependencyProperty MinRowsProperty = DependencyProperty.Register(
        nameof(MinRows), typeof(int), typeof(DynamicGrid), new PropertyMetadata(1), IsIntValueGreaterThanZero);

    internal static readonly DependencyProperty MinColumnsProperty = DependencyProperty.Register(
        nameof(MinColumns), typeof(int), typeof(DynamicGrid), new PropertyMetadata(1), IsIntValueGreaterThanZero);

    internal static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
        nameof(MaxRows), typeof(int), typeof(DynamicGrid), new PropertyMetadata(10), IsIntValueGreaterThanZero);

    internal static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
        nameof(MaxColumns), typeof(int), typeof(DynamicGrid), new PropertyMetadata(10), IsIntValueGreaterThanZero);

    static DynamicGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid),
            new FrameworkPropertyMetadata(typeof(DynamicGrid)));
    }

    public event EventHandler<IEnumerable<UIElement>>? ItemsChanged;
    
    public DynamicGrid()
    {
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        MouseRightButtonDown += OnMouseRightButtonDown;
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
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

    public void RegisterViewType<TView, TViewModel>() where TView : FrameworkElement where TViewModel : IGridItemHost
    {
        _viewRegistry[typeof(TViewModel)] = typeof(TView);
    }
    
    private FrameworkElement CreateViewForViewModel(IGridItemHost viewModel)
    {
        if (!_viewRegistry.TryGetValue(viewModel.GetType(), out var viewType))
        {
            throw new InvalidOperationException($"No view registered for view type: {nameof(viewModel)}");
        }

        if (_viewModelViewMapping.TryGetValue(viewModel, out var view))
        {
            return view;
        }
        
        view = (FrameworkElement)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;
        _viewModelViewMapping[viewModel] = view;
        return view;
    }

    private void UpdateGridItems(IList<IGridItemHost> items)
    {
        Children.Clear();

        UpdateRowDefinitions(items);
        UpdateColumnDefinitions(items);

        foreach (var item in items)
        {
            var element = CreateViewForViewModel(item);
            SetRow(element, item.GridItem.Row);
            SetColumn(element, item.GridItem.Column);
            SetRowSpan(element, item.GridItem.RowSpan);
            SetColumnSpan(element, item.GridItem.ColumnSpan);
            Children.Add(element);
        }
    }

    private void UpdateRowDefinitions(IList<IGridItemHost> items)
    {
        if (!items.Any())
        {
            return;
        }
        
        int rowDefinitionsCount = items.Max(i => i.GridItem.Row + i.GridItem.RowSpan);
        if (rowDefinitionsCount > RowDefinitions.Count)
        {
            for (int i = RowDefinitions.Count; i < rowDefinitionsCount; i++)
                RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        } 
        else if (rowDefinitionsCount < RowDefinitions.Count)
        {
            RowDefinitions.RemoveRange(RowDefinitions.Count - 1, RowDefinitions.Count - rowDefinitionsCount);
        }
    }

    private void UpdateColumnDefinitions(IList<IGridItemHost> items)
    {
        if (!items.Any())
        {
            return;
        }
        
        int columnDefinitionsCount = items.Max(i => i.GridItem.Column + i.GridItem.ColumnSpan);
        if (columnDefinitionsCount > ColumnDefinitions.Count)
        {
            for (int i = ColumnDefinitions.Count; i < columnDefinitionsCount; i++)
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        } 
        else if (columnDefinitionsCount < ColumnDefinitions.Count)
        {
            ColumnDefinitions.RemoveRange(ColumnDefinitions.Count - 1, ColumnDefinitions.Count - columnDefinitionsCount);
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

    private void AddSelectMenuItem(UIElement element, ContextMenu contextMenu)
    {
        if (element is not FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            return;
        }

        var selectItem = new MenuItem
        {
            Header = gridItemHost.GridItem.IsSelected ? "Unselect" : "Select",
            Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.SelectCommand,
            CommandParameter = gridItemHost.GridItem
        };
        
        contextMenu.Items.Add(selectItem);
    }

    private void AddRowMenuItems(ContextMenu contextMenu, UIElement element)
    {
        if (RowDefinitions.Count == MinRows && MinRows == MaxRows)
        {
            return;
        }
        
        if (element is not FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            return;
        }

        if (RowDefinitions.Count < MaxRows)
        {
            var addRowItem = new MenuItem { Header = "Add Row" };
            var addRowAboveItem = new MenuItem
            {
                Header = "Above",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.AddRowAboveCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            var addRowBelowItem = new MenuItem
            {
                Header = "Below",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.AddRowBelowCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            addRowItem.Items.Add(addRowAboveItem);
            addRowItem.Items.Add(addRowBelowItem);
            contextMenu.Items.Add(addRowItem);
        }

        if (RowDefinitions.Count > MinRows)
        {
            var removeRowItem = new MenuItem
            {
                Header = "Remove Row",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.RemoveRowCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            contextMenu.Items.Add(removeRowItem);
        }
    }

    private void AddColumnMenuItems(ContextMenu contextMenu, UIElement element)
    {
        if (ColumnDefinitions.Count == MinColumns && MinColumns == MaxColumns)
        {
            return;
        }

        if (element is not FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            return;
        }
        
        if (ColumnDefinitions.Count < MaxColumns)
        {
            var addColumnItem = new MenuItem { Header = "Add Column" };
            var addColumnAboveItem = new MenuItem
            {
                Header = "Left",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.AddColumnToLeftCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            var addColumnBelowItem = new MenuItem
            {
                Header = "Right",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.AddColumnToRightCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            addColumnItem.Items.Add(addColumnAboveItem);
            addColumnItem.Items.Add(addColumnBelowItem);
            contextMenu.Items.Add(addColumnItem);
        }

        if (ColumnDefinitions.Count > MinColumns)
        {
            var removeColumnItem = new MenuItem
            {
                Header = "Remove Column",
                Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.RemoveColumnCommand,
                CommandParameter = gridItemHost.GridItem
            };
            
            contextMenu.Items.Add(removeColumnItem);
        }
    }

    private void AddMergeMenuItem(ContextMenu contextMenu, UIElement element)
    {
        if (element is not FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            return;
        }
        
        var mergeItem = new MenuItem
        {
            Header = "Merge selected Cells",
            Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.MergeCellsCommand,
            CommandParameter = gridItemHost.GridItem
        };
        
        contextMenu.Items.Add(mergeItem);
    }

    private void AddSplitMenuItem(ContextMenu contextMenu, UIElement element)
    {
        if (element is not FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            return;
        }

        var splitItem = new MenuItem { Header = "Split Cell" };
        var splitMerge = new MenuItem
        {
            Header = "Split Merge",
            Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.SplitMergeCommand,
            CommandParameter = gridItemHost.GridItem
        };

        splitItem.Items.Add(splitMerge);

        var splitHorizontalItem = new MenuItem
        {
            Header = "Split Horizontal",
            Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.SplitCellHorizontalCommand,
            CommandParameter = gridItemHost.GridItem
        };

        splitItem.Items.Add(splitHorizontalItem);

        var splitVerticalItem = new MenuItem
        {
            Header = "Split Vertical",
            Command = ((IDynamicGridManagerHost)DataContext).DynamicGridManager.SplitCellVerticalCommand,
            CommandParameter = gridItemHost.GridItem
        };

        splitItem.Items.Add(splitVerticalItem);

        contextMenu.Items.Add(splitItem);
    }

    private static bool IsIntValueGreaterThanZero(object value)
    {
        return (int)value > 0;
    }
    
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement clickedElement)
        {
            clickedElement.ContextMenu = AssignContextMenu(clickedElement);
            clickedElement.ContextMenu.IsOpen = true;
        }
    }
    
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is IDynamicGridManagerHost oldContext)
        {
            oldContext.DynamicGridManager.CollectionChanged -= OnCollectionChanged;
        }

        if (e.NewValue is IDynamicGridManagerHost newContext)
        {
            newContext.DynamicGridManager.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var items = e.NewItems?.Cast<IGridItemHost>().ToList() ?? null;
        if (items != null)
        {
            UpdateGridItems(items);
            ItemsChanged?.Invoke(this, items.Select(i => _viewModelViewMapping[i]).ToList());
        }
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is IDynamicGridManagerHost dataContext)
        {
            dataContext.DynamicGridManager.CollectionChanged -= OnCollectionChanged;
        }
        
        MouseRightButtonDown -= OnMouseRightButtonDown;
        DataContextChanged -= OnDataContextChanged;
        Unloaded -= OnUnloaded;
    }
}