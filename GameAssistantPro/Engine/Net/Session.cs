using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GameAssistantPro.Engine.Net;

/// <summary>
/// Phiên kết nối TCP + đọc/ghi message theo khung [command][length:2][data].
/// Có cơ chế mã hóa luồng XOR-key (kiểu mService/TeaMobi): sau khi <see cref="SetKey"/>,
/// mọi byte gửi/nhận được XOR với key theo chỉ số quay vòng.
///
/// LƯU Ý: khung gói + cơ chế key này theo MẪU CHUNG của game TeaMobi. Nếu NRO khác
/// (độ dài length, thứ tự, cách trao key...) thì chỉnh tại đây cho khớp.
/// </summary>
public sealed class Session : IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private byte[]? _key;
    private int _curRead, _curWrite;
    private CancellationTokenSource? _readCts;

    public bool IsConnected => _client?.Connected == true;

    public event Action<Message>? MessageReceived;
    public event Action<Exception?>? Disconnected;

    public async Task ConnectAsync(string host, int port, CancellationToken ct)
    {
        _client = new TcpClient { NoDelay = true };
        await _client.ConnectAsync(host, port, ct);
        _stream = _client.GetStream();
        _readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = Task.Run(() => ReadLoopAsync(_readCts.Token));
    }

    /// <summary>Đặt key mã hóa luồng (gọi sau khi nhận message chứa key từ server).</summary>
    public void SetKey(byte[] key)
    {
        _key = key;
        _curRead = 0;
        _curWrite = 0;
    }

    public async Task SendAsync(Message msg, CancellationToken ct)
    {
        if (_stream is null) throw new InvalidOperationException("Chưa kết nối.");

        var data = msg.GetData();
        var frame = new byte[3 + data.Length];
        frame[0] = (byte)msg.Command;
        frame[1] = (byte)(data.Length >> 8);
        frame[2] = (byte)(data.Length & 0xFF);
        Array.Copy(data, 0, frame, 3, data.Length);

        if (_key is not null)
            for (int i = 0; i < frame.Length; i++)
                frame[i] = WriteKey(frame[i]);

        await _stream.WriteAsync(frame.AsMemory(), ct);
        await _stream.FlushAsync(ct);
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _stream is not null)
            {
                int command = ReadKey(await ReadRawByteAsync(ct));
                int hi = ReadKey(await ReadRawByteAsync(ct));
                int lo = ReadKey(await ReadRawByteAsync(ct));
                int length = (hi << 8) | lo;

                var data = new byte[length];
                int off = 0;
                while (off < length)
                {
                    int n = await _stream.ReadAsync(data.AsMemory(off, length - off), ct);
                    if (n <= 0) throw new EndOfStreamException();
                    off += n;
                }

                if (_key is not null)
                    for (int i = 0; i < length; i++)
                        data[i] = ReadKey(data[i]);

                MessageReceived?.Invoke(new Message((sbyte)command, data));
            }
        }
        catch (OperationCanceledException)
        {
            // dừng bình thường
        }
        catch (Exception ex)
        {
            Disconnected?.Invoke(ex);
        }
    }

    private async Task<int> ReadRawByteAsync(CancellationToken ct)
    {
        var buf = new byte[1];
        int n = await _stream!.ReadAsync(buf.AsMemory(0, 1), ct);
        if (n <= 0) throw new EndOfStreamException();
        return buf[0];
    }

    private byte ReadKey(int b)
    {
        if (_key is null) return (byte)b;
        byte r = (byte)(_key[_curRead++] ^ b);
        if (_curRead >= _key.Length) _curRead = 0;
        return r;
    }

    private byte WriteKey(byte b)
    {
        if (_key is null) return b;
        byte r = (byte)(_key[_curWrite++] ^ b);
        if (_curWrite >= _key.Length) _curWrite = 0;
        return r;
    }

    public void Dispose()
    {
        try { _readCts?.Cancel(); } catch { }
        _stream?.Dispose();
        _client?.Dispose();
    }
}
