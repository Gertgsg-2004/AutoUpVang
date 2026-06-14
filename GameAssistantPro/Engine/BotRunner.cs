using System;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>Quản lý vòng chạy của MỘT tài khoản trên một luồng nền riêng (có auto-restart + thống kê).</summary>
public class BotRunner : ObservableObject
{
    private readonly Func<IGameClient> _clientFactory;
    private CancellationTokenSource? _cts;
    private BotStatus _status = BotStatus.Idle;
    private int _loopCount;
    private int _errorCount;
    private int _restartCount;

    public AccountConfig Account { get; }
    public AppConfig Config { get; }

    public BotStatus Status
    {
        get => _status;
        private set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsActive));
                Account.RuntimeStatus = StatusText;
                if (value is BotStatus.Stopped or BotStatus.Error)
                    EndedAt = DateTime.Now;
            }
        }
    }

    public bool IsActive => Status is BotStatus.Connecting or BotStatus.Running or BotStatus.Stopping;

    public string StatusText => Status switch
    {
        BotStatus.Idle => "Chưa chạy",
        BotStatus.Connecting => "Đang kết nối",
        BotStatus.Running => "Đang chạy",
        BotStatus.Stopping => "Đang dừng",
        BotStatus.Stopped => "Đã dừng",
        BotStatus.Error => "Lỗi",
        _ => Status.ToString()
    };

    // ===== Thống kê =====
    public int LoopCount { get => _loopCount; private set => SetProperty(ref _loopCount, value); }
    public int ErrorCount { get => _errorCount; private set => SetProperty(ref _errorCount, value); }
    public int RestartCount { get => _restartCount; private set => SetProperty(ref _restartCount, value); }

    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public long StartPower { get; private set; }

    public long SmGained => Math.Max(0, Account.RuntimePower - StartPower);

    public TimeSpan Uptime => StartedAt is null
        ? TimeSpan.Zero
        : (EndedAt ?? DateTime.Now) - StartedAt.Value;

    public string UptimeText
    {
        get
        {
            var t = Uptime;
            return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }
    }

    public double SmPerHour
    {
        get
        {
            var hours = Uptime.TotalHours;
            return hours <= 0 ? 0 : SmGained / hours;
        }
    }

    /// <summary>Yêu cầu cập nhật các chỉ số phụ thuộc thời gian (gọi định kỳ từ UI).</summary>
    public void NotifyStats()
    {
        OnPropertyChanged(nameof(Uptime));
        OnPropertyChanged(nameof(UptimeText));
        OnPropertyChanged(nameof(SmGained));
        OnPropertyChanged(nameof(SmPerHour));
    }

    public event Action<BotRunner, string>? Log;

    public BotRunner(AccountConfig account, AppConfig config, Func<IGameClient> clientFactory)
    {
        Account = account;
        Config = config;
        _clientFactory = clientFactory;
        Account.RuntimeStatus = StatusText;
    }

    public void Start()
    {
        if (IsActive) return;
        _cts = new CancellationTokenSource();
        Status = BotStatus.Connecting;
        _ = Task.Run(() => RunAsync(_cts.Token));
    }

    public void Stop()
    {
        if (_cts is { IsCancellationRequested: false })
        {
            Emit("Yêu cầu dừng...");
            if (Status is BotStatus.Running or BotStatus.Connecting)
                Status = BotStatus.Stopping;
            _cts.Cancel();
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        int attempt = 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunSessionAsync(ct);
                break; // kết thúc bình thường (dừng hoặc đủ SM)
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ErrorCount++;
                Emit("Lỗi: " + ex.Message);

                var fn = Config.Function;
                if (!fn.AutoRestart || ct.IsCancellationRequested)
                {
                    Status = BotStatus.Error;
                    return;
                }

                attempt++;
                if (fn.MaxRestarts > 0 && attempt > fn.MaxRestarts)
                {
                    Emit($"Đã thử lại {fn.MaxRestarts} lần nhưng vẫn lỗi, dừng.");
                    Status = BotStatus.Error;
                    return;
                }

                RestartCount++;
                int delaySec = Math.Clamp(Math.Max(1, fn.RestartDelaySeconds) * attempt, 1, 300);
                Status = BotStatus.Connecting;
                Emit($"Tự khởi động lại sau {delaySec}s (lần thử {attempt})...");
                try { await Task.Delay(delaySec * 1000, ct); }
                catch (OperationCanceledException) { break; }
            }
        }

        Status = BotStatus.Stopped;
        Emit("Đã dừng.");
    }

    private async Task RunSessionAsync(CancellationToken ct)
    {
        using var client = _clientFactory();
        var ctx = new BotContext(Account, Config, Emit);

        Status = BotStatus.Connecting;
        await client.ConnectAsync(ctx, ct);
        await client.LoginAsync(ctx, ct);

        Account.RuntimePower = await client.GetPowerAsync(ct);
        MarkStarted(Account.RuntimePower);
        Status = BotStatus.Running;

        while (!ct.IsCancellationRequested)
        {
            if (!ScheduleHelper.IsWithin(Config.Function, DateTime.Now))
            {
                Emit("Ngoài khung giờ ON, tạm nghỉ 30s.");
                await Task.Delay(30_000, ct);
                continue;
            }

            if (Config.Trade.Enabled)
                await client.TradeAsync(ctx, ct);

            await client.AttackAsync(ctx, ct);
            await client.PickupAsync(ctx, ct);
            await client.FunctionsAsync(ctx, ct);

            Account.RuntimePower = await client.GetPowerAsync(ct);
            LoopCount++;

            if (Config.Function.ExitWhenEnoughSm && Config.Function.TargetSm > 0 &&
                Account.RuntimePower >= Config.Function.TargetSm)
            {
                Emit($"Đã đủ SM ({Account.RuntimePower:N0} >= {Config.Function.TargetSm:N0}). Thoát.");
                return;
            }

            await Task.Delay(Math.Max(300, Config.Function.DelayXmapMs), ct);
        }
    }

    private void MarkStarted(long power)
    {
        StartPower = power;
        StartedAt = DateTime.Now;
        EndedAt = null;
        NotifyStats();
    }

    private void Emit(string message) => Log?.Invoke(this, message);
}
