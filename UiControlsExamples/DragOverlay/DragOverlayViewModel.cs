using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UiControls;
using UiControls.DropOverlay;
using UiControls.DropOverlay.ViewModel;

namespace UiControlsExamples.DragOverlay;

public class DragOverlayViewModel : ObservableObject, IDropOverlayHost
{
    public DragOverlayViewModel()
    {
        ObservableCollection<DropZoneViewModel> dropZoneViewModels = CreateDragOverlayZones();
        DropOverlayViewModel = new DropOverlayViewModel
        {
            DropZones = dropZoneViewModels,
            DefaultDropZone = dropZoneViewModels[0]
        };
        
        HandleDropCommand = new RelayCommand(HandleDrop);
        ShouldShowOverlayCommand = new RelayCommand(_ => { }, ShouldShowOverlayCanExecute);
    }

    public DropOverlayViewModel DropOverlayViewModel { get; }
    public ICommand HandleDropCommand { get; }
    public ICommand ShouldShowOverlayCommand { get; }
    
    private static void HandleDrop(object? parameter)
    {
        if (parameter is not DropEventArgs dropEventArgs)
            return;

        object? sender = dropEventArgs.Sender;
        DragEventArgs? e = dropEventArgs.DragEventArgs;
        DropZoneViewModel? dropZone = dropEventArgs.DropZone;

        // Handle the drop logic here
        if (sender is not Border border || e?.Data.GetData(DataFormats.Text) is not string draggedText) 
            return;
        
        string dropText = draggedText;
        if (dropZone != null)
        {
            dropText = $"{draggedText} ({dropZone.Identifier})";
        }

        if (border.Child is TextBlock textBlock)
        {
            textBlock.Text = dropText;
        }
    }
    
    private static bool ShouldShowOverlayCanExecute(object? arg)
    {
        if (arg is not DropEventArgs { DragEventArgs: { } dragEventArgs })
            return false;
        
        return (dragEventArgs.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey;
    }
    
    private static ObservableCollection<DropZoneViewModel> CreateDragOverlayZones()
    {
        return
        [
            new TextDropZoneViewModel
            {
                RelativeX = 0.375,
                RelativeY = 0.375,
                RelativeWidth = 0.25,
                RelativeHeight = 0.25,
                Identifier = "Center",
                Text = "",
                Background = Colors.LightBlue,
                Foreground = Colors.Black,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.1,
                RelativeY = 0,
                RelativeWidth = 0.8,
                RelativeHeight = 0.1,
                Identifier = "Top",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 0,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.9,
                RelativeY = 0.1,
                RelativeWidth = 0.1,
                RelativeHeight = 0.8,
                Identifier = "Right",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 90,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0.1,
                RelativeY = 0.9,
                RelativeWidth = 0.8,
                RelativeHeight = 0.1,
                Identifier = "Bottom",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 180,
                Background = Colors.LightBlue,
                CornerRadius = 5
            },
            new ImageDropZoneViewModel
            {
                RelativeX = 0,
                RelativeY = 0.1,
                RelativeWidth = 0.1,
                RelativeHeight = 0.8,
                Identifier = "Left",
                ImagePath = "/DragOverlay/Images/add_above.png",
                ImageWidth = 20,
                ImageHeight = 20,
                ImageRotation = 270,
                Background = Colors.LightBlue,
                CornerRadius = 5
            }
        ];
    }
}