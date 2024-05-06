using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Linq;

public enum MessageType
{
    HandShake = -1,
    Console = 0,
    Position = 1
}

public interface IMessage<T>
{
    public MessageType GetMessageType();
    public byte[] Serialize();
    public T Deserialize(byte[] message);
}

public class NetHandShakeC2S: IMessage<char[]>
{
    public char[] data;
    public char[] Deserialize(byte[] message)
    {
        char[] outData = new char[message.Length - 4];

        for (int i = 0; i < outData.Length; i++)
        {
            outData[i] = (char)message[i + 4];
        }
        
        // outData.Item1 = BitConverter.ToInt64(message, 4);
        // outData.Item2 = BitConverter.ToInt32(message, 12);

        return outData;
    }

    public MessageType GetMessageType()
    {
       return MessageType.HandShake;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }
        
        return outData.ToArray();
    }
}

public class NetHandShakeS2C : IMessage<Player>
{
   public Player data;
    
    public Player Deserialize(byte[] message)
    {
        char[] outData = new char[message.Length - 4];
            
        for (int i = 0; i < outData.Length; i++)
        {
            outData[i] = (char)message[i + 4];
        }

        return data;
    }

    public MessageType GetMessageType()
    {
       return MessageType.HandShake;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        return outData.ToArray();
    }
}

// //*
// /// <summary>
// ///  public Player Deserialize(byte[] message)
// {
//     int offset = 4;
//
//     // Deserializar ID
//     data.clientID = BitConverter.ToInt32(message, offset);
//     offset += sizeof(int);
//
//     // Deserializar Nombre
//     int nameLength = BitConverter.ToInt32(message, offset);
//     offset += sizeof(int);
//     data.name = Encoding.UTF8.GetString(message, offset, nameLength);
//     offset += nameLength;
//
//     // Deserializar Número de jugadores
//     int numPlayers = BitConverter.ToInt32(message, offset);
//     offset += sizeof(int);
//
//     // Deserializar Datos de jugadores
//     data.players = new List<Player>();
//     for (int i = 0; i < numPlayers; i++)
//     {
//         int playerId = BitConverter.ToInt32(message, offset);
//         offset += sizeof(int);
//
//         int playerNameLength = BitConverter.ToInt32(message, offset);
//         offset += sizeof(int);
//         string playerName = Encoding.UTF8.GetString(message, offset, playerNameLength);
//         offset += playerNameLength;
//
//         // Crear un nuevo objeto Player y agregarlo a la lista
//         data.players.Add(new Player { clientID = playerId, name = playerName });
//     }
//
//     return data;
// }
//
// public MessageType GetMessageType()
// {
//     return MessageType.HandShake;
// }
//
// public byte[] Serialize()
// {
//     // Serialize ID
//     List<byte> outData = new List<byte>();
//     outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
//     outData.AddRange(BitConverter.GetBytes(data.clientID));
//
//     // Serialize Name
//     byte[] nameBytes = Encoding.UTF8.GetBytes(data.name);
//     outData.AddRange(BitConverter.GetBytes(nameBytes.Length));
//     outData.AddRange(nameBytes);
//
//     return outData.ToArray();
// }
//
// /// </summary>
public class NetConsole : IMessage<string>
{
    string data;
    public string Deserialize(byte[] message)
    {
        char[] outData = new char[message.Length - 4];

        for (int i = 4; i < message.Length; i++)
        { 
            outData[i - 4] = (char)message[i];
        }

        return outData.ToString();
    }

    public MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(ASCIIEncoding.UTF8.GetBytes(data));


        return outData.ToArray();
    }
}

public class NetVector3 : IMessage<UnityEngine.Vector3>
{
    private static ulong lastMsgID = 0;
    private Vector3 data;

    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 8);
        outData.y = BitConverter.ToSingle(message, 12);
        outData.z = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}