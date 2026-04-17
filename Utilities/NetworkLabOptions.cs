namespace Telecom_ThesisProject.Utilities;

public class NetworkLabOptions
{
    public string ManagementNetworkCidr { get; set; } = "192.168.0.0/24";
    public int SnmpPort { get; set; } = 161;
    public int SnmpTimeoutMs { get; set; } = 3000;
    public string SysNameOid { get; set; } = "1.3.6.1.2.1.1.5.0";
    public string SysDescrOid { get; set; } = "1.3.6.1.2.1.1.1.0";
    public int SshPort { get; set; } = 22;
    public int TcpProbeTimeoutMs { get; set; } = 2000;
}
