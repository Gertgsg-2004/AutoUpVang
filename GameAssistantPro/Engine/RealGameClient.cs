using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Engine.Net;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>
/// Client KẾT NỐI THẬT tới server NRO/TeaMobi (port từ client mã nguồn mở).
/// Quy trình: connect -> xin key (-27) -> SetKey -> setClientType + login (-29)
/// -> chọn skill (34) -> đánh quái (54) -> nhặt đồ (-20) theo đồ rơi (cmd 68).
///
/// Opcode/định dạng khớp giao thức NRO mã nguồn mở. Server CHÍNH THỨC có thể khác
/// version -> chỉnh "Phiên bản client" + host/port ở tab Tài khoản.
/// </summary>
public sealed class RealGameClient : IGameClient
{
    // Opcode (port từ Cmd.cs / Service.cs mã nguồn mở)
    private const sbyte CMD_GET_KEY = -27;
    private const sbyte CMD_NOT_LOGIN = -29;
    private const sbyte CMD_SELECT_SKILL = 34;
    private const sbyte CMD_ATTACK_NPC = 54;
    private const sbyte CMD_PICK_ITEM = -20;
    private const sbyte CMD_ADD_ITEM_MAP = 68;
    private const sbyte CMD_LOGIN_FAIL = -102;

    // Số slot quái tối đa để "tàn sát" (chưa parse map nên đánh hết slot; server bỏ qua slot trống/chết).
    private const int MaxMobSlots = 30;

    private readonly Session _session = new();
    private readonly TaskCompletionSource<bool> _keyReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _lock = new();
    private readonly List<(short itemMapId, short templateId)> _droppedItems = new();
    private readonly HashSet<sbyte> _seenCommands = new();

    private BotContext? _ctx;
    private long _power;
    private volatile bool _alive;
    private bool _skillSelected;

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

        await _session.SendAsync(new Message(CMD_GET_KEY), ct);
        ctx.Log("Đã kết nối, đã gửi yêu cầu key (-27). Chờ server trả key...");
    }

    public async Task LoginAsync(BotContext ctx, CancellationToken ct)
    {
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

        // setClientType (command -29, sub = 2)
        using (var m = new Message(CMD_NOT_LOGIN))
        {
            m.WriteByte(2);
            m.WriteByte(3);
            m.WriteByte(1);
            m.WriteBoolean(false);
            m.WriteInt(ctx.Config.Launch.GameWidth);
            m.WriteInt(ctx.Config.Launch.GameHeight);
            m.WriteBoolean(false);
            m.WriteBoolean(true);
            m.WriteUTF("Windows|" + version);
            await _session.SendAsync(m, ct);
        }

        // login (command -29, sub = 0)
        using (var m = new Message(CMD_NOT_LOGIN))
        {
            m.WriteByte(0);
            m.WriteUTF(ctx.Account.Username);
            m.WriteUTF(ctx.Account.Password);
            m.WriteUTF(version);
            m.WriteByte(0);
            await _session.SendAsync(m, ct);
        }

        ctx.Log($"Đã gửi đăng nhập (user='{ctx.Account.Username}', version={version}).");
    }

    public async Task TradeAsync(BotContext ctx, CancellationToken ct)
    {
        await Task.CompletedTask; // TODO: cài gói giao dịch khi cần
    }

    public async Task AttackAsync(BotContext ctx, CancellationToken ct)
    {
        if (!_alive) throw new IOException("Mất kết nối tới server.");

        // Chọn skill 1 lần (lấy skill đầu trong danh sách cấu hình).
        if (!_skillSelected)
        {
            var skill = ctx.Config.Train.Skills.FirstOrDefault();
            if (skill is not null)
            {
                await SelectSkillAsync(skill.Id, ct);
                ctx.Log($"Đã chọn skill {skill.Id}.");
            }
            _skillSelected = true;
        }

        // "Tàn sát": gửi 1 gói đánh tất cả slot quái (server bỏ qua slot trống/chết).
        using var m = new Message(CMD_ATTACK_NPC);
        for (int i = 0; i < MaxMobSlots; i++)
            m.WriteByte(i);
        await _session.SendAsync(m, ct);
    }

    public async Task PickupAsync(BotContext ctx, CancellationToken ct)
    {
        if (!_alive) return;

        List<(short itemMapId, short templateId)> items;
        lock (_lock)
        {
            if (_droppedItems.Count == 0) return;
            items = _droppedItems.ToList();
            _droppedItems.Clear();
        }

        var pickup = ctx.Config.Train.Pickup;
        foreach (var it in items)
        {
            if (ShouldPick(pickup, it.templateId))
            {
                await PickItemAsync(it.itemMapId, ct);
                ctx.Log($"Nhặt đồ (itemMapId={it.itemMapId}, tpl={it.templateId}).");
            }
        }
    }

    public Task FunctionsAsync(BotContext ctx, CancellationToken ct) => Task.CompletedTask;

    public Task<long> GetPowerAsync(CancellationToken ct) => Task.FromResult(_power);

    public void Dispose() => _session.Dispose();

    // ===== Gửi hành động =====
    private async Task SelectSkillAsync(int skillTemplateId, CancellationToken ct)
    {
        using var m = new Message(CMD_SELECT_SKILL);
        m.WriteShort(skillTemplateId);
        await _session.SendAsync(m, ct);
    }

    private async Task PickItemAsync(int itemMapId, CancellationToken ct)
    {
        using var m = new Message(CMD_PICK_ITEM);
        m.WriteShort(itemMapId);
        await _session.SendAsync(m, ct);
    }

    private static bool ShouldPick(PickupConfig pickup, short templateId)
    {
        return pickup.Mode switch
        {
            PickupMode.All => true,
            PickupMode.ById or PickupMode.ByType => pickup.Items.Any(e => e.Id == templateId),
            PickupMode.Exclude => !pickup.Items.Any(e => e.Id == templateId),
            _ => true
        };
    }

    // ===== Nhận message =====
    private void OnMessage(Message m)
    {
        try
        {
            switch (m.Command)
            {
                case CMD_GET_KEY:
                    HandleGetKey(m);
                    return;

                case CMD_ADD_ITEM_MAP:
                    HandleItemDrop(m);
                    return;

                case CMD_LOGIN_FAIL:
                    string reason = TryReadUtf(m);
                    _ctx?.Log("Đăng nhập THẤT BẠI: " + reason);
                    return;
            }

            // Log mỗi opcode lạ 1 lần (để dò giao thức, tránh ngập log).
            if (_seenCommands.Add(m.Command))
                _ctx?.Log($"<< cmd={m.Command} ({m.GetData().Length} byte) [lần đầu]");
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

        for (int j = 0; j < key.Length - 1; j++)
            key[j + 1] = (byte)(key[j + 1] ^ key[j]);

        _session.SetKey(key);
        _ctx?.Log($"Đã nhận & đặt key ({len} byte).");

        try
        {
            var ip2 = m.ReadUTF();
            int port2 = m.ReadInt();
            bool connect2 = m.ReadByte() != 0;
            if (connect2 && !string.IsNullOrEmpty(ip2))
                _ctx?.Log($"(Server gợi ý chuyển sang {ip2}:{port2}.)");
        }
        catch { /* không có thông tin redirect */ }

        _keyReady.TrySetResult(true);
    }

    private void HandleItemDrop(Message m)
    {
        short itemMapId = m.ReadShort();
        short templateId = m.ReadShort();
        m.ReadShort(); // x
        m.ReadShort(); // y
        int owner = m.ReadInt();
        if (owner == -2)
            m.ReadShort();

        lock (_lock)
            _droppedItems.Add((itemMapId, templateId));
    }

    private static string TryReadUtf(Message m)
    {
        try { return m.ReadUTF(); }
        catch { return "(không rõ)"; }
    }
}
