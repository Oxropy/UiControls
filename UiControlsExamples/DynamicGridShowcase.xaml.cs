namespace UiControlsExamples;

public partial class DynamicGridShowcase
{
    public DynamicGridShowcase()
    {
        InitializeComponent();
        DataContext = new DynamicGridShowcaseViewModel();
        DynamicGrid.RegisterViewType<ColoredCell, DefaultCellViewModel>();
        DynamicGrid.RegisterViewType<ColoredCell, ColoredCellViewModel>();
        ((DynamicGridShowcaseViewModel)DataContext).SetDefaultCells();
    }
}