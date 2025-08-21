using System.Windows;
using UiControls.DropOverlay.ViewModel;

namespace UiControls.DropOverlay.View;

public delegate void HandleDropDelegate(object sender, DragEventArgs e, DropZoneViewModel? dropZone);