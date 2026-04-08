namespace Telecom_ThesisProject.Utilities;

public class NetworkDevicePollResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";

    public static NetworkDevicePollResult Ok(string sysName, string sysDescr)
    {
        var shortDescr = sysDescr.Length > 200 ? sysDescr[..200] + "…" : sysDescr;
        return new NetworkDevicePollResult
        {
            Success = true,
            Message = $"SNMP: OK. sysName: {sysName}\nsysDescr: {shortDescr}"
        };
    }

    public static NetworkDevicePollResult Fail(string error) =>
        new() { Success = false, Message = $"SNMP: ошибка — {error}" };
}
