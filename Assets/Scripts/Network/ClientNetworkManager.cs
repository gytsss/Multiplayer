using System;
using System.Net;
using UnityEngine;

public class ClientNetworkManager : NetworkManager
{
    string clientName;

    public void StartClient(IPAddress ip, int port, string clientName)
    {
        this.enabled = true;
        this.port = port;
        this.ipAddress = ip;
        this.clientName = clientName;
        connection = new UdpConnection(ip, port, this);
        
        connection.Send(new NetHandShakeC2S(clientName).Serialize(-1));
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
                ChatScreen.Instance.onChatMessage.Invoke(msg, clientId);
                
                break;
            case MessageType.Position:
                break;
            case MessageType.HandShakeS2C:
                
                Debug.Log("HandShakeS2C");
                NetHandShakeS2C netHandShakeS2C = new NetHandShakeS2C();
                Players = netHandShakeS2C.Deserialize(data);
                foreach (var player in Players)
                {
                    if (player.name == clientName)
                    {
                        clientId = player.clientID;
                        Debug.Log($"Client {clientName} connected with ID: {clientId}");
                        break;
                    }
                }

                break;
            case MessageType.HandShakeC2S:
                break;
            case MessageType.Ping:
                NetPing netPing = new();
                SendToServer(netPing.Serialize(clientId));
                Debug.Log("Pong from: " + clientName);
                break;
            default:
                break;
        }
    }

    protected override void CreateMessage(string message)
    {
        NetConsole netConsole = new(message);
        byte[] data = netConsole.Serialize(clientId);
        SendToServer(data);
    }
    
    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }
    
    
}