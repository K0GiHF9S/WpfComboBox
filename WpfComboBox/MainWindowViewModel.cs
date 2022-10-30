using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfComboBox;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    public record A(string Name);
    public CompositeObservableCollection<A> NamesCollection { get; }
    public ObservableCollection<A> Names { get; }

    private string appendName = string.Empty;

    public string AppendName
    {
        get => appendName;
        set
        {
            appendName = value;
            OnPropertyChanged();
        }
    }

    private A? selectedName;

    public A? SelectedName
    {
        get => selectedName;
        set
        {
            selectedName = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand AppendCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefleshCommand { get; }

    public RelayCommand AppendCollectionCommand { get; }
    public RelayCommand DeleteCollectionCommand { get; }
    public RelayCommand RefleshCollectionCommand { get; }

    public MainWindowViewModel()
    {
        Names = new() { new("a"), new("b"), new("c") };
        AppendCommand = new(() => Names.Add(new(AppendName)));
        DeleteCommand = new(() =>
        {
            if (SelectedName is not null)
            {
                Names.Remove(SelectedName);
            }
        });
        RefleshCommand = new(() =>
        {
            var backup = Names.ToArray();
            Names.Clear();
            foreach (var item in backup)
            {
                Names.Add(item);
            }
        });
        NamesCollection = new();
        NamesCollection.Add(Names);
        AppendCollectionCommand = new(() => NamesCollection.Insert(0, new(new() { new("1"), new("2"), new("3") })));
        DeleteCollectionCommand = new(() => NamesCollection.RemoveAt(0));
        RefleshCollectionCommand = new(() =>
        {
            CompositeObservableCollection<A> backup = new(NamesCollection);
            NamesCollection.Clear();
            for (int i = 0; i < backup.Count; i++)
            {
                NamesCollection.Add(backup[i]);
            }
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
