using System.Collections.ObjectModel;
using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Một dòng map giao dịch: STT, ID map, ID normal.</summary>
public class TradeMapEntry : ObservableObject
{
    private int _stt;
    private int _mapId;
    private int _normalId;

    public int Stt { get => _stt; set => SetProperty(ref _stt, value); }
    public int MapId { get => _mapId; set => SetProperty(ref _mapId, value); }
    public int NormalId { get => _normalId; set => SetProperty(ref _normalId, value); }
}

/// <summary>Một set đồ giao dịch: ID đồ sao / ID đồ thường.</summary>
public class TradeSet : ObservableObject
{
    private int _stt;
    private int _starItemId;
    private int _normalItemId;

    public int Stt { get => _stt; set => SetProperty(ref _stt, value); }
    public int StarItemId { get => _starItemId; set => SetProperty(ref _starItemId, value); }
    public int NormalItemId { get => _normalItemId; set => SetProperty(ref _normalItemId, value); }
}

/// <summary>Cấu hình tab Giao dịch (chuyển đồ set/map/khu dùng chung).</summary>
public class TradeConfig : ObservableObject
{
    private bool _enabled;
    private string _accountName = "";
    private int _map;
    private int _zone;

    /// <summary>Kích hoạt bản đồ giao dịch.</summary>
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

    /// <summary>Tài khoản được chọn để giao dịch.</summary>
    public string AccountName { get => _accountName; set => SetProperty(ref _accountName, value); }

    /// <summary>Map giao dịch.</summary>
    public int Map { get => _map; set => SetProperty(ref _map, value); }

    /// <summary>Khu giao dịch.</summary>
    public int Zone { get => _zone; set => SetProperty(ref _zone, value); }

    /// <summary>Bảng map giao dịch (STT, ID map, ID normal).</summary>
    public ObservableCollection<TradeMapEntry> MapEntries { get; } = new();

    /// <summary>Danh sách set đồ giao dịch.</summary>
    public ObservableCollection<TradeSet> Sets { get; } = new();
}
