using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfComboBox;

public class CompositeObservableCollection<T> : IEnumerable<T>, INotifyCollectionChanged, IWeakEventListener
{
    private sealed class CompositeObservableCollectionEnumerator : IEnumerator<T>
    {
        private IEnumerator<ObservableCollection<T>> _source;
        private IEnumerator<T>? _inner;

        public T Current => _inner is not null ? _inner.Current : throw new InvalidOperationException();

        object? IEnumerator.Current => Current;

        internal CompositeObservableCollectionEnumerator(ObservableCollection<ObservableCollection<T>> collection)
        {
            _source = collection.GetEnumerator();
            if (_source.MoveNext())
            {
                _inner = _source.Current.GetEnumerator();
            }
        }

        public bool MoveNext()
        {
            while (_inner?.MoveNext() ?? false)
            {
                return true;
            }
            if (_source.MoveNext())
            {
                _inner?.Dispose();
                _inner = _source.Current.GetEnumerator();
                return MoveNext();
            }
            return false;
        }

        public void Reset()
        {
            _inner?.Dispose();
            _source.Reset();
        }

        public void Dispose()
        {
            _inner?.Dispose();
            _source.Dispose();
        }
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private ObservableCollection<ObservableCollection<T>> _collection;

    public int Count => _collection.Count;

    public CompositeObservableCollection()
    {
        _collection = new();
    }

    public CompositeObservableCollection(CompositeObservableCollection<T> source)
    {
        _collection = new(source._collection);
    }

    public ObservableCollection<T> this[int index]
    {
        get => _collection[index];
        set
        {
            var old = this[index];
            if (old != value)
            {
                int count = _collection.Take(index).Sum(x => x.Count);
                RemoveCollection(old);
                AddCollection(value);
                _collection[index] = value;
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, old, count);
            }
        }
    }

    public IEnumerator<T> GetEnumerator() => new CompositeObservableCollectionEnumerator(_collection);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
        if (typeof(CollectionChangedEventManager) != managerType)
        {
            return false;
        }
        var notify = (NotifyCollectionChangedEventArgs)e;
        var source = (ObservableCollection<T>)sender;
        int count = _collection.TakeWhile(x => x != source).Sum(x => x.Count);
        NotifyCollectionChangedEventArgs new_notify = notify.Action switch
        {
            NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(notify.Action, notify.NewItems, count + notify.NewStartingIndex),
            NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(notify.Action, notify.OldItems, count + notify.OldStartingIndex),
            NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(notify.Action, notify.NewItems!, notify.OldItems!, count + notify.NewStartingIndex),
            NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(notify.Action, notify.NewItems, count + notify.NewStartingIndex, count + notify.OldStartingIndex),
            NotifyCollectionChangedAction.Reset => notify,
            _ => throw new NotSupportedException(),
        };
        CollectionChanged?.Invoke(sender, new_notify);
        return true;
    }

    public int IndexOf(ObservableCollection<T> item) => _collection.IndexOf(item);

    public void Insert(int index, ObservableCollection<T> item)
    {
        int count = _collection.Take(index).Sum(x => x.Count);
        AddCollection(item);
        _collection.Insert(index, item);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, count);
    }

    public void RemoveAt(int index)
    {
        if (0 <= index && index < Count)
        {
            int count = _collection.Take(index).Sum(x => x.Count);
            var item = this[index];
            RemoveCollection(item);
            _collection.RemoveAt(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, count);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"{index} is out of range.");
        }
    }

    public void Add(ObservableCollection<T> item)
    {
        int count = _collection.Sum(x => x.Count);
        AddCollection(item);
        _collection.Add(item);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, count);
    }

    public void Clear()
    {
        foreach (var item in _collection)
        {
            RemoveCollection(item);
        }
        _collection.Clear();
        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
    }

    public bool Contains(ObservableCollection<T> item) => _collection.Contains(item);
    public bool Contains(T item) => _collection.Any(x => x.Contains(item));

    public bool Remove(ObservableCollection<T> item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    private void AddCollection(ObservableCollection<T> item)
    {
        if (Contains(item))
        {
            throw new ArgumentException($"{item} is already exist.");
        }
        CollectionChangedEventManager.AddListener(item, this);
    }

    private void RemoveCollection(ObservableCollection<T> item)
    {
        CollectionChangedEventManager.RemoveListener(item, this);
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, ObservableCollection<T> changedItems, int startingIndex)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, ObservableCollection<T> newItems, ObservableCollection<T> oldItems, int startingIndex)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems, oldItems, startingIndex));
    }
}
