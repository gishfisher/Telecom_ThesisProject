using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Utilities.Interfaces;

public interface INetworkDevicePoller
{
    NetworkDevicePollResult PollSnmp(NetworkDevice device);

    bool TryProbeSshPort(string ipAddress, out string message);
}
