using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UiControlsExamples.KeyboardOverlay;

public partial class KeyboardOverlayShowcase
{
    public KeyboardOverlayShowcase()
    {
        InitializeComponent();
    }

    private void DragItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Child is TextBlock textBlock)
        {
            var dragData = new DataObject(DataFormats.Text, textBlock.Tag?.ToString() ?? textBlock.Text);
            DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
        }
    }

    private void DropArea_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.Text) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void DropArea_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            string? droppedData = e.Data.GetData(DataFormats.Text) as string;
            MessageBox.Show($"Dropped: {droppedData}", "Drop Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        e.Handled = true;
    }
}