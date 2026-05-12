using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

class ConnectionsViewModel : ObservableObject
{
    private readonly ConnectionService _connectionService;
    private readonly IMessageService _messageService;

    #region Properties

    private Connection _selectedConnection;
    public Connection SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            _selectedConnection = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public ObservableCollection<Connection> Connections { get; set; }

    public RelayCommand AddCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand EditCommand { get; }

    public Action<Connection?>? Navigate { get; set; }

    public ConnectionsViewModel()
    {
        _connectionService = new ConnectionService();
        _messageService = new MessageService();

        Connections = new ObservableCollection<Connection>();

        LoadData();

        AddCommand = new RelayCommand(_ => Navigate?.Invoke(null));
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedConnection != null);
        EditCommand = new RelayCommand(_ => Navigate?.Invoke(SelectedConnection), _ => SelectedConnection != null);
    }

    public void DeleteSelected()
    {
        if (SelectedConnection == null) return;

        if (_messageService.Confirm($"Удалить подключение №{SelectedConnection.Id}, {SelectedConnection.Client.GetFullNameIn}?"))
        {
            try
            {
                _connectionService.DeleteConnection(SelectedConnection);
                LoadData();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);                
            }
        }
    }

    public void LoadData()
    {
        var connections = _connectionService.GetAllConnections();
        Connections.Clear();
        foreach (var connection in connections)
        {
            Connections.Add(connection);
        }
    }

    public void Refresh()
    {
        LoadData();
    }
}
