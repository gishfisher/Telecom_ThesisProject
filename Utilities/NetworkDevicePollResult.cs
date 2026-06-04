using Lextm.SharpSnmpLib;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Utilities.DTOs;

namespace Telecom_ThesisProject.Utilities;

public class NetworkDevicePollResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public List<DevicePortDto> Ports { get; init; } = new List<DevicePortDto>();
    public string UptimeStatus { get; init; } = "";

    // Проверка SNMP

    public static NetworkDevicePollResult SnmpOk(string sysName, string sysDescr)
    {
        var shortDescr = sysDescr.Length > 200 ? sysDescr[..200] + "…" : sysDescr;
        return new NetworkDevicePollResult
        {
            Success = true,
            Message = $"SNMP: OK. sysName: {sysName}\nsysDescr: {shortDescr}",
        };
    }

    public static NetworkDevicePollResult SnmpOk(List<DevicePortDto> portInfo)
    {
        return new NetworkDevicePollResult
        {
            Success = true,
            Message = $"SNMP: OK. {portInfo.Count} портов найдено.",
            Ports = portInfo
        };
    }

    public static NetworkDevicePollResult SnmpOk(string sysUpTime)
    {
        return new NetworkDevicePollResult
        {
            Success = true,
            Message = $"SNMP: OK. sysUpTime: {sysUpTime}",
            UptimeStatus = sysUpTime
        };
    }

    public static NetworkDevicePollResult SnmpFail(string error) =>
        new() { Success = false, Message = $"SNMP: ошибка — {error}" };

    // Проверка Ping

    public static NetworkDevicePollResult PingOk(string pingMessage) =>
        new() { Success = true, Message = $"ICMP: OK. {pingMessage}" };

    public static NetworkDevicePollResult PingFail(string error) => 
        new() { Success = false, Message = $"ICMP: ошибка — {error}" };

    // Проверка SSH

    public static NetworkDevicePollResult SshOk(string message) =>
        new() { Success = true, Message = $"SSH: {message}" }; 

    public static NetworkDevicePollResult SshFail(string error) => 
        new() { Success = false, Message = $"SSH: ошибка — {error}" };
}