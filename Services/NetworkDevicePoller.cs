using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Xaml.Behaviors.Layout;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Utilities;
using Telecom_ThesisProject.Utilities.DTOs;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.Services;

public class NetworkDevicePoller : INetworkDevicePoller
{
    private readonly NetworkLabOptions _options;

    public NetworkDevicePoller(NetworkLabOptions? options = null)
    {
        // Загрузка настроек для конфигурирования запросов к сетевым устройствам
        _options = options ?? new NetworkLabOptions();
    }

    // === Синхронные методы ===

    public NetworkDevicePollResult PollSnmp(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile?.Community.Trim();

        var endpoint = new IPEndPoint(ip, _options.SnmpPort);
        var community = new OctetString(communityText!);
        
        var variables = new List<Variable>
        {
            new(new ObjectIdentifier("1.3.6.1.2.1.1.5.0")),
            new(new ObjectIdentifier("1.3.6.1.2.1.1.1.0")),
        };

        try
        {
            var response = Messenger.Get(VersionCode.V2, endpoint, community, variables, _options.SnmpTimeoutMs);
            var sysName = GetString(response, "1.3.6.1.2.1.1.5.0");
            var sysDescr = GetString(response, "1.3.6.1.2.1.1.1.0");
            return NetworkDevicePollResult.SnmpOk(sysName, sysDescr);
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    public NetworkDevicePollResult PollSnmpUpTime(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile?.Community.Trim();

        var endpoint = new IPEndPoint(ip, _options.SnmpPort);
        var community = new OctetString(communityText!);

        var variables = new List<Variable>
        {
            new(new ObjectIdentifier("1.3.6.1.2.1.1.3.0")),
        };

        try
        {
            var response = Messenger.Get(VersionCode.V2, endpoint, community, variables, _options.SnmpTimeoutMs);
            var sysUpTimeRaw = GetString(response, "1.3.6.1.2.1.1.3.0");

            if (TimeSpan.TryParse(sysUpTimeRaw, out TimeSpan ts))
            {
                string formattedTime = $"{(int)ts.TotalDays} дней, {ts:hh\\:mm\\:ss}";
                return NetworkDevicePollResult.SnmpOk(formattedTime);
            }

            return NetworkDevicePollResult.SnmpOk(sysUpTimeRaw);
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    public NetworkDevicePollResult GetSnmpPorts(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile!.Community.Trim();

        var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), _options.SnmpPort);
        var community = new OctetString(communityText);

        var descrResults = new List<Variable>();
        var statusResults = new List<Variable>();

        try
        {
            Messenger.Walk(VersionCode.V2, endpoint, community, new ObjectIdentifier("1.3.6.1.2.1.2.2.1.2"), descrResults, _options.SnmpTimeoutMs, WalkMode.WithinSubtree);
            Messenger.Walk(VersionCode.V2, endpoint, community, new ObjectIdentifier("1.3.6.1.2.1.2.2.1.8"), statusResults, _options.SnmpTimeoutMs, WalkMode.WithinSubtree);

            var ports = new List<DevicePortDto>();

            for (int i = 0; i < descrResults.Count; i++)
            {
                var statusValue = i < statusResults.Count ? statusResults[i].Data.ToString() : "0";

                ports.Add(new DevicePortDto
                {
                    DeviceId = device.Id,
                    PortName = descrResults[i].Data?.ToString() ?? $"Port {i}",
                    StatusRawValue = int.TryParse(statusValue, out var val) ? val : 0,
                    OperationalStatus = MapSnmpStatus(statusValue)
                });
            }

            return NetworkDevicePollResult.SnmpOk(ports);
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    public bool TryProbeSshPort(string ipAddress, out string message)
    {
        message = "";
        if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress.Trim(), out var ip))
        {
            message = "Некорректный IP.";
            return false;
        }

        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync(ip, _options.SshPort);
            if (!task.Wait(_options.TcpProbeTimeoutMs))
            {
                message = $"SSH: порт {_options.SshPort} не ответил за {_options.TcpProbeTimeoutMs} мс.";
                return false;
            }

            message = $"SSH: TCP-порт {_options.SshPort} доступен.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"SSH: {ex.Message}";
            return false;
        }
    }

    public NetworkDevicePollResult PollPing(NetworkDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.IpAddress) || !IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.PingFail("Некорректный IP адрес.");

        try
        {
            using (var pingSender = new Ping())
            {
                var reply = pingSender.Send(ip, 1000);

                if (reply.Status == IPStatus.Success)
                    return NetworkDevicePollResult.PingOk($"{reply.RoundtripTime} ms");
                else
                    return NetworkDevicePollResult.PingFail($"{reply.Status}");
            }
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.PingFail($"{ex.Message}");
        }
    }

    // === Асинхронные методы ===

    // Асинхронная версия метода для опроса SNMP
    public async Task<NetworkDevicePollResult> PollSnmpAsync(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile?.Community.Trim();

        var endpoint = new IPEndPoint(ip, device.SnmpProfile?.Port ?? _options.SnmpPort);
        var community = new OctetString(communityText!);

        var cts = new CancellationTokenSource(_options.SnmpTimeoutMs);

        var variables = new List<Variable>
        {
            new (new ObjectIdentifier(_options.SysNameOid)),
            new (new ObjectIdentifier(_options.SysDescrOid)),
        };

        try
        {
            var response = await Messenger.GetAsync(VersionCode.V2, endpoint, community, variables).WaitAsync(cts.Token);
            var sysName = GetString(response, _options.SysNameOid);
            var sysDescr = GetString(response, _options.SysDescrOid);
            return NetworkDevicePollResult.SnmpOk(sysName, sysDescr);
        }
        catch (OperationCanceledException)
        {
            return NetworkDevicePollResult.SnmpFail($"Таймаут: устройство не ответило за {_options.SnmpTimeoutMs} мс.");
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    // Асинхронная версия метода для опроса SNMP Uptime
    public async Task<NetworkDevicePollResult> PollSnmpUpTimeAsync(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile?.Community.Trim();

        var endpoint = new IPEndPoint(ip, device.SnmpProfile?.Port ?? _options.SnmpPort);
        var community = new OctetString(communityText!);
        
        var cts = new CancellationTokenSource(_options.SnmpTimeoutMs);
        
        var variables = new List<Variable> 
        { 
            new (new ObjectIdentifier(_options.SysUpTimeOid)) 
        };

        try
        {
            var response = await Messenger.GetAsync(VersionCode.V2, endpoint, community, variables).WaitAsync(cts.Token);

            var sysUpTimeRaw = GetString(response, _options.SysUpTimeOid);

            if (TimeSpan.TryParse(sysUpTimeRaw, out TimeSpan ts))
            {
                string formattedTime = $"{(int)ts.TotalDays} дн. {ts:hh\\:mm\\:ss}";
                return NetworkDevicePollResult.SnmpOk(formattedTime);
            }

            return NetworkDevicePollResult.SnmpOk(sysUpTimeRaw);
        }
        catch (OperationCanceledException)
        {
            return NetworkDevicePollResult.SnmpFail($"Таймаут: устройство не ответило за {_options.SnmpTimeoutMs} мс.");
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    // Асинхронная версия метода для получения информации о портах через SNMP
    public async Task<NetworkDevicePollResult> GetSnmpPortsAsync(NetworkDevice device)
    {
        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile!.Community.Trim();

        var endpoint = new IPEndPoint(ip, device.SnmpProfile?.Port ?? _options.SnmpPort);
        var community = new OctetString(communityText);

        var descrResults = new List<Variable>();
        var statusResults = new List<Variable>();

        var cts = new CancellationTokenSource(_options.SnmpTimeoutMs);

        try
        {
            await Messenger.WalkAsync(VersionCode.V2, endpoint, community, new ObjectIdentifier(_options.IfDescrOid), descrResults, WalkMode.WithinSubtree).
                            WaitAsync(cts.Token);
            await Messenger.WalkAsync(VersionCode.V2, endpoint, community, new ObjectIdentifier(_options.IfOperStatusOid), statusResults, WalkMode.WithinSubtree).
                            WaitAsync(cts.Token);

            var ports = new List<DevicePortDto>();

            for (int i = 0; i < descrResults.Count; i++)
            {
                var statusValue = i < statusResults.Count ? statusResults[i].Data.ToString() : "0";

                ports.Add(new DevicePortDto
                {
                    DeviceId = device.Id,
                    PortName = descrResults[i].Data?.ToString() ?? $"Port {i}",
                    StatusRawValue = int.TryParse(statusValue, out var val) ? val : 0,
                    OperationalStatus = MapSnmpStatus(statusValue)
                });
            }

            return NetworkDevicePollResult.SnmpOk(ports);
        }
        catch (OperationCanceledException)
        {
            return NetworkDevicePollResult.SnmpFail($"Таймаут: устройство не ответило за {_options.SnmpTimeoutMs} мс.");
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    // Асинхронная версия Ping-метода
    public async Task<NetworkDevicePollResult> PollPingAsync(NetworkDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.IpAddress) || !IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.PingFail("Некорректный IP адрес.");

        try
        {
            using (var pingSender = new Ping())
            {
                var reply = await pingSender.SendPingAsync(ip, _options.PingTimeoutMs);

                if (reply.Status == IPStatus.Success)
                {
                    return NetworkDevicePollResult.PingOk($"{reply.RoundtripTime} мс.");
                }
                else
                {
                    return NetworkDevicePollResult.PingFail($"{reply.Status}");
                }
            }
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.PingFail($"{ex.Message}");
        }
    }
    // Асинхронная версия метода для проверки доступности SSH-порта
    public async Task<NetworkDevicePollResult> ProbeSshPortAsync(NetworkDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.IpAddress) || !IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
        {
            return NetworkDevicePollResult.SshFail("Некорректный IP.");
        }

        var cts = new CancellationTokenSource(_options.TcpProbeTimeoutMs);

        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync(ip, _options.SshPort);
            await task.WaitAsync(cts.Token);

            return NetworkDevicePollResult.SshOk($"TCP-порт {_options.SshPort} доступен.");

        }
        catch (OperationCanceledException)
        {
            return NetworkDevicePollResult.SshFail($"Порт {_options.SshPort} не ответил за {_options.TcpProbeTimeoutMs} мс.");
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SshFail($"{ex.Message}");
        }
    }

    // === Вспомогательные методы ===

    // Вспомогательный метод для преобразования статуса порта
    private string MapSnmpStatus(string value)
    {
        switch (value)
        {
            case "1":
                return "Активен";
            case "2":
                return "Неактивен";
            case "3":
                return "Тестирование";
            default:
                return "Неизвестно";
        }
    }

    // Преобразование полученных данных в строку
    private static string GetString(IList<Variable> response, string oid)
    {
        foreach (var v in response)
        {
            if (v.Id.ToString() == oid && v.Data != null)
                return v.Data.ToString();
        }

        return string.Empty;
    }
}