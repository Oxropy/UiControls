using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using UiControls.DropOverlay.ViewModel;

namespace UiControls.DropOverlay.View;

public class DropOverlayBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty DropOverlayControlProperty = DependencyProperty.Register(
        nameof(DropOverlayControl), typeof(DropOverlayControl), typeof(DropOverlayBehavior),
        new PropertyMetadata(default(DropOverlayControl?)));

    public static readonly DependencyProperty ShouldShowOverlayCommandProperty = DependencyProperty.Register(
        nameof(ShouldShowOverlayCommand), typeof(ICommand), typeof(DropOverlayBehavior),
        new PropertyMetadata(default(ICommand?)));

    public static readonly DependencyProperty HandleDropCommandProperty = DependencyProperty.Register(
        nameof(HandleDropCommand), typeof(ICommand), typeof(DropOverlayBehavior),
        new PropertyMetadata(default(ICommand?)));

    private bool _isOverlayVisible;
    private bool _isDraggingOver;
    private readonly Dictionary<UIElement, bool> _originalHitTestValues = new();

    public DropOverlayControl? DropOverlayControl
    {
        get => (DropOverlayControl?)GetValue(DropOverlayControlProperty);
        set => SetValue(DropOverlayControlProperty, value);
    }

    public ICommand? ShouldShowOverlayCommand
    {
        get => (ICommand?)GetValue(ShouldShowOverlayCommandProperty);
        set => SetValue(ShouldShowOverlayCommandProperty, value);
    }

    public ICommand? HandleDropCommand
    {
        get => (ICommand?)GetValue(HandleDropCommandProperty);
        set => SetValue(HandleDropCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += Element_DragEnter;
            AssociatedObject.DragOver += Element_DragOver;
            AssociatedObject.DragLeave += Element_DragLeave;
            AssociatedObject.Drop += Element_Drop;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.DragEnter -= Element_DragEnter;
            AssociatedObject.DragOver -= Element_DragOver;
            AssociatedObject.DragLeave -= Element_DragLeave;
            AssociatedObject.Drop -= Element_Drop;
        }

        // Ensure we stop monitoring keyboard if still subscribed
        InputManager.Current.PreProcessInput -= InputManagerOnPreProcessInput;

        base.OnDetaching();
    }

    private void Element_DragEnter(object sender, DragEventArgs e)
    {
        // Start monitoring keyboard to update overlay without mouse movement
        _isDraggingOver = true;
        InputManager.Current.PreProcessInput += InputManagerOnPreProcessInput;
        int row = Grid.GetRow(AssociatedObject);
        int column = Grid.GetColumn(AssociatedObject);
        System.Diagnostics.Debug.WriteLine($"DragEnter - Row: {row}, Column: {column}");
    }

    private void Element_DragOver(object sender, DragEventArgs e)
    {
        bool shouldShow = ShouldShowOverlay(AssociatedObject, e);
        
        int row = Grid.GetRow(AssociatedObject);
        int column = Grid.GetColumn(AssociatedObject);

        System.Diagnostics.Debug.WriteLine($"DragOver - Row: {row}, Column: {column}, ShouldShow: {shouldShow}, _isOverlayVisible: {_isOverlayVisible}, KeyStates: {e.KeyStates}");
        
        if (shouldShow && !_isOverlayVisible)
        {
            DropOverlayControl?.Show(AssociatedObject);
            DisableChildHitTesting(AssociatedObject);
            _isOverlayVisible = true;
            System.Diagnostics.Debug.WriteLine("OVERLAY SHOWN");
        }
        else if (!shouldShow && _isOverlayVisible)
        {
            DropOverlayControl?.Hide();
            RestoreChildHitTesting();
            _isOverlayVisible = false;
            System.Diagnostics.Debug.WriteLine("OVERLAY HIDDEN");
        }

        if (_isOverlayVisible)
        {
            Point position = e.GetPosition(AssociatedObject);
            DropZoneViewModel? dropZoneViewModel = DropOverlayControl?.GetDropPosition(position, AssociatedObject);
            e.Effects = dropZoneViewModel is null ? DragDropEffects.None : DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.Move;
        }
        
        System.Diagnostics.Debug.WriteLine($"Effect: {e.Effects}");

        e.Handled = true;
    }

    private void Element_DragLeave(object sender, DragEventArgs e)
    {
        if (!Equals(sender, AssociatedObject))
            return;
        
        int row = Grid.GetRow(AssociatedObject);
        int column = Grid.GetColumn(AssociatedObject);
        System.Diagnostics.Debug.WriteLine($"DragLeave - Row: {row}, Column: {column}");
        
        // Stop monitoring keyboard when drag leaves
        _isDraggingOver = false;
        InputManager.Current.PreProcessInput -= InputManagerOnPreProcessInput;
        
        DropOverlayControl?.Hide();
        RestoreChildHitTesting();
        _isOverlayVisible = false;
    }

    private void Element_Drop(object sender, DragEventArgs e)
    {
        DropEventArgs dropEventArgs = GetDropEventArgs(sender, e);
        HandleDropCommand?.Execute(dropEventArgs);
        DropOverlayControl?.Hide();
        
        // Stop monitoring keyboard when drop completes
        _isDraggingOver = false;
        InputManager.Current.PreProcessInput -= InputManagerOnPreProcessInput;
        
        RestoreChildHitTesting();
        _isOverlayVisible = false;
    }

    private bool ShouldShowOverlay(object sender, DragEventArgs e)
    {
        DropEventArgs dropEventArgs = GetDropEventArgs(sender, e, addDropZone: false);
        return ShouldShowOverlayCommand?.CanExecute(dropEventArgs) == true;
    }

    private DropEventArgs GetDropEventArgs(object sender, DragEventArgs e, bool addDropZone = true)
    {
        return new DropEventArgs
        {
            Sender = sender,
            DragEventArgs = e,
            DropZone = addDropZone ? GetCurrentTargetDropZoneViewModel(e) : null
        };
    }

    private DropZoneViewModel? GetCurrentTargetDropZoneViewModel(DragEventArgs e)
    {
        if (!_isOverlayVisible)
        {
            if (DropOverlayControl?.DataContext is IDropOverlayHost host)
            {
                return host.DropOverlayViewModel.DefaultDropZone;
            }

            return null;
        }

        Point position = e.GetPosition(AssociatedObject);
        return DropOverlayControl?.GetDropPosition(position, AssociatedObject);
    }

    private bool ShouldShowOverlayKeyboardOnly()
    {
        // Evaluate using current keyboard state; pass a DropEventArgs with null DragEventArgs
        var dropEventArgs = new DropEventArgs
        {
            Sender = AssociatedObject,
            DragEventArgs = null,
            DropZone = null
        };
        return ShouldShowOverlayCommand?.CanExecute(dropEventArgs) == true;
    }
    
    private void DisableChildHitTesting(FrameworkElement parent)
    {
        _originalHitTestValues.Clear();
        DisableChildHitTestingRecursive(parent);
    }

    private void DisableChildHitTestingRecursive(DependencyObject element)
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (var i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);
            if (child is UIElement uiChild)
            {
                _originalHitTestValues[uiChild] = uiChild.IsHitTestVisible;
                uiChild.IsHitTestVisible = false;
            }
        
            DisableChildHitTestingRecursive(child);
        }
    }

    private void RestoreChildHitTesting()
    {
        foreach (KeyValuePair<UIElement, bool> kvp in _originalHitTestValues)
        {
            kvp.Key.IsHitTestVisible = kvp.Value;
        }

        _originalHitTestValues.Clear();
    }

    private void InputManagerOnPreProcessInput(object? sender, PreProcessInputEventArgs e)
    {
        if (!_isDraggingOver)
            return;

        if (e?.StagingItem?.Input is KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.IsRepeat)
                return;

            bool shouldShow = ShouldShowOverlayKeyboardOnly();
            if (shouldShow && !_isOverlayVisible)
            {
                DropOverlayControl?.Show(AssociatedObject);
                DisableChildHitTesting(AssociatedObject);
                _isOverlayVisible = true;
            }
            else if (!shouldShow && _isOverlayVisible)
            {
                DropOverlayControl?.Hide();
                RestoreChildHitTesting();
                _isOverlayVisible = false;
            }
        }
    }
}