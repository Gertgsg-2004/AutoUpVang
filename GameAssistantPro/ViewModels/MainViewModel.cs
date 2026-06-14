using System;
using System.Windows.Input;
using GameAssistantPro.Core;
using GameAssistantPro.Models;
using GameAssistantPro.Services;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel chính: giữ AppConfig, tạo các tab con, xử lý lưu/đọc cấu hình.</summary>
public class MainViewModel : ObservableObject
{
    private readonly IConfigService _configService;
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

    public string ConfigPath => _configService.ConfigPath;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsRunning { get => _isRunning; private set => SetProperty(ref _isRunning, value); }

    public ICommand SaveConfigCommand { get; }
    public ICommand ReloadConfigCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public MainViewModel() : this(new JsonConfigService()) { }

    public MainViewModel(IConfigService configService)
    {
        _configService = configService;

        SaveConfigCommand = new RelayCommand(SaveConfig);
        ReloadConfigCommand = new RelayCommand(LoadConfig);
        StartCommand = new RelayCommand(Start, () => !IsRunning);
        StopCommand = new RelayCommand(Stop, () => IsRunning);

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
        IsRunning = true;
        StatusMessage = "Demo UI: engine auto chưa được tích hợp ở bản này (sẽ làm ở milestone sau).";
    }

    private void Stop()
    {
        IsRunning = false;
        StatusMessage = "Đã dừng.";
    }
}
