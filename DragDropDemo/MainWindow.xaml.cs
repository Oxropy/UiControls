using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UiControls.DropOverlay;

namespace DragDropDemo;

public partial class MainWindow
{
    private readonly DropOverlayControl _dropOverlay = new();
    private Point _startPoint;
    private UIElement? _currentTarget;

    public MainWindow()
    {
        InitializeComponent();
        InitializeOverlayCanvas();
        InitializeDragDrop();
    }

    private void InitializeOverlayCanvas()
    {
        // Insert the canvas right after the TargetGrid in the visual tree
        if (TargetGrid.Parent is Grid parentGrid)
        {
            parentGrid.Children.Add(_dropOverlay);
            Grid.SetColumn(_dropOverlay, 1); // Same column as TargetGrid

            // Bind the canvas size to the TargetGrid size
            _dropOverlay.SetBinding(WidthProperty, new Binding(nameof(ActualWidth)) { Source = TargetGrid });
            _dropOverlay.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = TargetGrid });
        }
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
            _dropOverlay.Show(target);
        }
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
        if (!IsControlPressed(e))
        {
            _dropOverlay.Hide();
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            return;
        }

        // Get the mouse position relative to the current target
        if (_currentTarget is FrameworkElement target)
        {
            _dropOverlay.Show(target);

            Point position = e.GetPosition(target);
            DropOverlayPosition dropOverlayPosition = _dropOverlay.GetDropPosition(position, target);
            _dropOverlay.UpdateHighlight(dropOverlayPosition);

            e.Effects = dropOverlayPosition != DropOverlayPosition.Unknown
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Border_DragLeave(object sender, DragEventArgs e)
    {
        _dropOverlay.Hide();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.GetData(DataFormats.Text) is string draggedText)
        {
            string dropText = draggedText;
            if (IsControlPressed(e))
            {
                var position = e.GetPosition(border);
                var dropPosition = _dropOverlay.GetDropPosition(position, border);
                if (dropPosition == DropOverlayPosition.Unknown)
                {
                    _dropOverlay.Hide();
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
        _dropOverlay.Hide();
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