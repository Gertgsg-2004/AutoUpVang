using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Engine.Net;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>
/// Client KẾT NỐI THẬT tới server game qua TCP (khung message kiểu TeaMobi/J2ME).
///
/// ⚠️ ĐÂY LÀ KHUNG. Phần kết nối socket + đọc/ghi message là THẬT, nhưng các giá trị
/// RIÊNG của Ngọc Rồng phải được điền cho khớp (lấy từ tool/source cũ của bạn — DataNro/keyAPI):
///   1) Server host/port  -> tab Tài khoản > Mở game.
///   2) Cách server trao KEY mã hóa (message nào, key nằm ở đâu trong payload).
///   3) OPCODE + định dạng gói ĐĂNG NHẬP, và opcode báo login OK/FAIL.
///   4) Opcode các hành động (đánh quái, nhặt đồ...).
///
/// Khi chạy, client sẽ in ra mọi message server gửi về (cmd + số byte) ở tab Nhật ký
/// -> dùng để DÒ đúng opcode rồi điền vào các chỗ TODO bên dưới.
/// </summary>
public sealed class RealGameClient : IGameClient
{
    // ===== HẰNG SỐ GIAO THỨC — VÍ DỤ, CẦN XÁC MINH/THAY CHO KHỚP NRO =====
    private const sbyte CMD_GET_KEY = -27;   // message server gửi key (ví dụ ở nhiều game TeaMobi)
    private const sbyte CMD_LOGIN   = -127;  // lệnh đăng nhập (ví dụ)

    private readonly Session _session = new();
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
        ctx.Log("Đã mở socket TCP. Chờ key/handshake từ server...");

        // TODO (nếu NRO yêu cầu): gửi message khởi tạo đầu tiên ở đây.
    }

    public async Task LoginAsync(BotContext ctx, CancellationToken ct)
    {
        // ==== MẪU gói đăng nhập — SỬA cho khớp NRO ====
        using var m = new Message(CMD_LOGIN);
        m.WriteUTF(ctx.Account.Username);
        m.WriteUTF(ctx.Account.Password);
        m.WriteByte(0);            // ví dụ: kiểu đăng nhập
        m.WriteUTF("v1.4.0");      // ví dụ: phiên bản client
        await _session.SendAsync(m, ct);

        ctx.Log("Đã gửi gói đăng nhập (MẪU). Xem các message server trả về bên dưới để xác định opcode login OK/FAIL.");
        // Lưu ý: chưa chờ xác nhận login vì chưa biết opcode trả về của NRO -> để kết nối mở,
        // quan sát log message đến rồi điền opcode vào OnMessage().
        await Task.CompletedTask;
    }

    private void OnMessage(Message m)
    {
        try
        {
            switch (m.Command)
            {
                case CMD_GET_KEY:
                    // TODO: nhiều game key nằm trong payload theo định dạng riêng (đọc đúng rồi SetKey).
                    var key = m.GetData();
                    _session.SetKey(key);
                    _ctx?.Log($"Đã nhận & đặt key ({key.Length} byte).");
                    break;

                // TODO: thêm các case opcode của NRO, ví dụ:
                // case CMD_LOGIN_OK:  _ctx?.Log("Đăng nhập THÀNH CÔNG."); break;
                // case CMD_LOGIN_FAIL: _ctx?.Log("Đăng nhập THẤT BẠI: " + m.ReadUTF()); break;
                // case CMD_PLAYER_INFO: _power = m.ReadLong(); break;

                default:
                    _ctx?.Log($"<< message cmd={m.Command}, {m.GetData().Length} byte");
                    break;
            }
        }
        catch (Exception ex)
        {
            _ctx?.Log("Lỗi xử lý message: " + ex.Message);
        }
    }

    // Các hành động — điền gói tương ứng khi đã biết opcode.
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
