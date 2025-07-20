using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UiControls.DropOverlay;
using UiControls.DynamicGrid.ViewModel;
using UiControlsExamples.DynamicGrid;

namespace UiControlsExamples.DynamicGridDragOverlay;

public partial class DynamicGridDragOverlayShowcase
{
    private Point _startPoint;
    private UIElement? _currentTarget;
    private readonly DynamicGridDragOverlayViewModel _viewModel;

    public DynamicGridDragOverlayShowcase()
    {
        InitializeComponent();
        InitializeDragDrop();
        _viewModel = new DynamicGridDragOverlayViewModel();
        DataContext = _viewModel;
        DynamicGrid.RegisterViewType<ColoredCell, DefaultCellViewModel>();
        DynamicGrid.RegisterViewType<ColoredCell, ColoredCellViewModel>();
        DynamicGrid.ItemsChanged += DynamicGridOnItemsChanged;
        _viewModel.DynamicGridManager.ResetItems();
    }

    private void DynamicGridOnItemsChanged(object? sender, IEnumerable<UIElement> e)
    {
        foreach (UIElement element in e)
        {
            element.AllowDrop = true;
            element.DragEnter -= Cell_DragEnter;
            element.DragEnter += Cell_DragEnter;
            element.DragOver -= Cell_DragOver;
            element.DragOver += Cell_DragOver;
            element.DragLeave -= Cell_DragLeave;
            element.DragLeave += Cell_DragLeave;
            element.Drop -= Cell_Drop;
            element.Drop += Cell_Drop;
        }
    }

    private void InitializeDragDrop()
    {
        // Setup TreeView drag events
        SourceTreeView.PreviewMouseLeftButtonDown += (_, e) => _startPoint = e.GetPosition(null);

        SourceTreeView.PreviewMouseMove += OnSourceTreeViewOnPreviewMouseMove;

        return;

        void OnSourceTreeViewOnPreviewMouseMove(object _, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - _startPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem is not null)
            {
                DragDrop.DoDragDrop(treeViewItem, treeViewItem.Header, DragDropEffects.Move);
            }
        }
    }

    private static bool IsControlPressed(DragEventArgs e)
    {
        return (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey;
    }

    private void Cell_DragEnter(object sender, DragEventArgs e)
    {
        _currentTarget = sender as UIElement;
        if (_currentTarget is FrameworkElement target && !IsControlPressed(e))
        {
            DropOverlay.Show(target);
        }
    }

    private void Cell_DragOver(object sender, DragEventArgs e)
    {
        if (!IsControlPressed(e))
        {
            DropOverlay.Hide();
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            return;
        }

        // Get the mouse position relative to the current target
        if (_currentTarget is FrameworkElement target)
        {
            DropOverlay.Show(target);

            Point position = e.GetPosition(target);
            DropOverlayPosition dropOverlayPosition = DropOverlay.GetDropPosition(position, target);
            DropOverlay.UpdateHighlight(dropOverlayPosition);

            e.Effects = dropOverlayPosition != DropOverlayPosition.Unknown
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Cell_DragLeave(object sender, DragEventArgs e)
    {
        DropOverlay.Hide();
    }

    private void Cell_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ColoredCell { DataContext: IGridItemHost gridItemHost } coloredCell
            || e.Data.GetData(DataFormats.Text) is not string draggedText)
        {
            _currentTarget = null;
            DropOverlay.Hide();
            return;
        }

        DropOverlayPosition dropPosition = DropOverlayPosition.Center;
        if (IsControlPressed(e))
        {
            var position = e.GetPosition(coloredCell);
            dropPosition = DropOverlay.GetDropPosition(position, coloredCell);
        }

        GridItem? gridItem;
        switch (dropPosition)
        {
            case DropOverlayPosition.Unknown:
                DropOverlay.Hide();
                return;
            case DropOverlayPosition.Top:
                _viewModel.DynamicGridManager.AddRowAboveCommand.Execute(gridItemHost.GridItem);
                gridItem = gridItemHost.GridItem with
                {
                    Row = gridItemHost.GridItem.Row - 1,
                    RowSpan = 1,
                    ColumnSpan = 1
                };
                break;
            case DropOverlayPosition.Right:
                _viewModel.DynamicGridManager.AddColumnToRightCommand.Execute(gridItemHost.GridItem);
                gridItem = gridItemHost.GridItem with
                {
                    Column = gridItemHost.GridItem.Column + 1,
                    RowSpan = 1,
                    ColumnSpan = 1
                };
                break;
            case DropOverlayPosition.Bottom:
                _viewModel.DynamicGridManager.AddRowBelowCommand.Execute(gridItemHost.GridItem);
                gridItem = gridItemHost.GridItem with
                {
                    Row = gridItemHost.GridItem.Row + 1,
                    RowSpan = 1,
                    ColumnSpan = 1
                };
                break;
            case DropOverlayPosition.Left:
                _viewModel.DynamicGridManager.AddColumnToLeftCommand.Execute(gridItemHost.GridItem);
                gridItem = gridItemHost.GridItem with
                {
                    Column = gridItemHost.GridItem.Column - 1,
                    RowSpan = 1,
                    ColumnSpan = 1
                };
                break;
            case DropOverlayPosition.Center:
                gridItem = gridItemHost.GridItem with { };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dropPosition));
        }

        _viewModel.DynamicGridManager.AddItem(new ColoredCellViewModel
        {
            Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(draggedText)), GridItem = gridItem
        });

        _currentTarget = null;
        DropOverlay.Hide();
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        do
        {
            switch (current)
            {
                case null:
                    return null;
                case T t:
                    return t;
                default:
                    current = VisualTreeHelper.GetParent(current);
                    break;
            }
        } while (current != null);

        return null;
    }
}