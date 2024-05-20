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

public abstract class NetworkManager : MonoBehaviour, IReceiveData
{
    public IPAddress ipAddress { get; protected set; }

    public int port { get; protected set; }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    protected UdpConnection connection;

    protected List<Player> Players = new List<Player>();

    protected int clientId = 0;


    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);

        ManageData(data, ip);
    }

    protected abstract void ManageData(byte[] data, IPEndPoint ip);

    private void Start()
    {
        ChatScreen.Instance.onSendChatMessage.AddListener(CreateMessage);
    }

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
    

    protected abstract void CreateMessage(string message);
}