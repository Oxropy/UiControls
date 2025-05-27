using System.Drawing;

namespace UiControls;

public class CellChangedEventArgs : EventArgs
{
    public CellChangedEventArgs(CellsChangeType changeType, Point position, object? cellDataContext)
    {
        ChangeType = changeType;
        Position = position;
        CellDataContext = cellDataContext;
    }

    public CellsChangeType ChangeType { get; }
    public Point Position { get; }
    public object? CellDataContext { get; }
}