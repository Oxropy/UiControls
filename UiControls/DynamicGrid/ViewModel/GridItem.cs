namespace UiControls.DynamicGrid.ViewModel;

public sealed record GridItem
{
    public int Row
    {
        get;
        set =>
            field = value >= 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Row must be greater than or equal to 0.");
    }

    public int Column
    {
        get;
        set =>
            field = value >= 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Column must be greater than or equal to 0.");
    }

    public int RowSpan
    {
        get;
        set =>
            field = value >= 1
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value, "RowSpan must be greater than 0.");
    } = 1;

    public int ColumnSpan
    {
        get;
        set =>
            field = value >= 1
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), value, "ColumnSpan must be greater than 0.");
    } = 1;

    public bool IsSelected { get; set; }
}