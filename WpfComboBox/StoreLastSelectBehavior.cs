using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;

namespace WpfComboBox;

internal class StoreLastSelectBehavior : Behavior<ComboBox>
{
    public static readonly DependencyProperty ComparerProperty =
        DependencyProperty.Register(nameof(Comparer), typeof(IEqualityComparer), typeof(StoreLastSelectBehavior), new FrameworkPropertyMetadata(EqualityComparer<object>.Default));
    public IEqualityComparer Comparer
    {
        get { return (IEqualityComparer)GetValue(ComparerProperty); }
        set { SetValue(ComparerProperty, value); }
    }

    private WeakReference<object?> _lastSelected = new(null);

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject.ItemsSource is INotifyCollectionChanged changed)
        {
            AssociatedObject.SelectionChanged += SelectionChanged;
            changed.CollectionChanged += CollectionChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject.ItemsSource is INotifyCollectionChanged changed)
        {
            changed.CollectionChanged -= CollectionChanged;
            AssociatedObject.SelectionChanged -= SelectionChanged;
        }
        base.OnDetaching();
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IEnumerable<object> collection
            && _lastSelected.TryGetTarget(out var last_selected)
            && collection.FirstOrDefault(o => Comparer.Equals(o, last_selected)) is object item)
        {
            AssociatedObject.SelectedItem = item;
        }
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1)
        {
            _lastSelected.SetTarget(e.AddedItems[0]);
        }
    }
}
