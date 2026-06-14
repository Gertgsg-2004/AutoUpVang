using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameAssistantPro.Core;
using GameAssistantPro.Engine;
using GameAssistantPro.Models;
using GameAssistantPro.Services;
using Microsoft.Win32;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel chính: AppConfig, các tab con, engine (BotManager), nhật ký + thống kê.</summary>
public class MainViewModel : ObservableObject
{
    private const int MaxLogLines = 500;
    private const string AllLabel = "Tất cả";

    private readonly IConfigService _configService;
    private readonly BotManager _manager;
    private readonly DispatcherTimer _statsTimer;
    private AppConfig _config = AppConfig.CreateDefault();

    private AccountViewModel _account = null!;
    private TradeViewModel _trade = null!;
    private TrainViewModel _train = null!;
    private FunctionViewModel _function = null!;
    private string _statusMessage = "";
    private string _accountFilter = AllLabel;
    private bool _isRunning;

    public AccountViewModel Account { get => _account; private set => SetProperty(ref _account, value); }
    public TradeViewModel Trade { get => _trade; private set => SetProperty(ref _trade, value); }
    public TrainViewModel Train { get => _train; private set => SetProperty(ref _train, value); }
    public FunctionViewModel Function { get => _function; private set => SetProperty(ref _function, value); }

    /// <summary>Danh sách runner (cho bảng trạng thái + thống kê).</summary>
    public ObservableCollection<BotRunner> Runners => _manager.Runners;

    /// <summary>Nhật ký (mới nhất ở trên cùng).</summary>
    public ObservableCollection<LogEntry> Logs { get; } = new();

    /// <summary>View có lọc của nhật ký (theo tài khoản).</summary>
    public ICollectionView LogsView { get; }

    /// <summary>Danh sách lựa chọn lọc log ("Tất cả" + tên tài khoản).</summary>
    public ObservableCollection<string> AccountFilters { get; } = new();

    public string AccountFilter
    {
        get => _accountFilter;
        set { if (SetProperty(ref _accountFilter, value)) LogsView.Refresh(); }
    }

    public string ConfigPath => _configService.ConfigPath;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public bool IsRunning
    {
        get => _isRunning;
        private set { if (SetProperty(ref _isRunning, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand SaveConfigCommand { get; }
    public ICommand ReloadConfigCommand { get; }
    public ICommand StartAllCommand { get; }
    public ICommand StopAllCommand { get; }
    public ICommand ClearLogsCommand { get; }
    public ICommand ExportLogsCommand { get; }
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

        LogsView = CollectionViewSource.GetDefaultView(Logs);
        LogsView.Filter = o => _accountFilter == AllLabel || (o as LogEntry)?.Account == _accountFilter;

        SaveConfigCommand = new RelayCommand(SaveConfig);
        ReloadConfigCommand = new RelayCommand(LoadConfig, () => !IsRunning);
        StartAllCommand = new RelayCommand(StartAll,
            () => _config.Accounts.Any(a => a.Enabled && !_manager.IsActive(a)));
        StopAllCommand = new RelayCommand(StopAll, () => _manager.AnyRunning);
        ClearLogsCommand = new RelayCommand(() => Logs.Clear());
        ExportLogsCommand = new RelayCommand(ExportLogs, () => Logs.Count > 0);
        StartAccountCommand = new RelayCommand<AccountConfig>(StartAccount, CanStartAccount);
        StopAccountCommand = new RelayCommand<AccountConfig>(StopAccount, CanStopAccount);

        _statsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _statsTimer.Tick += (_, _) => { foreach (var r in Runners) r.NotifyStats(); };
        _statsTimer.Start();

        LoadConfig();
    }

    private void LoadConfig()
    {
        _config = _configService.Load();
        Account = new AccountViewModel(_config);
        Trade = new TradeViewModel(_config);
        Train = new TrainViewModel(_config.Train);
        Function = new FunctionViewModel(_config.Function);
        RefreshAccountFilters();
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
        RefreshAccountFilters();
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
        RefreshAccountFilters();
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

    private void ExportLogs()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = $"gap-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var lines = LogsView.Cast<LogEntry>().Select(e => e.Display);
            File.WriteAllLines(dialog.FileName, lines);
            StatusMessage = "Đã xuất log: " + dialog.FileName;
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi xuất log: " + ex.Message;
        }
    }

    private void RefreshAccountFilters()
    {
        AccountFilters.Clear();
        AccountFilters.Add(AllLabel);
        foreach (var name in _config.Accounts
                     .Select(a => a.DisplayName)
                     .Where(n => !string.IsNullOrWhiteSpace(n))
                     .Distinct())
        {
            AccountFilters.Add(name);
        }
        if (!AccountFilters.Contains(_accountFilter))
            AccountFilter = AllLabel;
    }

    private void OnEngineStateChanged()
        => Dispatch(() =>
        {
            IsRunning = _manager.AnyRunning;
            CommandManager.InvalidateRequerySuggested();
        });

    private void OnRunnerLog(BotRunner runner, string message)
    {
        var entry = new LogEntry
        {
            Time = DateTime.Now,
            Account = runner.Account.DisplayName,
            Message = message
        };
        Dispatch(() =>
        {
            Logs.Insert(0, entry);
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
