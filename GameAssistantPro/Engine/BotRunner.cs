using System;
using System.Threading;
using System.Threading.Tasks;
using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>Quản lý vòng chạy của MỘT tài khoản trên một luồng nền riêng.</summary>
public class BotRunner : ObservableObject
{
    private readonly Func<IGameClient> _clientFactory;
    private CancellationTokenSource? _cts;
    private BotStatus _status = BotStatus.Idle;

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
        IGameClient? client = null;
        try
        {
            client = _clientFactory();
            var ctx = new BotContext(Account, Config, Emit);

            Status = BotStatus.Connecting;
            await client.ConnectAsync(ctx, ct);
            await client.LoginAsync(ctx, ct);
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

                if (Config.Function.ExitWhenEnoughSm && Config.Function.TargetSm > 0 &&
                    Account.RuntimePower >= Config.Function.TargetSm)
                {
                    Emit($"Đã đủ SM ({Account.RuntimePower:N0} >= {Config.Function.TargetSm:N0}). Thoát.");
                    break;
                }

                await Task.Delay(Math.Max(300, Config.Function.DelayXmapMs), ct);
            }

            Status = BotStatus.Stopped;
        }
        catch (OperationCanceledException)
        {
            Status = BotStatus.Stopped;
            Emit("Đã dừng.");
        }
        catch (Exception ex)
        {
            Status = BotStatus.Error;
            Emit("Lỗi: " + ex.Message);
        }
        finally
        {
            client?.Dispose();
        }
    }

    private void Emit(string message) => Log?.Invoke(this, message);
}
