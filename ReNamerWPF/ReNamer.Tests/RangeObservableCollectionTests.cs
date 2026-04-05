using System.Collections.Specialized;
using System.ComponentModel;
using ReNamer.Services;

namespace ReNamer.Tests;

public class RangeObservableCollectionTests
{
    [Fact]
    public void AddRange_RaisesSingleResetAndPropertyNotifications()
    {
        var collection = new RangeObservableCollection<int>();
        var actions = new List<NotifyCollectionChangedAction>();
        var properties = new List<string>();

        collection.CollectionChanged += (_, e) => actions.Add(e.Action);
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName ?? string.Empty);

        collection.AddRange(new[] { 1, 2, 3 });

        Assert.Equal(new[] { 1, 2, 3 }, collection);
        Assert.Single(actions);
        Assert.Equal(NotifyCollectionChangedAction.Reset, actions[0]);
        Assert.Contains("Count", properties);
        Assert.Contains("Item[]", properties);
    }

    [Fact]
    public void RemoveRange_RaisesSingleResetWhenItemsRemoved()
    {
        var collection = new RangeObservableCollection<int> { 1, 2, 3, 4 };
        var actions = new List<NotifyCollectionChangedAction>();

        collection.CollectionChanged += (_, e) => actions.Add(e.Action);

        var removed = collection.RemoveRange(new[] { 2, 4, 4 });

        Assert.Equal(2, removed);
        Assert.Equal(new[] { 1, 3 }, collection);
        Assert.Single(actions);
        Assert.Equal(NotifyCollectionChangedAction.Reset, actions[0]);
    }

    [Fact]
    public void ReplaceAll_RaisesSingleResetAndReplacesContents()
    {
        var collection = new RangeObservableCollection<string> { "a", "b" };
        var actions = new List<NotifyCollectionChangedAction>();

        collection.CollectionChanged += (_, e) => actions.Add(e.Action);

        collection.ReplaceAll(new[] { "x", "y", "z" });

        Assert.Equal(new[] { "x", "y", "z" }, collection);
        Assert.Single(actions);
        Assert.Equal(NotifyCollectionChangedAction.Reset, actions[0]);
    }
}
