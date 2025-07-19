using UiControls.DynamicGrid.ViewModel.ContextMenu;

namespace UiControls.DynamicGrid.ViewModel;

public class ContextMenuBuilder
{
    private readonly DynamicGridManager _dynamicGridManager;

    public ContextMenuBuilder(DynamicGridManager dynamicGridManager)
    {
        _dynamicGridManager = dynamicGridManager;
    }

    public ContextMenuStart<IGridItemHost> Build(IGridItemHost gridItemHost)
    {
        return new ContextMenuStart<IGridItemHost>
        {
            ViewModel = gridItemHost,
            Items = CreateItems(gridItemHost)
        };
    }

    private List<IContextMenu> CreateItems(IGridItemHost gridItemHost)
    {
        return
        [
            AddSelectMenuItem(gridItemHost),
            AddRowMenuItems(gridItemHost),
            AddColumnMenuItems(gridItemHost),
            AddMergeMenuItem(gridItemHost),
            AddSplitMenuItem(gridItemHost)
        ];
    }

    private ContextMenuItem AddSelectMenuItem(IGridItemHost gridItemHost)
    {
        return new ContextMenuItem
        {
            Header = gridItemHost.GridItem.IsSelected ? "Unselect" : "Select",
            Command = _dynamicGridManager.SelectCommand,
            CommandParameter = gridItemHost.GridItem
        };
    }

    private IContextMenu AddRowMenuItems(IGridItemHost gridItemHost)
    {
        if (_dynamicGridManager.RowDefinitionsCount == _dynamicGridManager.MinRows
            && _dynamicGridManager.MinRows == _dynamicGridManager.MaxRows)
        {
            return new ContextMenuEmpty();
        }

        List<IContextMenu> items = [];
        var addRowAboveItem = new ContextMenuItem
        {
            Header = "Add above",
            Command = _dynamicGridManager.AddRowAboveCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(addRowAboveItem);
        
        var addRowBelowItem = new ContextMenuItem
        {
            Header = "Add below",
            Command = _dynamicGridManager.AddRowBelowCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(addRowBelowItem);

        var removeRowItem = new ContextMenuItem
        {
            Header = "Remove",
            Command = _dynamicGridManager.RemoveRowCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(removeRowItem);

        return new ContextMenuList
        {
            Header = "Row",
            Items = items
        };
    }

    private IContextMenu AddColumnMenuItems(IGridItemHost gridItemHost)
    {
        if (_dynamicGridManager.ColumnDefinitionsCount == _dynamicGridManager.MinColumns
            && _dynamicGridManager.MinColumns == _dynamicGridManager.MaxColumns)
        {
            return new ContextMenuEmpty();
        }

        List<IContextMenu> items = [];

        var addColumnAboveItem = new ContextMenuItem
        {
            Header = "Add to left",
            Command = _dynamicGridManager.AddColumnToLeftCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(addColumnAboveItem);
        
        var addColumnBelowItem = new ContextMenuItem
        {
            Header = "Add to right",
            Command = _dynamicGridManager.AddColumnToRightCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(addColumnBelowItem);

        var removeColumnItem = new ContextMenuItem
        {
            Header = "Remove",
            Command = _dynamicGridManager.RemoveColumnCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(removeColumnItem);

        return new ContextMenuList
        {
            Header = "Column",
            Items = items
        };
    }

    private ContextMenuItem AddMergeMenuItem(IGridItemHost gridItemHost)
    {
        return new ContextMenuItem
        {
            Header = "Merge selected Cells",
            Command = _dynamicGridManager.MergeCellsCommand,
            CommandParameter = gridItemHost.GridItem
        };
    }

    private IContextMenu AddSplitMenuItem(IGridItemHost gridItemHost)
    {
        List<IContextMenu> items = [];
        var splitMerge = new ContextMenuItem
        {
            Header = "Split Merge",
            Command = _dynamicGridManager.SplitMergeCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(splitMerge);

        var splitHorizontalItem = new ContextMenuItem
        {
            Header = "Split Horizontal",
            Command = _dynamicGridManager.SplitCellHorizontalCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(splitHorizontalItem);

        var splitVerticalItem = new ContextMenuItem
        {
            Header = "Split Vertical",
            Command = _dynamicGridManager.SplitCellVerticalCommand,
            CommandParameter = gridItemHost.GridItem
        };

        items.Add(splitVerticalItem);

        return new ContextMenuList
        {
            Header = "Split Cell",
            Items = items
        };
    }
}