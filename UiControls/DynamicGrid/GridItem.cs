namespace UiControls.DynamicGrid;

public sealed class GridItem : ObservableObject
{
    public int Row { get; set; }
    public int Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;
    public bool IsSelected { get; set; }

}