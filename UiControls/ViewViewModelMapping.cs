using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace UiControls;

public class ViewViewModelMapping<TViewModelBase> where TViewModelBase : notnull
{
    private readonly Dictionary<Type, Type> _viewRegistry = new();
    private readonly Dictionary<TViewModelBase, FrameworkElement> _viewModelViewMapping = new();

    public void RegisterViewType<TView, TViewModel>() 
        where TView : FrameworkElement 
        where TViewModel : TViewModelBase
    {
        _viewRegistry[typeof(TViewModel)] = typeof(TView);
    }
    
    public FrameworkElement GetViewForViewModel(TViewModelBase viewModel, Action<FrameworkElement>? configureViewBehavior = null)
    {
        if (_viewModelViewMapping.TryGetValue(viewModel, out FrameworkElement? view))
        {
            return view;
        }
        
        if (!_viewRegistry.TryGetValue(viewModel.GetType(), out Type? viewType))
        {
            throw new InvalidOperationException($"No view registered for view type: {nameof(viewModel)}");
        }
        
        view = (FrameworkElement)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;
        configureViewBehavior?.Invoke(view);
        
        _viewModelViewMapping[viewModel] = view;
        return view;
    }

    public List<TViewModelBase> GetViewModels()
    {
        return _viewModelViewMapping.Keys.ToList();
    }
    
    public FrameworkElement GetView(TViewModelBase viewModel)
    {
        return _viewModelViewMapping[viewModel];
    }
    
    public bool TryGetViewForViewModel(TViewModelBase viewModel, [MaybeNullWhen(false)] out FrameworkElement view)
    {
        return _viewModelViewMapping.TryGetValue(viewModel, out view);
    }
    
    public List<FrameworkElement> GetViews() => _viewModelViewMapping.Values.ToList();

    public List<(TViewModelBase ViewModel, FrameworkElement View)> GetViewAndViewModels() =>
        _viewModelViewMapping.Select(i => (i.Key, i.Value)).ToList();
    
    public void RemoveNotExistingViewModels(IList<TViewModelBase> items)
    {
        List<TViewModelBase> keysToRemove = _viewModelViewMapping.Keys
            .Where(key => !items.Contains(key))
            .ToList();

        foreach (TViewModelBase key in keysToRemove)
        {
            _viewModelViewMapping.Remove(key);
        }
    }
}