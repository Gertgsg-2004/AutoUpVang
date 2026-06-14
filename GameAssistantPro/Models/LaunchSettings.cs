using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Cài đặt khi mở trò chơi (kích thước cửa sổ game, độ trễ).</summary>
public class LaunchSettings : ObservableObject
{
    private int _gameWidth = 480;
    private int _gameHeight = 800;
    private int _openGameDelayMs = 3000;

    /// <summary>Chiều rộng cửa sổ game.</summary>
    public int GameWidth { get => _gameWidth; set => SetProperty(ref _gameWidth, value); }

    /// <summary>Chiều cao cửa sổ game.</summary>
    public int GameHeight { get => _gameHeight; set => SetProperty(ref _gameHeight, value); }

    /// <summary>Độ trễ (ms) khi mở trò chơi.</summary>
    public int OpenGameDelayMs { get => _openGameDelayMs; set => SetProperty(ref _openGameDelayMs, value); }
}
