using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using UiControls.DropOverlay.View;
using UiControls.DropOverlay.ViewModel;
using UiControlsExamples.DynamicGrid;

namespace UiControlsExamples.DynamicGridDragOverlay;

public partial class DynamicGridDragOverlayShowcase
{
    private Point _startPoint;

    public DynamicGridDragOverlayShowcase()
    {
        InitializeComponent();
        InitializeDragDrop();
        var viewModel = new DynamicGridDragOverlayViewModel();
        DataContext = viewModel;
        DynamicGrid.ConfigureViewBehavior = ConfigureDropOverlayBehavior;
        DynamicGrid.RegisterViewType<ColoredCell, DefaultCellViewModel>();
        DynamicGrid.RegisterViewType<ColoredCell, ColoredCellViewModel>();
        viewModel.DynamicGridManager.ResetItems();
        
        DropOverlay.RegisterViewType<DropOverlayImage, ImageDropZoneViewModel>();
        DropOverlay.RegisterViewType<DropOverlayText, TextDropZoneViewModel>();
    }

    private void ConfigureDropOverlayBehavior(FrameworkElement element)
    {
        System.Diagnostics.Debug.WriteLine($"Configuring behavior for: {element.GetType().Name} - DataContext: {element.DataContext?.GetType().Name}");

        var behavior = new DropOverlayBehavior
        {
            DropOverlayControl = DropOverlay
        };
    
        var shouldShowCommandBinding = new Binding("ShouldShowOverlayCommand")
        {
            Source = DataContext
        };
        BindingOperations.SetBinding(behavior, DropOverlayBehavior.ShouldShowOverlayCommandProperty, shouldShowCommandBinding);
    
        var handleDropCommandBinding = new Binding("HandleDropCommand")
        {
            Source = DataContext
        };
        BindingOperations.SetBinding(behavior, DropOverlayBehavior.HandleDropCommandProperty, handleDropCommandBinding);
            
        BehaviorCollection behaviorCollection = Interaction.GetBehaviors(element);
        behaviorCollection.Add(behavior);
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