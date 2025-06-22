using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UiControls.DropOverlay;

namespace DragDropDemo;

public partial class MainWindow
{
    private Point _startPoint;
    private UIElement? _currentTarget;

    public MainWindow()
    {
        InitializeComponent();
        InitializeDragDrop();
    }

    private void InitializeDragDrop()
    {
        // Setup TreeView drag events
        SourceTreeView.PreviewMouseLeftButtonDown += (_, e) => _startPoint = e.GetPosition(null);

        SourceTreeView.PreviewMouseMove += OnSourceTreeViewOnPreviewMouseMove;

        // Setup Grid drop events
        foreach (UIElement element in TargetGrid.Children)
        {
            if (element is Border border)
            {
                border.AllowDrop = true;
                border.DragEnter += Border_DragEnter;
                border.DragOver += Border_DragOver;
                border.DragLeave += Border_DragLeave;
                border.Drop += Border_Drop;
            }
        }

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

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        _currentTarget = sender as UIElement;
        if (_currentTarget is FrameworkElement target && !IsControlPressed(e))
        {
            DropOverlay.Show(target);
        }
    }

    private void Border_DragOver(object sender, DragEventArgs e)
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

    private void Border_DragLeave(object sender, DragEventArgs e)
    {
        DropOverlay.Hide();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.GetData(DataFormats.Text) is string draggedText)
        {
            string dropText = draggedText;
            if (IsControlPressed(e))
            {
                var position = e.GetPosition(border);
                var dropPosition = DropOverlay.GetDropPosition(position, border);
                if (dropPosition == DropOverlayPosition.Unknown)
                {
                    DropOverlay.Hide();
                    return;
                }

                dropText = $"{draggedText} ({dropPosition})";
            }

            // Update the target element with the dropped item
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = dropText;
            }
        }

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