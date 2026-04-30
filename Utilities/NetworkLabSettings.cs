using System.IO;
using Microsoft.Extensions.Configuration;

namespace Telecom_ThesisProject.Utilities;

public static class NetworkLabSettings
{
    //public static NetworkLabOptions Load()
    //{
    //    var config = new ConfigurationBuilder()
    //        .SetBasePath(Directory.GetCurrentDirectory())
    //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    //        .Build();

    //    var o = new NetworkLabOptions();
    //    var section = config.GetSection("NetworkLab");
    //    if (!section.Exists())
    //        return o;

    //    o.ManagementNetworkCidr = section["ManagementNetworkCidr"] ?? o.ManagementNetworkCidr;

    //    if (int.TryParse(section["SnmpPort"], out var sp)) o.SnmpPort = sp;
    //    if (int.TryParse(section["SnmpTimeoutMs"], out var st)) o.SnmpTimeoutMs = st;
    //    //o.SysNameOid = section["SysNameOid"] ?? o.SysNameOid;
    //    //o.SysDescrOid = section["SysDescrOid"] ?? o.SysDescrOid;
    //    if (int.TryParse(section["SshPort"], out var ssh)) o.SshPort = ssh;
    //    if (int.TryParse(section["TcpProbeTimeoutMs"], out var tcp)) o.TcpProbeTimeoutMs = tcp;

    //    return o;
    //}
}
