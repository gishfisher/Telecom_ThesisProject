using Telecom_ThesisProject.Core;

namespace Telecom_ThesisProject.MVVM.ViewModel;

/// <summary>Заглушка: соединения клиентов (Connection) привязаны к портам; отдельный CRUD можно добавить позже.</summary>
class ConnectionsViewModel : ObservableObject
{
    public string Info =>
        "Раздел «Соединения»: учёт подключений клиентов к портам устройств (таблица Connections). " +
        "Полное редактирование будет добавлено отдельно.";
}
