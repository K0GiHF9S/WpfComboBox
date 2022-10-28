using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfComboBox;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> Names { get; }

    private string appendName = String.Empty;

    public string AppendName
    {
        get => appendName;
        set
        {
            appendName = value;
            OnPropertyChanged();
        }
    }

    private string selectedName = String.Empty;

    public string SelectedName
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

    public MainWindowViewModel()
    {
        Names = new() { "a", "b", "c" };
        AppendCommand = new(() => Names.Add(AppendName));
        DeleteCommand = new(() => Names.Remove(SelectedName));
        RefleshCommand = new(() =>
        {
            var backup = Names.ToArray();
            Names.Clear();
            foreach (var item in backup)
            {
                Names.Add(item);
            }
        });
        PropertyChanged += MainWindowViewModel_PropertyChanged;
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
