using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerNetworkManager : NetworkManager
{
    const int SERVER_ID = -5;
    protected readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    protected readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    public void StartServer(int port)
    {
        this.enabled = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    void AddClient(IPEndPoint ip, string clientName)
    {
        if (!ipToId.ContainsKey(ip))
        {
            int id = clientId;
            ipToId[ip] = clientId;


            clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup, clientName));
            Players.Add(new Player(clientName, clientId));

            Debug.Log("Player Connected with ID: " + clientId + " Name: " + clientName);

            SendS2CHandShake();

            clientId++;
        }
    }

    public void SendS2CHandShake()
    {
        NetHandShakeS2C netHandShakeS2C = new(Players);

        Broadcast(netHandShakeS2C.Serialize(SERVER_ID));
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    protected override void ManageData(byte[] data, IPEndPoint ip)
    {
        MessageType msgType = (MessageType)BitConverter.ToInt32(data, 0);
        
        if (!NetBaseMessage.ReCheckSum(data))
        {
            Debug.Log("Checksum failed");
            return;
        }
        
        switch (msgType)
        {
            case MessageType.ChatConsole:

                NetConsole netConsole = new NetConsole();
                string msg = netConsole.Deserialize(data);
                ChatScreen.Instance.onChatMessage.Invoke(msg, SERVER_ID);
                Broadcast(data);

                break;
            case MessageType.Position:
                break;
            case MessageType.HandShakeS2C:

                break;
            case MessageType.HandShakeC2S:

                NetHandShakeC2S netHandShakeC2S = new();
                AddClient(ip, netHandShakeC2S.Deserialize(data));
                NetPing netPing = new();
                SendToClient(netPing.Serialize(SERVER_ID), ip);
                Debug.Log("Initiate ping from: " + ip);
                break;
            case MessageType.Ping:
                
                NetPing netPing2 = new();
                SendToClient(netPing2.Serialize(SERVER_ID), ip);
                Debug.Log("Ping from: " + ip);
                break;
            default:
                break;
        }
    }

    protected override void CreateMessage(string message)
    {
        NetConsole netConsole = new(message);
        byte[] data = netConsole.Serialize(-5);
        Broadcast(data);
        ChatScreen.Instance.onChatMessage.Invoke(message, -5);
    }

    public void SendToClient(byte[] data, IPEndPoint ipEndpoint)
    {
        connection.Send(data, ipEndpoint);
    }
}