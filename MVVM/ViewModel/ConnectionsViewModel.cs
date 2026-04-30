using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.ViewModel;

class ConnectionsViewModel : ObservableObject
{
    private ConnectionService _connectionService;

    #region Properties

    private Connection? _selectedConnection = null;
    public Connection? SelectedConnection
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
        Connections = new ObservableCollection<Connection>();

        _connectionService = new ConnectionService();

        LoadData();

        AddCommand = new RelayCommand(_ => Navigate?.Invoke(null));
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedConnection != null);
        EditCommand = new RelayCommand(_ => Navigate?.Invoke(SelectedConnection), _ => SelectedConnection != null);
    }

    public void DeleteSelected()
    {
        if (SelectedConnection != null)
        {
            _connectionService.DeleteConnection(SelectedConnection);
            LoadData();
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
