using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>Quản lý tất cả <see cref="BotRunner"/> (một runner cho mỗi tài khoản đang bật).</summary>
public class BotManager
{
    private readonly Func<IGameClient> _clientFactory;

    public ObservableCollection<BotRunner> Runners { get; } = new();

    /// <summary>Phát log từ bất kỳ runner nào.</summary>
    public event Action<BotRunner, string>? Log;

    /// <summary>Phát khi không còn runner nào hoạt động.</summary>
    public event Action? AllStopped;

    public bool AnyRunning => Runners.Any(r => r.IsActive);

    public BotManager(Func<IGameClient> clientFactory) => _clientFactory = clientFactory;

    /// <summary>Dừng các runner cũ rồi tạo + chạy runner cho mọi tài khoản đang bật.</summary>
    public void StartAll(AppConfig config)
    {
        StopAll();
        foreach (var runner in Runners.ToList())
            Detach(runner);
        Runners.Clear();

        foreach (var account in config.Accounts.Where(a => a.Enabled))
        {
            var runner = new BotRunner(account, config, _clientFactory);
            runner.Log += OnRunnerLog;
            runner.PropertyChanged += OnRunnerPropertyChanged;
            Runners.Add(runner);
            runner.Start();
        }
    }

    public void StopAll()
    {
        foreach (var runner in Runners)
            runner.Stop();
    }

    private void OnRunnerLog(BotRunner runner, string message) => Log?.Invoke(runner, message);

    private void OnRunnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BotRunner.Status) && !AnyRunning)
            AllStopped?.Invoke();
    }

    private void Detach(BotRunner runner)
    {
        runner.Log -= OnRunnerLog;
        runner.PropertyChanged -= OnRunnerPropertyChanged;
    }
}
