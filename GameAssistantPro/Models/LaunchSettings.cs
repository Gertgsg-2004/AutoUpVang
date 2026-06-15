using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Cài đặt khi mở game (kích thước, độ trễ) và kết nối server.</summary>
public class LaunchSettings : ObservableObject
{
    private int _gameWidth = 480;
    private int _gameHeight = 800;
    private int _openGameDelayMs = 3000;
    private bool _useRealClient;
    private string _serverHost = "";
    private int _serverPort;
    private string _clientVersion = "2.3.0";

    /// <summary>Chiều rộng cửa sổ game.</summary>
    public int GameWidth { get => _gameWidth; set => SetProperty(ref _gameWidth, value); }

    /// <summary>Chiều cao cửa sổ game.</summary>
    public int GameHeight { get => _gameHeight; set => SetProperty(ref _gameHeight, value); }

    /// <summary>Độ trễ (ms) khi mở trò chơi.</summary>
    public int OpenGameDelayMs { get => _openGameDelayMs; set => SetProperty(ref _openGameDelayMs, value); }

    /// <summary>Dùng RealGameClient (kết nối server thật) thay vì client mô phỏng.</summary>
    public bool UseRealClient { get => _useRealClient; set => SetProperty(ref _useRealClient, value); }

    /// <summary>Địa chỉ server game (IP hoặc domain).</summary>
    public string ServerHost { get => _serverHost; set => SetProperty(ref _serverHost, value); }

    /// <summary>Cổng server game.</summary>
    public int ServerPort { get => _serverPort; set => SetProperty(ref _serverPort, value); }

    /// <summary>Phiên bản client gửi khi đăng nhập (vd 2.3.0).</summary>
    public string ClientVersion { get => _clientVersion; set => SetProperty(ref _clientVersion, value); }
}
