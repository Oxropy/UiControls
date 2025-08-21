using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using UiControls.DynamicGrid.ViewModel;
using UiControls.DynamicGrid.ViewModel.ContextMenu;

namespace UiControls.DynamicGrid.View;

public sealed class DynamicGrid : Grid
{
    private readonly ViewViewModelMapping<IGridItemHost> _viewViewModelMapping = new();

    private static readonly RoutedEvent OpenContextMenuEvent = EventManager.RegisterRoutedEvent(
        nameof(OpenContextMenu), 
        RoutingStrategy.Bubble, 
        typeof(RoutedEventHandler), 
        typeof(DynamicGrid));

    public static readonly DependencyProperty ContextMenuItemProperty = DependencyProperty.Register(
        nameof(ContextMenuItem), typeof(ContextMenuStart<IGridItemHost>), typeof(DynamicGrid), new PropertyMetadata(null, UpdateContextMenu));

    static DynamicGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid),
            new FrameworkPropertyMetadata(typeof(DynamicGrid)));
    }

    public event RoutedEventHandler OpenContextMenu
    {
        add => AddHandler(OpenContextMenuEvent, value);
        remove => RemoveHandler(OpenContextMenuEvent, value);
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
    
    public Action<FrameworkElement>? ConfigureViewBehavior { get; set; }
    
    public ContextMenuStart<IGridItemHost>? ContextMenuItem
    {
        get => (ContextMenuStart<IGridItemHost>)GetValue(ContextMenuItemProperty);
        set => SetValue(ContextMenuItemProperty, value);
    }

    public void RegisterViewType<TView, TViewModel>() where TView : FrameworkElement where TViewModel : IGridItemHost
    {
        _viewViewModelMapping.RegisterViewType<TView, TViewModel>();
    }
    
    private void UpdateGridItems(IList<IGridItemHost> items)
    {
        Children.Clear();

        UpdateRowDefinitions(items);
        UpdateColumnDefinitions(items);

        foreach (var item in items)
        {
            var element = _viewViewModelMapping.GetViewForViewModel(item, ConfigureViewBehavior);
            SetRow(element, item.GridItem.Row);
            SetColumn(element, item.GridItem.Column);
            SetRowSpan(element, item.GridItem.RowSpan);
            SetColumnSpan(element, item.GridItem.ColumnSpan);
            Children.Add(element);
        }
        
        _viewViewModelMapping.RemoveNotExistingViewModels(items);
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
            RowDefinitions.RemoveRange(rowDefinitionsCount, RowDefinitions.Count - rowDefinitionsCount);
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
            ColumnDefinitions.RemoveRange(columnDefinitionsCount, ColumnDefinitions.Count - columnDefinitionsCount);
        }
    }
    
    private static void UpdateContextMenu(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DynamicGrid dynamicGrid 
            || e.NewValue is not ContextMenuStart<IGridItemHost> contextMenuStart 
            || !dynamicGrid._viewViewModelMapping.TryGetViewForViewModel(contextMenuStart.ViewModel, out FrameworkElement? view))
            return;

        var contextMenu = new ContextMenu();
        foreach (IContextMenu item in contextMenuStart.Items)
        {
            MenuItem menuItem = CreateMenuItem(item);
            contextMenu.Items.Add(menuItem);
        }
            
        view.ContextMenu = contextMenu;
        view.ContextMenu.IsOpen = true;
    }

    private static MenuItem CreateMenuItem(IContextMenu item)
    {
        return item switch
        {
            ContextMenuItem i => CreateMenuItem(i),
            ContextMenuList l => CreateMenuItem(l),
            _ => throw new InvalidOperationException($"Unknown item type {item.GetType().Name}")
        };
    }

    private static MenuItem CreateMenuItem(ContextMenuItem item)
    {
        var trigger = new Trigger
        {
            Property = IsEnabledProperty,
            Value = false
        };
        
        trigger.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
        
        var style = new Style(typeof(MenuItem));
        style.Triggers.Add(trigger);

        var menuItem = new MenuItem
        {
            Header = item.Header,
            Command = item.Command,
            CommandParameter = item.CommandParameter,
            Style = style
        };
        
        return menuItem;
    }
    
    private static MenuItem CreateMenuItem(ContextMenuList item)
    {
        var menuItem = new MenuItem
        {
            Header = item.Header
        };

        foreach (var i in item.Items.Select(CreateMenuItem))
        {
            menuItem.Items.Add(i);
        }

        var multiBinding = new MultiBinding
        {
            Converter = new HasVisibleItemsMultiConverter()
        };
        multiBinding.Bindings.Add(new Binding("Items")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        });
        multiBinding.Bindings.Add(new Binding("Items.Count")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        });

        var trigger = new DataTrigger
        {
            Binding = multiBinding,
            Value = false
        };

        trigger.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));

        var style = new Style(typeof(MenuItem));
        style.Triggers.Add(trigger);

        menuItem.Style = style;

        return menuItem;
    }
    
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: IGridItemHost gridItemHost })
        {
            RaiseEvent(new GridItemRoutedEventArgs(OpenContextMenuEvent, gridItemHost));
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
        if (DataContext is not IDynamicGridManagerHost host)
            return;

        List<IGridItemHost> items = host.DynamicGridManager.Items.ToList();

        UpdateGridItems(items);
        ItemsChanged?.Invoke(this, items.Select(i => _viewViewModelMapping.GetView(i)).ToList());
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