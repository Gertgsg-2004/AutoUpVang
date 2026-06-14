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

    public bool IsRunning
    {
        get => _isRunning;
        private set { if (SetProperty(ref _isRunning, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand SaveConfigCommand { get; }
    public ICommand ReloadConfigCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ClearLogsCommand { get; }

    public MainViewModel() : this(new JsonConfigService()) { }

    public MainViewModel(IConfigService configService)
    {
        _configService = configService;

        // === ĐIỂM CẮM GAME THẬT ===
        // Đổi "() => new SimulatedGameClient()" thành factory tạo client thật của bạn
        // (lớp triển khai IGameClient có kết nối socket / gửi nhận packet).
        _manager = new BotManager(() => new SimulatedGameClient());
        _manager.Log += OnRunnerLog;
        _manager.AllStopped += OnAllStopped;

        SaveConfigCommand = new RelayCommand(SaveConfig);
        ReloadConfigCommand = new RelayCommand(LoadConfig, () => !IsRunning);
        StartCommand = new RelayCommand(Start, () => !IsRunning && _config.Accounts.Any(a => a.Enabled));
        StopCommand = new RelayCommand(Stop, () => IsRunning);
        ClearLogsCommand = new RelayCommand(() => Logs.Clear());

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

    private void Start()
    {
        Logs.Clear();
        _manager.StartAll(_config);
        IsRunning = _manager.AnyRunning;
        int count = Runners.Count(r => r.IsActive);
        StatusMessage = count > 0 ? $"Đang chạy {count} tài khoản..." : "Không có tài khoản nào được bật.";
    }

    private void Stop()
    {
        _manager.StopAll();
        StatusMessage = "Đã gửi yêu cầu dừng tất cả tài khoản.";
    }

    private void OnAllStopped()
        => Dispatch(() =>
        {
            IsRunning = false;
            StatusMessage = "Tất cả tài khoản đã dừng.";
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
