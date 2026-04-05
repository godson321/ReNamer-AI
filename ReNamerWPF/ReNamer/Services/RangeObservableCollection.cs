using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ReNamer.Services;

public class RangeObservableCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        var list = items?.ToList() ?? new List<T>();
        if (list.Count == 0)
            return;

        CheckReentrancy();
        foreach (var item in list)
            Items.Add(item);

        RaiseReset();
    }

    public int RemoveRange(IEnumerable<T> items)
    {
        var list = items?.Distinct().ToList() ?? new List<T>();
        if (list.Count == 0)
            return 0;

        CheckReentrancy();
        int removed = 0;
        foreach (var item in list)
        {
            if (Items.Remove(item))
                removed++;
        }

        if (removed > 0)
            RaiseReset();

        return removed;
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        var list = items?.ToList() ?? new List<T>();

        CheckReentrancy();
        Items.Clear();
        foreach (var item in list)
            Items.Add(item);

        RaiseReset();
    }

    private void RaiseReset()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
