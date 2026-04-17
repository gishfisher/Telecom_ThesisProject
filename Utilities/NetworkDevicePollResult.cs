namespace Telecom_ThesisProject.Utilities;

public class NetworkDevicePollResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";

    public static NetworkDevicePollResult SnmpOk(string sysName, string sysDescr)
    {
        var shortDescr = sysDescr.Length > 200 ? sysDescr[..200] + "…" : sysDescr;
        return new NetworkDevicePollResult
        {
            Success = true,
            Message = $"SNMP: OK. sysName: {sysName}\nsysDescr: {shortDescr}"
        };
    }

    public static NetworkDevicePollResult SnmpFail(string error) =>
        new() { Success = false, Message = $"SNMP: ошибка — {error}" };

    public static NetworkDevicePollResult PingOk(string pingMessage) =>
        new() { Success = true, Message = $"ICMP: OK. {pingMessage}" };

    public static NetworkDevicePollResult PingFail(string error) => 
        new() { Success = false, Message = $"ICMP: ошибка — {error}" };
}