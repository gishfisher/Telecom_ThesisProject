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
            return NetworkDevicePollResult.Fail("Не указан IP адрес.");

        if (!IPAddress.TryParse(device.IpAddress.Trim(), out var ip))
            return NetworkDevicePollResult.Fail("Некорректный IP адрес.");

        //var communityText = string.IsNullOrWhiteSpace(device.SnmpCommunity)
        //    ? "public"
        //    : device.SnmpCommunity.Trim();

        var endpoint = new IPEndPoint(ip, _options.SnmpPort);
        var community = new OctetString("public");
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
            return NetworkDevicePollResult.Ok(sysName, sysDescr);
        }
        catch (Exception ex)
        {
            return NetworkDevicePollResult.Fail(ex.Message);
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
}
