using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Engine.Net;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>
/// Client KẾT NỐI THẬT tới server NRO/TeaMobi. Quy trình (port từ client mã nguồn mở):
///   1) TCP connect host:port.
///   2) Gửi message rỗng command -27 để xin KEY.
///   3) Nhận command -27 -> đọc key (byte length + key bytes + biến đổi cộng dồn XOR) -> SetKey.
///   4) Gửi setClientType (command -29, sub=2) rồi gửi login (command -29, sub=0: user/pass/version/type).
///   5) Lắng nghe & log mọi message server trả về.
///
/// LƯU Ý: khớp với giao thức NRO mã nguồn mở (version mặc định 2.3.0). Server CHÍNH THỨC
/// có thể dùng version/khác đôi chút -> chỉnh "Phiên bản client" và host/port ở tab Tài khoản.
/// </summary>
public sealed class RealGameClient : IGameClient
{
    private const sbyte CMD_GET_KEY = -27;
    private const sbyte CMD_NOT_LOGIN = -29;

    private readonly Session _session = new();
    private readonly TaskCompletionSource<bool> _keyReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private BotContext? _ctx;
    private long _power;
    private volatile bool _alive;

    public async Task ConnectAsync(BotContext ctx, CancellationToken ct)
    {
        _ctx = ctx;

        var host = ctx.Config.Launch.ServerHost;
        var port = ctx.Config.Launch.ServerPort;
        if (string.IsNullOrWhiteSpace(host) || port <= 0)
            throw new InvalidOperationException("Chưa cấu hình Server host/port (tab Tài khoản > Mở game).");

        _session.MessageReceived += OnMessage;
        _session.Disconnected += ex => { _alive = false; ctx.Log("Mất kết nối" + (ex is null ? "." : ": " + ex.Message)); };

        ctx.Log($"Đang kết nối {host}:{port} ...");
        await _session.ConnectAsync(host, port, ct);
        _alive = true;

        // Xin key (giống client thật: gửi message rỗng command -27).
        await _session.SendAsync(new Message(CMD_GET_KEY), ct);
        ctx.Log("Đã kết nối, đã gửi yêu cầu key (-27). Chờ server trả key...");
    }

    public async Task LoginAsync(BotContext ctx, CancellationToken ct)
    {
        // Chờ nhận được key.
        using (ct.Register(() => _keyReady.TrySetCanceled()))
        {
            var done = await Task.WhenAny(_keyReady.Task, Task.Delay(15000, ct));
            if (done != _keyReady.Task)
                throw new TimeoutException("Không nhận được key từ server (kiểm tra host/port/phiên bản).");
            await _keyReady.Task;
        }

        var version = string.IsNullOrWhiteSpace(ctx.Config.Launch.ClientVersion)
            ? "2.3.0"
            : ctx.Config.Launch.ClientVersion;

        // (1) setClientType — command -29, sub = 2
        using (var m = new Message(CMD_NOT_LOGIN))
        {
            m.WriteByte(2);
            m.WriteByte(3);                          // typeClient (ví dụ) — chỉnh nếu server yêu cầu khác
            m.WriteByte(1);                          // zoomLevel
            m.WriteBoolean(false);
            m.WriteInt(ctx.Config.Launch.GameWidth);
            m.WriteInt(ctx.Config.Launch.GameHeight);
            m.WriteBoolean(false);                   // isQwerty
            m.WriteBoolean(true);                    // isTouch
            m.WriteUTF("Windows|" + version);
            await _session.SendAsync(m, ct);
        }

        // (2) login — command -29, sub = 0: username, password, version, type
        using (var m = new Message(CMD_NOT_LOGIN))
        {
            m.WriteByte(0);
            m.WriteUTF(ctx.Account.Username);
            m.WriteUTF(ctx.Account.Password);
            m.WriteUTF(version);
            m.WriteByte(0);                          // type
            await _session.SendAsync(m, ct);
        }

        ctx.Log($"Đã gửi setClientType + đăng nhập (user='{ctx.Account.Username}', version={version}). Theo dõi message server bên dưới.");
    }

    private void OnMessage(Message m)
    {
        try
        {
            if (m.Command == CMD_GET_KEY)
            {
                HandleGetKey(m);
                return;
            }

            _ctx?.Log($"<< cmd={m.Command}, {m.GetData().Length} byte");
        }
        catch (Exception ex)
        {
            _ctx?.Log("Lỗi xử lý message: " + ex.Message);
        }
    }

    private void HandleGetKey(Message m)
    {
        int len = m.ReadUnsignedByte();
        var key = new byte[len];
        for (int i = 0; i < len; i++)
            key[i] = (byte)m.ReadByte();

        // Biến đổi key: key[j+1] ^= key[j]  (đúng theo client gốc).
        for (int j = 0; j < key.Length - 1; j++)
            key[j + 1] = (byte)(key[j + 1] ^ key[j]);

        _session.SetKey(key);
        _ctx?.Log($"Đã nhận & đặt key ({len} byte).");

        // Thông tin server gợi ý chuyển (nếu có) — chưa tự chuyển.
        try
        {
            var ip2 = m.ReadUTF();
            int port2 = m.ReadInt();
            bool connect2 = m.ReadByte() != 0;
            if (connect2 && !string.IsNullOrEmpty(ip2))
                _ctx?.Log($"(Server gợi ý chuyển sang {ip2}:{port2}.)");
        }
        catch
        {
            // không có thông tin redirect
        }

        _keyReady.TrySetResult(true);
    }

    public Task TradeAsync(BotContext ctx, CancellationToken ct) => Task.CompletedTask;

    public Task AttackAsync(BotContext ctx, CancellationToken ct)
    {
        // Phát hiện rớt kết nối để runner kích hoạt auto-restart.
        if (!_alive) throw new IOException("Mất kết nối tới server.");
        return Task.CompletedTask;
    }

    public Task PickupAsync(BotContext ctx, CancellationToken ct) => Task.CompletedTask;
    public Task FunctionsAsync(BotContext ctx, CancellationToken ct) => Task.CompletedTask;

    public Task<long> GetPowerAsync(CancellationToken ct) => Task.FromResult(_power);

    public void Dispose() => _session.Dispose();
}
