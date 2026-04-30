namespace Telecom_ThesisProject.Utilities;

public class NetworkLabOptions
{
    public int SnmpPort { get; set; } = 161;
    public int SnmpTimeoutMs { get; set; } = 3000;
    public int SshPort { get; set; } = 22;
    public int TcpProbeTimeoutMs { get; set; } = 2000;
    public int PingTimeoutMs { get; set; } = 1000;

    // == OIDs для SNMP запросов ===

    // Имя устройства
    public string SysNameOid { get; set; } = "1.3.6.1.2.1.1.5.0"; 
    // Описание устройства
    public string SysDescrOid { get; set; } = "1.3.6.1.2.1.1.1.0";
    // Время работы (Uptime) устройства
    public string SysUpTimeOid { get; set; } = "1.3.6.1.2.1.1.3.0";
    // Индекс интерфейса (для получения списка портов)
    public string IfIndexOid { get; set; } = "1.3.6.1.2.1.2.2.1.1";
    // Описание интерфейса (имя порта)
    public string IfDescrOid { get; set; } = "1.3.6.1.2.1.2.2.1.2"; 
    // Статус интерфейса (операционный статус)
    public string IfOperStatusOid { get; set; } = "1.3.6.1.2.1.2.2.1.8";
}
