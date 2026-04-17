using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Utilities;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.Services;

public class SnmpNetworkDevicePoller : INetworkDevicePoller
{
    private readonly NetworkLabOptions _options;

    public SnmpNetworkDevicePoller(NetworkLabOptions? options = null)
    {
        _options = options ?? NetworkLabSettings.Load();
    }

    public NetworkDevicePollResult PollSnmp(NetworkDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.IpAddress))
            return NetworkDevicePollResult.SnmpFail("Не указан IP адрес.");

        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.SnmpFail("Некорректный IP адрес.");

        var communityText = string.IsNullOrWhiteSpace(device.SnmpProfile?.Community)
            ? "public"
            : device.SnmpProfile?.Community.Trim();

        var endpoint = new IPEndPoint(ip, _options.SnmpPort);
        var community = new OctetString(communityText!);
        var variables = new List<Variable>
        {
            new(new ObjectIdentifier(_options.SysNameOid)),
            new(new ObjectIdentifier(_options.SysDescrOid)),
        };

        try
        {
            var response = Messenger.Get(VersionCode.V2, endpoint, community, variables, _options.SnmpTimeoutMs);
            var sysName = GetString(response, _options.SysNameOid);
            var sysDescr = GetString(response, _options.SysDescrOid);
            return NetworkDevicePollResult.SnmpOk(sysName, sysDescr);
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.SnmpFail(ex.Message);
        }
    }

    private static string GetString(IList<Variable> response, string oid)
    {
        foreach (var v in response)
        {
            if (v.Id.ToString() == oid && v.Data != null)
                return v.Data.ToString();
        }

        return "—";
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
            using (var pingSender = new System.Net.NetworkInformation.Ping())
            {
                var reply = pingSender.Send(ip, 1000);

                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return NetworkDevicePollResult.PingOk($"{reply.RoundtripTime} ms");
                else
                    return NetworkDevicePollResult.PingFail($"{reply.Status}");
            }
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.PingFail($"Ошибка при попытке пинга: {ex.Message}");
        }
    }
}