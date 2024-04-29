using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using UnityEngine;

public struct Client
{
    public string name;
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp, string name)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.name = name;
    }
}

public struct Player
{
    public string name;
    public int clientID;
    
    public Player(string name, int clientID)
    {
        this.name = name;
        this.clientID = clientID;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    
    private List<Player> Players = new List<Player>();

    int clientId = 0; // This id should be generated during first handshake

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartClient(IPAddress ip, int port, string clientName)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        AddClient(new IPEndPoint(ip, port), clientName);
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
            
            SendS2CHandShake(clientId, clientName);

            clientId++;
        }
    }

    public void SendS2CHandShake(int clientId, string clientName)
    {
        NetHandShakeS2C netHandShakeS2C = new NetHandShakeS2C();

         

        Broadcast(netHandShakeS2C.Serialize());
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip, string clientName)
    {
        AddClient(ip, clientName); 

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }


    public void SendToServer(byte[] data)
    {
        connection.Send(data);
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

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}
