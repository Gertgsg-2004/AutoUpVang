using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>Quản lý các <see cref="BotRunner"/> — mỗi tài khoản một runner, start/stop độc lập.</summary>
public class BotManager
{
    private readonly Func<IGameClient> _clientFactory;
    private readonly Dictionary<AccountConfig, BotRunner> _byAccount = new();

    public ObservableCollection<BotRunner> Runners { get; } = new();

    /// <summary>Phát log từ bất kỳ runner nào.</summary>
    public event Action<BotRunner, string>? Log;

    /// <summary>Phát mỗi khi trạng thái một runner thay đổi (để UI cập nhật nút/cờ).</summary>
    public event Action? StateChanged;

    public bool AnyRunning => Runners.Any(r => r.IsActive);

    public BotManager(Func<IGameClient> clientFactory) => _clientFactory = clientFactory;

    /// <summary>Đang chạy tài khoản này?</summary>
    public bool IsActive(AccountConfig account)
        => _byAccount.TryGetValue(account, out var r) && r.IsActive;

    /// <summary>Chạy MỘT tài khoản (bỏ qua nếu đang chạy; thay runner cũ đã dừng).</summary>
    public void Start(AccountConfig account, AppConfig config)
    {
        if (_byAccount.TryGetValue(account, out var existing))
        {
            if (existing.IsActive) return;
            Detach(existing);
            Runners.Remove(existing);
            _byAccount.Remove(account);
        }

        var runner = new BotRunner(account, config, _clientFactory);
        runner.Log += OnRunnerLog;
        runner.PropertyChanged += OnRunnerPropertyChanged;
        _byAccount[account] = runner;
        Runners.Add(runner);
        runner.Start();
    }

    /// <summary>Dừng MỘT tài khoản.</summary>
    public void Stop(AccountConfig account)
    {
        if (_byAccount.TryGetValue(account, out var runner))
            runner.Stop();
    }

    /// <summary>Chạy tất cả tài khoản đang bật.</summary>
    public void StartAll(AppConfig config)
    {
        foreach (var account in config.Accounts.Where(a => a.Enabled))
            Start(account, config);
    }

    /// <summary>Dừng tất cả.</summary>
    public void StopAll()
    {
        foreach (var runner in Runners)
            runner.Stop();
    }

    private void OnRunnerLog(BotRunner runner, string message) => Log?.Invoke(runner, message);

    private void OnRunnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BotRunner.Status))
            StateChanged?.Invoke();
    }

    private void Detach(BotRunner runner)
    {
        runner.Log -= OnRunnerLog;
        runner.PropertyChanged -= OnRunnerPropertyChanged;
    }
}
