namespace UiControlsExamples;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        DynamicGrid.RegisterViewType<ColoredCell, DefaultCellViewModel>();
        DynamicGrid.RegisterViewType<ColoredCell, ColoredCellViewModel>();
        ((MainWindowViewModel)DataContext).SetDefaultCells();
    }
}