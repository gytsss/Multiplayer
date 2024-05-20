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
    HandShakeS2C = -2,
    HandShakeC2S = -1,
    ChatConsole = 0,
    Position = 1,
    Ping = 2
}

public abstract class NetBaseMessage
{
    protected MessageType msgType;
    public abstract MessageType GetMessageType();
    public abstract byte[] Serialize(int playerId);

    public NetBaseMessage()
    {
    }

    public void AddCheckSum(List<byte> outData)
    {
        ulong checkSum = 0;
        ulong checkSum2 = 0;
        for (int i = 0; i < outData.Count; i++)
        {
            if (outData[i] > 200)
            {
                checkSum += outData[i];
                checkSum2 -= outData[i];
            }
            else if (outData[i] > 150)
            {
                checkSum -= outData[i];
                checkSum2 += outData[i];
            }
            else if (outData[i] > 100)
            {
                checkSum >>= outData[i];
                checkSum2 <<= outData[i];
            }
            else
            {
                checkSum <<= outData[i];
                checkSum2 >>= outData[i];
            }
        }

        outData.AddRange(BitConverter.GetBytes(checkSum));
        outData.AddRange(BitConverter.GetBytes(checkSum2));
    }

    public static bool ReCheckSum(byte[] message)
    {
        ulong checkSum = 0;
        ulong checkSum2 = 0;

        for (int i = 0; i < message.Length - 16; i++)
        {
            if (message[i] > 200)
            {
                checkSum += message[i];
                checkSum2 -= message[i];
            }
            else if (message[i] > 150)
            {
                checkSum -= message[i];
                checkSum2 += message[i];
            }
            else if (message[i] > 100)
            {
                checkSum >>= message[i];
                checkSum2 <<= message[i];
            }
            else
            {
                checkSum <<= message[i];
                checkSum2 >>= message[i];
            }
        }

        if (checkSum == BitConverter.ToUInt64(message, message.Length - 16) &&
            checkSum2 == BitConverter.ToUInt64(message, message.Length - 8))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public abstract class NetBaseMessage<T> : NetBaseMessage
{
    public abstract T Deserialize(byte[] message);
    protected T data;

    protected NetBaseMessage(T data)
    {
        this.data = data;
    }

    protected NetBaseMessage()
    {
    }
}

public class NetPing : NetBaseMessage<int>
{
    public override int Deserialize(byte[] message)
    {
        return 0;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Ping;
    }

    public override byte[] Serialize(int playerId)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType())); //0-3
        outData.AddRange(BitConverter.GetBytes(playerId)); //4-7

        AddCheckSum(outData);
        return outData.ToArray();
    }
}

public abstract class NumeredNetBaseMessage<T> : NetBaseMessage<T>
{
    protected static ulong lastMsgID = 0;

    protected NumeredNetBaseMessage(T data) : base(data)
    {
    }

    protected NumeredNetBaseMessage()
    {
    }
    
}

public class NetHandShakeC2S : NetBaseMessage<string>
{
    public override string Deserialize(byte[] message)
    {
        string outData = "";

        int dataLength = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < dataLength; i++)
        {
            outData += (char)message[i + 12];
        }


        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.HandShakeC2S;
    }

    public override byte[] Serialize(int playerId)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType())); //0-3
        outData.AddRange(BitConverter.GetBytes(playerId)); //4-7

        outData.AddRange(BitConverter.GetBytes(data.Length)); //8-11

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        AddCheckSum(outData);
        return outData.ToArray();
    }

    public NetHandShakeC2S(string data) : base(data)
    {
    }

    public NetHandShakeC2S() : base()
    {
    }
}

public class NetHandShakeS2C : NetBaseMessage<List<Player>>
{
    public override List<Player> Deserialize(byte[] message)
    {
        List<Player> outData = new List<Player>();

        int listSize = BitConverter.ToInt32(message, 8);
        int charsCount = 0;

        for (int i = 0; i < listSize; i++)
        {
            Player player = new Player();
            player.clientID = BitConverter.ToInt32(message, 12 + charsCount);

            int nameLength = BitConverter.ToInt32(message, 16 + charsCount);

            string name = "";

            for (int j = 0; j < nameLength; j++)
            {
                name += (char)message[20 + j + charsCount];
            }

            charsCount += nameLength + 8;
            player.name = name;
            outData.Add(player);
        }

        return outData;
    }


    public override MessageType GetMessageType()
    {
        return MessageType.HandShakeS2C;
    }

    public override byte[] Serialize(int playerId)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType())); //0-3
        outData.AddRange(BitConverter.GetBytes(playerId)); //4-7

        int listSize = data.Count;

        outData.AddRange(BitConverter.GetBytes(listSize)); //8-11

        for (int i = 0; i < listSize; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i].clientID));
            outData.AddRange(BitConverter.GetBytes(data[i].name.Length));

            for (int j = 0; j < data[i].name.Length; j++)
            {
                outData.Add((byte)data[i].name[j]);
            }
        }

        AddCheckSum(outData);
        return outData.ToArray();
    }

    public NetHandShakeS2C(List<Player> data) : base(data)
    {
    }

    public NetHandShakeS2C()
    {
    }
}

public class NetConsole : NumeredNetBaseMessage<string>
{
    public override string Deserialize(byte[] message)
    {
        string outData = "";

        int dataLength = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < dataLength; i++)
        {
            outData += (char)message[i + 12];
        }

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ChatConsole;
    }

    public override byte[] Serialize(int playerId)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType())); //0-3
        outData.AddRange(BitConverter.GetBytes(playerId)); //4-7

        outData.AddRange(BitConverter.GetBytes(data.Length)); //8-11

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }
        AddCheckSum(outData);
        return outData.ToArray();
    }
    
    public NetConsole(string data) : base(data)
    {
    }

    public NetConsole() : base()
    {
    }
}

public class NetVector3 : NumeredNetBaseMessage<Vector3>
{
    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 16);
        outData.y = BitConverter.ToSingle(message, 20);
        outData.z = BitConverter.ToSingle(message, 24);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public override byte[] Serialize(int playerId)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType())); //0-3
        outData.AddRange(BitConverter.GetBytes(playerId)); //4-7
        outData.AddRange(BitConverter.GetBytes(lastMsgID++)); // 8-15
        outData.AddRange(BitConverter.GetBytes(data.x)); //16-19
        outData.AddRange(BitConverter.GetBytes(data.y)); //20-23
        outData.AddRange(BitConverter.GetBytes(data.z)); //24-27
        
        AddCheckSum(outData);
        
        return outData.ToArray();
    }

    public NetVector3(Vector3 data) : base(data)
    {
    }
}