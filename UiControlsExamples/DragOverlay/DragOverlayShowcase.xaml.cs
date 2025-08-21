using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UiControls.DropOverlay.View;
using UiControls.DropOverlay.ViewModel;

namespace UiControlsExamples.DragOverlay;

public partial class DragOverlayShowcase
{
    private Point _startPoint;

    public DragOverlayShowcase()
    {
        InitializeComponent();
        InitializeDragDrop();
        
        DataContext = new DragOverlayViewModel();
        
        DropOverlay.RegisterViewType<DropOverlayImage, ImageDropZoneViewModel>();
        DropOverlay.RegisterViewType<DropOverlayText, TextDropZoneViewModel>();
    }

    private void InitializeDragDrop()
    {
        SourceTreeView.PreviewMouseLeftButtonDown += (_, e) => _startPoint = e.GetPosition(null);
        SourceTreeView.PreviewMouseMove += OnSourceTreeViewOnPreviewMouseMove;

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