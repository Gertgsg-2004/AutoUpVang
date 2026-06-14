using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GameAssistantPro.Core;
using GameAssistantPro.Engine;
using GameAssistantPro.Models;
using GameAssistantPro.Services;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel chính: giữ AppConfig, các tab con, engine (BotManager) và nhật ký.</summary>
public class MainViewModel : ObservableObject
{
    private const int MaxLogLines = 500;

    private readonly IConfigService _configService;
    private readonly BotManager _manager;
    private AppConfig _config = AppConfig.CreateDefault();

    private AccountViewModel _account = null!;
    private TradeViewModel _trade = null!;
    private TrainViewModel _train = null!;
    private FunctionViewModel _function = null!;
    private string _statusMessage = "";
    private bool _isRunning;

    public AccountViewModel Account { get => _account; private set => SetProperty(ref _account, value); }
    public TradeViewModel Trade { get => _trade; private set => SetProperty(ref _trade, value); }
    public TrainViewModel Train { get => _train; private set => SetProperty(ref _train, value); }
    public FunctionViewModel Function { get => _function; private set => SetProperty(ref _function, value); }

    /// <summary>Danh sách runner đang chạy (cho tab Nhật ký).</summary>
    public ObservableCollection<BotRunner> Runners => _manager.Runners;

    /// <summary>Nhật ký hoạt động (mới nhất ở trên cùng).</summary>
    public ObservableCollection<string> Logs { get; } = new();

    public string ConfigPath => _configService.ConfigPath;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    /// <summary>Có ít nhất một tài khoản đang chạy.</summary>
    public bool IsRunning
    {
        get => _isRunning;
        private set { if (SetProperty(ref _isRunning, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    // Lệnh tổng
    public ICommand SaveConfigCommand { get; }
    public ICommand ReloadConfigCommand { get; }
    public ICommand StartAllCommand { get; }
    public ICommand StopAllCommand { get; }
    public ICommand ClearLogsCommand { get; }

    // Lệnh theo từng tài khoản
    public ICommand StartAccountCommand { get; }
    public ICommand StopAccountCommand { get; }

    public MainViewModel() : this(new JsonConfigService()) { }

    public MainViewModel(IConfigService configService)
    {
        _configService = configService;

        // === ĐIỂM CẮM GAME THẬT ===
        // Đổi "() => new SimulatedGameClient()" thành factory tạo client thật của bạn.
        _manager = new BotManager(() => new SimulatedGameClient());
        _manager.Log += OnRunnerLog;
        _manager.StateChanged += OnEngineStateChanged;

        SaveConfigCommand = new RelayCommand(SaveConfig);
        ReloadConfigCommand = new RelayCommand(LoadConfig, () => !IsRunning);
        StartAllCommand = new RelayCommand(StartAll,
            () => _config.Accounts.Any(a => a.Enabled && !_manager.IsActive(a)));
        StopAllCommand = new RelayCommand(StopAll, () => _manager.AnyRunning);
        ClearLogsCommand = new RelayCommand(() => Logs.Clear());

        StartAccountCommand = new RelayCommand<AccountConfig>(StartAccount, CanStartAccount);
        StopAccountCommand = new RelayCommand<AccountConfig>(StopAccount, CanStopAccount);

        LoadConfig();
    }

    private void LoadConfig()
    {
        _config = _configService.Load();
        Account = new AccountViewModel(_config);
        Trade = new TradeViewModel(_config);
        Train = new TrainViewModel(_config.Train);
        Function = new FunctionViewModel(_config.Function);
        StatusMessage = $"Đã tải cấu hình: {_configService.ConfigPath}";
    }

    private void SaveConfig()
    {
        try
        {
            _configService.Save(_config);
            StatusMessage = $"Đã lưu cấu hình lúc {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi lưu cấu hình: " + ex.Message;
        }
    }

    private void StartAll()
    {
        _manager.StartAll(_config);
        StatusMessage = $"Đang chạy {Runners.Count(r => r.IsActive)} tài khoản...";
    }

    private void StopAll()
    {
        _manager.StopAll();
        StatusMessage = "Đã gửi yêu cầu dừng tất cả tài khoản.";
    }

    private void StartAccount(AccountConfig? account)
    {
        if (account is null) return;
        _manager.Start(account, _config);
        StatusMessage = $"Đang chạy '{account.DisplayName}'...";
    }

    private void StopAccount(AccountConfig? account)
    {
        if (account is null) return;
        _manager.Stop(account);
        StatusMessage = $"Đã yêu cầu dừng '{account.DisplayName}'.";
    }

    private bool CanStartAccount(AccountConfig? account)
        => account is not null && !_manager.IsActive(account);

    private bool CanStopAccount(AccountConfig? account)
        => account is not null && _manager.IsActive(account);

    private void OnEngineStateChanged()
        => Dispatch(() =>
        {
            IsRunning = _manager.AnyRunning;
            CommandManager.InvalidateRequerySuggested();
        });

    private void OnRunnerLog(BotRunner runner, string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {runner.Account.DisplayName}: {message}";
        Dispatch(() =>
        {
            Logs.Insert(0, line);
            while (Logs.Count > MaxLogLines)
                Logs.RemoveAt(Logs.Count - 1);
        });
    }

    /// <summary>Chạy action trên luồng UI (vì engine chạy ở luồng nền).</summary>
    private static void Dispatch(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }
}
