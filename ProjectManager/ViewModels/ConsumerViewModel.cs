using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProjectManager.ViewModels;

public class ConsumerViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;

    public ObservableCollection<Consumer> Consumers { get; set; }

    private Consumer _selectedConsumer;
    public Consumer SelectedConsumer
    {
        get => _selectedConsumer;
        set
        {
            _selectedConsumer = value;
            OnPropertyChanged();
            if (value != null)
            {
                Phone = value.Phone;
                CommercialRegisterNo = value.CommercialRegisterNo;
            }
        }
    }

    private string _phone;
    public string Phone
    {
        get => _phone;
        set { _phone = value; OnPropertyChanged(); }
    }

    private string _commercialRegisterNo;
    public string CommercialRegisterNo
    {
        get => _commercialRegisterNo;
        set { _commercialRegisterNo = value; OnPropertyChanged(); }
    }

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCommand { get; }

    public ConsumerViewModel()
    {
        _dbContext = new AppDbContext();
        Consumers = new ObservableCollection<Consumer>(_dbContext.Consumers.ToList());

        AddCommand = new RelayCommand(AddConsumer);
        UpdateCommand = new RelayCommand(UpdateConsumer, () => SelectedConsumer != null);
        DeleteCommand = new RelayCommand(DeleteConsumer, () => SelectedConsumer != null);
        ClearCommand = new RelayCommand(ClearFields);
    }

    private void AddConsumer()
    {
        var existing = _dbContext.Consumers.Find(Phone);
        if (existing != null) return;

        var consumer = new Consumer
        {
            Phone = Phone,
            CommercialRegisterNo = CommercialRegisterNo
        };

        _dbContext.Consumers.Add(consumer);
        _dbContext.SaveChanges();
        Consumers.Add(consumer);
        ClearFields();
    }

    private void UpdateConsumer()
    {
        if (SelectedConsumer == null) return;

        SelectedConsumer.CommercialRegisterNo = CommercialRegisterNo;
        _dbContext.Entry(SelectedConsumer).State = EntityState.Modified;
        _dbContext.SaveChanges();

        // Refresh list
        LoadConsumers();
    }

    private void DeleteConsumer()
    {
        if (SelectedConsumer == null) return;

        _dbContext.Consumers.Remove(SelectedConsumer);
        _dbContext.SaveChanges();
        Consumers.Remove(SelectedConsumer);
        ClearFields();
    }

    private void ClearFields()
    {
        SelectedConsumer = null;
        Phone = string.Empty;
        CommercialRegisterNo = string.Empty;
    }

    private void LoadConsumers()
    {
        Consumers.Clear();
        foreach (var c in _dbContext.Consumers.ToList())
            Consumers.Add(c);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
