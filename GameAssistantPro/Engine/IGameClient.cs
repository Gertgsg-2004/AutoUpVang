using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameAssistantPro.Engine;

/// <summary>
/// Lớp giao tiếp với game. ĐÂY LÀ ĐIỂM CẮM giao thức thật của bạn.
/// Hiện có <see cref="SimulatedGameClient"/> để chạy mô phỏng. Khi có protocol,
/// hãy viết một lớp triển khai IGameClient (kết nối socket, gửi/nhận packet...)
/// rồi đổi factory trong MainViewModel sang lớp đó.
/// </summary>
public interface IGameClient : IDisposable
{
    /// <summary>Kết nối tới server (và proxy nếu có).</summary>
    Task ConnectAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Đăng nhập tài khoản.</summary>
    Task LoginAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Thực hiện một lượt giao dịch (nếu bật).</summary>
    Task TradeAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Thực hiện một lượt đánh quái.</summary>
    Task AttackAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Nhặt / xử lý đồ.</summary>
    Task PickupAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Các chức năng chung (mua bùa, vé vàng, mua item...).</summary>
    Task FunctionsAsync(BotContext ctx, CancellationToken ct);

    /// <summary>Lấy sức mạnh (SM) hiện tại.</summary>
    Task<long> GetPowerAsync(CancellationToken ct);
}
