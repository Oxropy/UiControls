using System.Collections;
using System.Collections.Specialized;

namespace UiControls.DynamicGrid.ViewModel;

internal sealed class GridItemHostSyncList : IList<IGridItemHost>, INotifyCollectionChanged
{
    private readonly List<IGridItemHost> _items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    public int Count => _items.Count;
    public bool IsReadOnly => false;
    public IGridItemHost this[int index] { get => _items[index]; set => _items[index] = value; }

    public void Add(IGridItemHost item) => _items.Add(item); 
    public void AddRange(params IEnumerable<IGridItemHost> items) => _items.AddRange(items); 
    public void Clear() => _items.Clear();
    public bool Contains(IGridItemHost item) => _items.Contains(item);
    public void CopyTo(IGridItemHost[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public IEnumerator<IGridItemHost> GetEnumerator() => _items.GetEnumerator();
    public int IndexOf(IGridItemHost item) => _items.IndexOf(item);
    public void Insert(int index, IGridItemHost item) => _items.Insert(index, item);
    public bool Remove(IGridItemHost item) => _items.Remove(item);
    public void RemoveAt(int index) => _items.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public void Sync()
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _items));
    }
}