using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UiControls.KeyboardOverlay.View;

public class DragOverlayControl : Canvas
{
    public static readonly DependencyProperty IsOverlayVisibleProperty = DependencyProperty.Register(
        nameof(IsOverlayVisible), typeof(bool), typeof(DragOverlayControl),
        new PropertyMetadata(false, OnOverlayVisibilityChanged));

    public static readonly DependencyProperty OverlayContentProperty = DependencyProperty.Register(
        nameof(OverlayContent), typeof(UIElement), typeof(DragOverlayControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty TriggerKeyProperty = DependencyProperty.Register(
        nameof(TriggerKey), typeof(Key), typeof(DragOverlayControl),
        new PropertyMetadata(Key.None));

    public static readonly DependencyProperty RequiredModifiersProperty = DependencyProperty.Register(
        nameof(RequiredModifiers), typeof(ModifierKeys), typeof(DragOverlayControl),
        new PropertyMetadata(ModifierKeys.None));

    private readonly Dictionary<UIElement, bool> _originalHitTestValues = new();
    private bool _isKeyPressed;
    private bool _isDragInProgress;

    public bool IsOverlayVisible
    {
        get => (bool)GetValue(IsOverlayVisibleProperty);
        set => SetValue(IsOverlayVisibleProperty, value);
    }

    public UIElement? OverlayContent
    {
        get => (UIElement?)GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }

    public Key TriggerKey
    {
        get => (Key)GetValue(TriggerKeyProperty);
        set => SetValue(TriggerKeyProperty, value);
    }

    public ModifierKeys RequiredModifiers
    {
        get => (ModifierKeys)GetValue(RequiredModifiersProperty);
        set => SetValue(RequiredModifiersProperty, value);
    }

    public DragOverlayControl()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Subscribe to global key events
        InputManager.Current.PreProcessInput += OnPreProcessInput;

        // Monitor for drag operations
        SubscribeToDragEvents();
    }

    private void SubscribeToDragEvents()
    {
        // Find the parent window to monitor drag events at the application level
        var window = FindParentWindow();
        if (window != null)
        {
            window.DragEnter += OnDragEnter;
            window.DragLeave += OnDragLeave;
            window.Drop += OnDragDrop;
            window.DragOver += OnDragOver;
        }
    }

    private void UnsubscribeFromDragEvents()
    {
        var window = FindParentWindow();
        if (window != null)
        {
            window.DragEnter -= OnDragEnter;
            window.DragLeave -= OnDragLeave;
            window.Drop -= OnDragDrop;
            window.DragOver -= OnDragOver;
        }
    }

    private Window? FindParentWindow()
    {
        DependencyObject? current = this;
        while (current != null)
        {
            current = VisualTreeHelper.GetParent(current);
            if (current is Window window)
            {
                return window;
            }
        }

        return null;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (!_isDragInProgress)
        {
            _isDragInProgress = true;
            CheckAndShowOverlay();
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (_isDragInProgress)
        {
            CheckAndShowOverlay();
        }
    }

    private void HideOverlay()
    {
        // Clear children
        Children.Clear();

        // Reset positioning properties on the overlay content when hiding
        if (OverlayContent != null)
        {
            SetLeft(OverlayContent, 0);
            SetTop(OverlayContent, 0);
        }

        // Always restore background hit testing when hiding
        RestoreBackgroundHitTesting();

        // Hide overlay
        Visibility = Visibility.Collapsed;
        IsHitTestVisible = false;

        IsOverlayVisible = false;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe from global key events
        InputManager.Current.PreProcessInput -= OnPreProcessInput;

        // Unsubscribe from drag events
        UnsubscribeFromDragEvents();

        // Ensure hit testing is restored when unloading
        RestoreBackgroundHitTesting();
    }

    private void CheckAndShowOverlay()
    {
        // Only show overlay if BOTH drag is in progress AND keyboard conditions are met
        bool shouldShow = _isDragInProgress;

        // If a trigger key is specified, it's REQUIRED during drag
        if (TriggerKey != Key.None)
        {
            bool keyCurrentlyDown = Keyboard.IsKeyDown(TriggerKey);
            bool modifiersMatch = Keyboard.Modifiers == RequiredModifiers;
            shouldShow = shouldShow && keyCurrentlyDown && modifiersMatch;
        }

        if (shouldShow && !IsOverlayVisible)
        {
            ShowOverlay();
        }
        else if (!shouldShow && IsOverlayVisible)
        {
            HideOverlay();
        }
    }

    private void CheckAndHideOverlay()
    {
        // Always hide overlay when drag is not in progress
        if (!_isDragInProgress && IsOverlayVisible)
        {
            HideOverlay();
        }
        // Or hide if drag is in progress but key conditions are not met
        else if (_isDragInProgress && TriggerKey != Key.None)
        {
            bool keyCurrentlyDown = Keyboard.IsKeyDown(TriggerKey);
            bool modifiersMatch = Keyboard.Modifiers == RequiredModifiers;
            bool shouldShow = keyCurrentlyDown && modifiersMatch;

            if (!shouldShow && IsOverlayVisible)
            {
                HideOverlay();
            }
        }
    }

    private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
    {
        if (TriggerKey == Key.None)
            return;

        if (e.StagingItem.Input is not KeyEventArgs keyArgs)
            return;

        bool correctKey = keyArgs.Key == TriggerKey;
        bool correctModifiers = Keyboard.Modifiers == RequiredModifiers;

        // Handle trigger key events
        if (correctKey)
        {
            if (keyArgs is { IsDown: true, IsRepeat: false } && correctModifiers && !_isKeyPressed)
            {
                _isKeyPressed = true;

                // Only show overlay if drag is currently in progress
                if (_isDragInProgress)
                {
                    ShowOverlay();
                }
            }
            else if (keyArgs.IsUp && _isKeyPressed)
            {
                _isKeyPressed = false;

                // Always hide overlay when key is released (regardless of drag state)
                HideOverlay();
            }
        }

        // Handle modifier key changes while trigger key is pressed
        if (_isKeyPressed && !correctModifiers)
        {
            bool isTriggerKeyStillDown = Keyboard.IsKeyDown(TriggerKey);
            if (isTriggerKeyStillDown && !correctModifiers)
            {
                _isKeyPressed = false;
                HideOverlay();
            }
        }

        // Handle any other key changes during drag to update overlay state
        if (_isDragInProgress)
        {
            CheckAndShowOverlay();
        }
    }

    private void OnDragDrop(object sender, DragEventArgs e)
    {
        if (_isDragInProgress)
        {
            _isDragInProgress = false;

            // Always hide overlay immediately when drag ends, regardless of key state
            if (IsOverlayVisible)
            {
                HideOverlay();
            }

            // Reset key state when drag ends
            _isKeyPressed = false;

            // Ensure hit testing is restored after drop
            RestoreBackgroundHitTesting();
        }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (!_isDragInProgress)
            return;

        // Only hide if we're actually leaving the window bounds
        if (sender is not Window window)
            return;

        var position = e.GetPosition(window);
        var windowBounds = new Rect(0, 0, window.ActualWidth, window.ActualHeight);

        if (!windowBounds.Contains(position))
        {
            _isDragInProgress = false;

            // Always hide overlay when drag leaves window
            if (IsOverlayVisible)
            {
                HideOverlay();
            }

            // Reset key state when drag leaves
            _isKeyPressed = false;

            // Ensure hit testing is restored when drag leaves
            RestoreBackgroundHitTesting();
        }
    }

    private static void OnOverlayVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragOverlayControl control)
        {
            if ((bool)e.NewValue)
            {
                control.ShowOverlay();
            }
            else
            {
                control.HideOverlay();
            }
        }
    }

    private void ShowOverlay()
    {
        if (OverlayContent == null)
            return;

        // Clear existing children
        Children.Clear();

        // Reset positioning properties on the overlay content before adding it
        SetLeft(OverlayContent, 0);
        SetTop(OverlayContent, 0);

        // Add the overlay content
        Children.Add(OverlayContent);

        // Make overlay visible and interactive
        Visibility = Visibility.Visible;
        IsHitTestVisible = true;

        // Force layout update to ensure proper sizing
        UpdateLayout();

        // Disable hit testing for background elements
        DisableBackgroundHitTesting();

        // Center the overlay content
        CenterOverlayContent();

        // Set focus to the overlay
        Focus();
    }

    private void CenterOverlayContent()
    {
        if (OverlayContent == null || ActualWidth == 0 || ActualHeight == 0)
            return;

        // Force measure if needed
        if (OverlayContent.DesiredSize.IsEmpty ||
            OverlayContent.DesiredSize is { Width: 0, Height: 0 })
        {
            OverlayContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        // Use the larger of DesiredSize or RenderSize for more accurate positioning
        Size contentSize = OverlayContent.DesiredSize;
        if (OverlayContent.RenderSize is { Width: > 0, Height: > 0 })
        {
            contentSize = OverlayContent.RenderSize;
        }

        // Center the content on the canvas
        double left = (ActualWidth - contentSize.Width) / 2;
        double top = (ActualHeight - contentSize.Height) / 2;

        SetLeft(OverlayContent, Math.Max(0, left));
        SetTop(OverlayContent, Math.Max(0, top));
    }

    private void DisableBackgroundHitTesting()
    {
        _originalHitTestValues.Clear();

        // Find the parent window or main container
        FrameworkElement? parent = FindParentContainer();
        if (parent != null)
        {
            // During drag operations, be much more conservative about disabling hit testing
            if (_isDragInProgress)
            {
                DisableHitTestingConservativelyForDrag(parent, this);
            }
            else
            {
                DisableHitTestingRecursively(parent, this);
            }
        }
    }

    private void DisableHitTestingConservativelyForDrag(DependencyObject element, DependencyObject skipElement)
    {
        if (element == skipElement)
            return;

        if (element is UIElement uiElement)
        {
            // During drag operations, only disable hit testing for very specific non-essential elements
            // Be very conservative - only disable elements we're sure don't affect drag/drop
            bool shouldDisable = ShouldDisableElementForDrag(uiElement);

            if (shouldDisable)
            {
                _originalHitTestValues[uiElement] = uiElement.IsHitTestVisible;
                uiElement.IsHitTestVisible = false;
            }
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            DisableHitTestingConservativelyForDrag(child, skipElement);
        }
    }

    private bool ShouldDisableElementForDrag(UIElement element)
    {
        // Keep ALL potentially drag-related elements enabled
        if (element.AllowDrop) return false;

        // Keep all interactive controls enabled
        if (element is Control) return false;

        // Keep containers that might contain draggable items enabled
        if (element is Panel || element is ContentPresenter || element is Border) return false;

        // Keep window enabled
        if (element is Window) return false;

        // Only disable purely decorative elements like TextBlock, Image, etc.
        if (element is TextBlock ||
            element is Image ||
            element is System.Windows.Shapes.Shape)
        {
            return true;
        }

        return false; // When in doubt, keep it enabled
    }

    private void DisableHitTestingRecursively(DependencyObject element, DependencyObject skipElement)
    {
        if (element == skipElement)
            return;

        if (element is UIElement uiElement)
        {
            _originalHitTestValues[uiElement] = uiElement.IsHitTestVisible;
            uiElement.IsHitTestVisible = false;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            DisableHitTestingRecursively(child, skipElement);
        }
    }

    private void RestoreBackgroundHitTesting()
    {
        foreach (var kvp in _originalHitTestValues)
        {
            // Make sure the element still exists and is valid before restoring
            kvp.Key.IsHitTestVisible = kvp.Value;
        }

        _originalHitTestValues.Clear();
    }


    private FrameworkElement? FindParentContainer()
    {
        DependencyObject? current = this;
        while (current != null)
        {
            current = VisualTreeHelper.GetParent(current);
            if (current is Window || current is UserControl || current is Grid)
            {
                return current as FrameworkElement;
            }
        }

        return null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsOverlayVisible && OverlayContent != null)
        {
            CenterOverlayContent();
        }
    }
}