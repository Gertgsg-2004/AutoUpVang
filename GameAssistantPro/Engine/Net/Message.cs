using System;
using System.IO;
using System.Text;

namespace GameAssistantPro.Engine.Net;

/// <summary>
/// Gói tin kiểu Java DataInputStream/DataOutputStream (big-endian) — mô hình message của
/// các game TeaMobi/J2ME (Ngọc Rồng, Avatar...). Khung gửi: [command:1][length:2 big-endian][data].
/// </summary>
public sealed class Message : IDisposable
{
    public sbyte Command { get; }
    private readonly MemoryStream _stream;

    /// <summary>Tạo message để GHI (gửi đi).</summary>
    public Message(sbyte command)
    {
        Command = command;
        _stream = new MemoryStream();
    }

    /// <summary>Tạo message từ dữ liệu ĐỌC được (nhận về).</summary>
    public Message(sbyte command, byte[] data)
    {
        Command = command;
        _stream = new MemoryStream();
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;
    }

    public byte[] GetData() => _stream.ToArray();
    public int Available => (int)(_stream.Length - _stream.Position);

    // ===== GHI (big-endian, giống Java DataOutputStream) =====
    public void WriteByte(int v) => _stream.WriteByte((byte)v);
    public void WriteBytes(byte[] v) => _stream.Write(v, 0, v.Length);
    public void WriteBoolean(bool v) => WriteByte(v ? 1 : 0);
    public void WriteShort(int v) { WriteByte(v >> 8); WriteByte(v); }
    public void WriteInt(int v) { WriteByte(v >> 24); WriteByte(v >> 16); WriteByte(v >> 8); WriteByte(v); }
    public void WriteLong(long v) { for (int i = 56; i >= 0; i -= 8) WriteByte((int)(v >> i)); }
    public void WriteUTF(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s ?? "");
        WriteShort(bytes.Length);
        WriteBytes(bytes);
    }

    // ===== ĐỌC =====
    private int ReadU8()
    {
        int b = _stream.ReadByte();
        if (b < 0) throw new EndOfStreamException();
        return b;
    }

    public sbyte ReadByte() => (sbyte)ReadU8();
    public int ReadUnsignedByte() => ReadU8();
    public bool ReadBoolean() => ReadU8() != 0;
    public short ReadShort() => (short)((ReadU8() << 8) | ReadU8());
    public int ReadUnsignedShort() => (ReadU8() << 8) | ReadU8();
    public int ReadInt() => (ReadU8() << 24) | (ReadU8() << 16) | (ReadU8() << 8) | ReadU8();

    public long ReadLong()
    {
        long v = 0;
        for (int i = 0; i < 8; i++) v = (v << 8) | (uint)ReadU8();
        return v;
    }

    public string ReadUTF()
    {
        int len = ReadUnsignedShort();
        var buf = new byte[len];
        int off = 0;
        while (off < len)
        {
            int n = _stream.Read(buf, off, len - off);
            if (n <= 0) throw new EndOfStreamException();
            off += n;
        }
        return Encoding.UTF8.GetString(buf);
    }

    public byte[] ReadBytes(int n)
    {
        var buf = new byte[n];
        int off = 0;
        while (off < n)
        {
            int r = _stream.Read(buf, off, n - off);
            if (r <= 0) throw new EndOfStreamException();
            off += r;
        }
        return buf;
    }

    public void Dispose() => _stream.Dispose();
}
