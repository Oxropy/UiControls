using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UiControls.KeyboardOverlay.View
{
    public class KeyboardOverlayControl : Canvas
    {
        public static readonly DependencyProperty IsOverlayVisibleProperty = DependencyProperty.Register(
            nameof(IsOverlayVisible), typeof(bool), typeof(KeyboardOverlayControl),
            new PropertyMetadata(false, OnOverlayVisibilityChanged));

        public static readonly DependencyProperty OverlayContentProperty = DependencyProperty.Register(
            nameof(OverlayContent), typeof(UIElement), typeof(KeyboardOverlayControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TriggerKeyProperty = DependencyProperty.Register(
            nameof(TriggerKey), typeof(Key), typeof(KeyboardOverlayControl),
            new PropertyMetadata(Key.None));

        public static readonly DependencyProperty RequiredModifiersProperty = DependencyProperty.Register(
            nameof(RequiredModifiers), typeof(ModifierKeys), typeof(KeyboardOverlayControl),
            new PropertyMetadata(ModifierKeys.None));

        private readonly Dictionary<UIElement, bool> _originalHitTestValues = new();
        private bool _isKeyPressed;

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

        public KeyboardOverlayControl()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to global key events
            InputManager.Current.PreProcessInput += OnPreProcessInput;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from global key events
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
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
                    ShowOverlay();
                }
                else if (keyArgs.IsUp && _isKeyPressed)
                {
                    _isKeyPressed = false;
                    HideOverlay();
                }
            }
            
            // Handle modifier key changes while trigger key is pressed
            if (_isKeyPressed && !correctModifiers)
            {
                // If modifiers changed while trigger key is still pressed, close overlay
                bool isTriggerKeyStillDown = Keyboard.IsKeyDown(TriggerKey);
                if (isTriggerKeyStillDown && !correctModifiers)
                {
                    _isKeyPressed = false;
                    HideOverlay();
                }
            }

        }

        private static void OnOverlayVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is KeyboardOverlayControl control)
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
            
            // Restore background hit testing
            RestoreBackgroundHitTesting();

            // Hide overlay
            Visibility = Visibility.Collapsed;
            IsHitTestVisible = false;

            IsOverlayVisible = false;
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
                DisableHitTestingRecursively(parent, this);
            }
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
                kvp.Key.IsHitTestVisible = kvp.Value;
            }
            _originalHitTestValues.Clear();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsOverlayVisible && OverlayContent != null)
            {
                CenterOverlayContent();
            }
        }
    }
}
