using System.Net;

public interface IReceiveData
{
    void OnReceiveData(byte[] data, IPEndPoint ipEndpoint, string name);
}