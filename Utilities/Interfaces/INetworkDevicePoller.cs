using System.Threading.Tasks.Dataflow;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Utilities.Interfaces;

public interface INetworkDevicePoller
{
    // Синхронные методы
    NetworkDevicePollResult PollSnmp(NetworkDevice device);
    NetworkDevicePollResult PollPing(NetworkDevice device);
    NetworkDevicePollResult PollSnmpUpTime(NetworkDevice device);
    NetworkDevicePollResult GetSnmpPorts(NetworkDevice device);

    // Асинхронные методы
    Task<NetworkDevicePollResult> PollSnmpAsync(NetworkDevice device);
    Task<NetworkDevicePollResult> PollPingAsync(NetworkDevice device);
    Task<NetworkDevicePollResult> PollSnmpUpTimeAsync(NetworkDevice device);
    Task<NetworkDevicePollResult> GetSnmpPortsAsync(NetworkDevice device);
    Task<NetworkDevicePollResult> ProbeSshPortAsync(NetworkDevice device);

    bool TryProbeSshPort(string ipAddress, out string message);
}
