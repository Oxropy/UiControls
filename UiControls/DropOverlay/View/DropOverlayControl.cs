using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using UiControls.DropOverlay.ViewModel;

namespace UiControls.DropOverlay.View;

public class DropOverlayControl : Canvas
{
    public static readonly DependencyProperty TemplatesProperty = DependencyProperty.Register(
        nameof(Templates), typeof(ObservableCollection<DropZoneViewModel>), typeof(DropOverlayControl),
        new PropertyMetadata(new ObservableCollection<DropZoneViewModel>()));

    private readonly ViewViewModelMapping<DropZoneViewModel> _viewViewModelMapping = new();
    private FrameworkElement? _lastTarget;
    
    public ObservableCollection<DropZoneViewModel> Templates
    {
        get => (ObservableCollection<DropZoneViewModel>)GetValue(TemplatesProperty);
        set => SetValue(TemplatesProperty, value);
    }

    public void RegisterViewType<TView, TViewModel>()
        where TView : FrameworkElement
        where TViewModel : DropZoneViewModel
    {
        _viewViewModelMapping.RegisterViewType<TView, TViewModel>();
    }

    public DropZoneViewModel? GetDropPosition(Point position, FrameworkElement target)
    {
        Point canvasPosition = target.TranslatePoint(position, this);

        foreach (DropZoneViewModel viewModel in _viewViewModelMapping.GetViewModels())
        {
            var viewBounds = new Rect(viewModel.Left, viewModel.Top, viewModel.Width, viewModel.Height);
            if (viewBounds.Contains(canvasPosition))
            {
                return viewModel;
            }
        }

        return null;
    }

    private void UpdatePosition(FrameworkElement target)
    {
        // Get the position of the target relative to the overlay canvas
        Point targetPos = target.TranslatePoint(new Point(0, 0), this);

        foreach ((DropZoneViewModel viewModel, FrameworkElement _) in _viewViewModelMapping.GetViewAndViewModels())
        {
            viewModel.UpdatePosition(targetPos, target.ActualWidth, target.ActualHeight);
        }
    }

    public void Show(FrameworkElement target)
    {
        foreach (DropZoneViewModel template in Templates)
        {
            FrameworkElement view = _viewViewModelMapping.GetViewForViewModel(template);
            view.SetBinding(LeftProperty, new Binding(nameof(DropZoneViewModel.Left)));
            view.SetBinding(TopProperty, new Binding(nameof(DropZoneViewModel.Top)));
            view.IsHitTestVisible = false;

            DisableHitTestingRecursively(view);
            
            Children.Add(view);
        }

        if (_lastTarget != target)
        {
            _lastTarget = target;
            UpdatePosition(target);
        }

        AllowDrop = true;
        IsHitTestVisible = true;
    }

    public void Hide()
    {
        _lastTarget = null;

        foreach (FrameworkElement view in _viewViewModelMapping.GetViews())
        {
            Children.Remove(view);
        }
        
        AllowDrop = false;
        IsHitTestVisible = false;
    }
    
    private static void DisableHitTestingRecursively(DependencyObject element)
    {
        if (element is UIElement uiElement)
        {
            uiElement.IsHitTestVisible = false;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (var i = 0; i < childrenCount; i++)
        {
            DisableHitTestingRecursively(VisualTreeHelper.GetChild(element, i));
        }
    }
}