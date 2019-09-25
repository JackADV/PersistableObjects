using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class GameDataWriter
{
    private BinaryWriter writer;

    public GameDataWriter(BinaryWriter writer)
    {
        this.writer = writer;
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }
    
    public void Write(Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

}
public class GameDataReader
{
    BinaryReader reader;

    public GameDataReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public int Readint()
    {
        return reader.ReadInt32();
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        value.w = reader.ReadSingle();
        return value;
    }

    public Vector3 ReadVector3()
    {
        Vector3 value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        return value;
    }
}
