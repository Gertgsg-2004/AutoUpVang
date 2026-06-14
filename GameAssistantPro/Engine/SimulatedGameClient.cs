using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>
/// Client MÔ PHỎNG (không kết nối game thật). Chỉ ghi log theo đúng cấu hình và
/// tăng SM giả lập để bạn thấy toàn bộ luồng engine + UI hoạt động.
/// Thay lớp này bằng client thật triển khai <see cref="IGameClient"/> khi có giao thức.
/// </summary>
public class SimulatedGameClient : IGameClient
{
    private readonly Random _rng = new();
    private long _power;

    public async Task ConnectAsync(BotContext ctx, CancellationToken ct)
    {
        var a = ctx.Account;
        var proxy = a.UseProxy ? $" qua proxy {a.ProxyHost}:{a.ProxyPort}" : "";
        ctx.Log($"Kết nối server '{a.Server}'{proxy}...");
        await Delay(ctx.Config.Launch.OpenGameDelayMs, ct);
        ctx.Log("Đã kết nối server.");
    }

    public async Task LoginAsync(BotContext ctx, CancellationToken ct)
    {
        ctx.Log($"Đăng nhập '{ctx.Account.Username}'...");
        await Delay(800, ct);
        _power = 1_000_000 + _rng.Next(0, 500_000);
        ctx.Log($"Đăng nhập thành công. SM ban đầu: {_power:N0}.");
    }

    public async Task TradeAsync(BotContext ctx, CancellationToken ct)
    {
        var tr = ctx.Config.Trade;
        ctx.Log($"Giao dịch tại map {tr.Map}-{tr.Zone} ({tr.Sets.Count} set, {tr.MapEntries.Count} dòng map).");
        await Delay(700, ct);
    }

    public async Task AttackAsync(BotContext ctx, CancellationToken ct)
    {
        var t = ctx.Config.Train;
        var target = t.SelectMode switch
        {
            MonsterSelectMode.ById => "theo ID: " + Ids(t.Monsters.Where(m => m.Enabled).Select(m => m.Id)),
            MonsterSelectMode.ByPosition => "theo vị trí",
            _ => "tất cả"
        };
        var filter = t.Filter switch
        {
            MonsterFilter.OnlyNormal => " (chỉ quái thường)",
            MonsterFilter.OnlySuper => " (chỉ siêu quái)",
            _ => ""
        };
        ctx.Log($"Đánh quái [{t.MarkMode}] {target}{filter}, HP<={t.HpLimitPercent}% @ map {t.Map}-{t.Zone}.");
        await Delay(900, ct);

        long gain = 50_000 + _rng.Next(0, 100_000);
        _power += gain;
        ctx.Log($"  +{gain:N0} SM (tổng {_power:N0}).");
    }

    public async Task PickupAsync(BotContext ctx, CancellationToken ct)
    {
        var p = ctx.Config.Train.Pickup;
        var extra = (p.AutoDrop ? ", auto vứt" : "") + (p.AutoDefault ? ", auto default" : "");
        ctx.Log($"Nhặt đồ [{p.Mode}]{extra}.");
        await Delay(500, ct);
    }

    public async Task FunctionsAsync(BotContext ctx, CancellationToken ct)
    {
        var f = ctx.Config.Function;

        if (f.Buff.Enabled)
        {
            var buffs = f.Buff.Buffs.Where(b => b.Enabled).Select(b => $"{b.Name} x{b.Quantity}").ToList();
            if (buffs.Count > 0)
                ctx.Log($"Mua bùa [{f.Buff.Duration}/{f.Buff.BuyMode}]: {string.Join(", ", buffs)}.");
        }
        if (f.GoldTicket.Enabled)
        {
            var e10 = f.GoldTicket.EnergyE10 ? ", E10" : "";
            ctx.Log($"Vé vàng: map {f.GoldTicket.EnterMap}{e10}, mua khi vàng >= {f.GoldTicket.BuyWhenGoldGte:N0}.");
        }
        if (f.BuyMask && f.MaskQty > 0)
            ctx.Log($"Mua khẩu trang x{f.MaskQty}.");
        if (f.BuyClover && f.CloverQty > 0)
            ctx.Log($"Mua cỏ 4 lá x{f.CloverQty}.");

        await Delay(400, ct);
    }

    public Task<long> GetPowerAsync(CancellationToken ct) => Task.FromResult(_power);

    public void Dispose() { }

    private static async Task Delay(int ms, CancellationToken ct)
        => await Task.Delay(Math.Clamp(ms, 200, 1500), ct);

    private static string Ids(IEnumerable<int> ids)
    {
        var s = string.Join(",", ids);
        return s.Length == 0 ? "(chưa có)" : s;
    }
}
