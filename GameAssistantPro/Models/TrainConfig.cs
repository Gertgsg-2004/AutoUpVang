using System.Collections.ObjectModel;
using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Một quái trong danh sách đánh.</summary>
public class MonsterEntry : ObservableObject
{
    private bool _enabled = true;
    private int _id;
    private string _name = "";
    private int _hpLimitPercent;

    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
    public int Id { get => _id; set => SetProperty(ref _id, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>Giới hạn HP % riêng cho quái này (0 = dùng giới hạn chung).</summary>
    public int HpLimitPercent { get => _hpLimitPercent; set => SetProperty(ref _hpLimitPercent, value); }
}

/// <summary>Cấu hình nhặt / xử lý đồ.</summary>
public class PickupConfig : ObservableObject
{
    private PickupMode _mode = PickupMode.All;
    private bool _autoDrop;
    private bool _autoDefault;

    /// <summary>Cơ chế nhặt: Tất cả / Theo ID / Theo loại / Loại trừ.</summary>
    public PickupMode Mode { get => _mode; set => SetProperty(ref _mode, value); }

    /// <summary>Danh sách ID/loại đồ áp dụng cho cơ chế nhặt.</summary>
    public ObservableCollection<IdEntry> Items { get; } = new();

    /// <summary>Danh sách đồ cần vứt.</summary>
    public ObservableCollection<IdEntry> DropList { get; } = new();

    /// <summary>Tự động vứt.</summary>
    public bool AutoDrop { get => _autoDrop; set => SetProperty(ref _autoDrop, value); }

    /// <summary>Tự động Default.</summary>
    public bool AutoDefault { get => _autoDefault; set => SetProperty(ref _autoDefault, value); }
}

/// <summary>Cấu hình né người / boss.</summary>
public class AvoidConfig : ObservableObject
{
    private bool _enabled;

    /// <summary>Kích hoạt né.</summary>
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

    /// <summary>Danh sách tên người/boss cần né.</summary>
    public ObservableCollection<NameEntry> Names { get; } = new();

    /// <summary>Danh sách khu cần né.</summary>
    public ObservableCollection<IdEntry> Zones { get; } = new();
}

/// <summary>Cấu hình cộng chỉ số.</summary>
public class AddStatsConfig : ObservableObject
{
    private bool _enabled;
    private int _hp;
    private int _mp;
    private int _sd;

    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
    public int Hp { get => _hp; set => SetProperty(ref _hp, value); }
    public int Mp { get => _mp; set => SetProperty(ref _mp, value); }

    /// <summary>SD = Sức đánh.</summary>
    public int Sd { get => _sd; set => SetProperty(ref _sd, value); }
}

/// <summary>Cấu hình tab Train.</summary>
public class TrainConfig : ObservableObject
{
    private int _map;
    private int _zone;
    private MarkMode _markMode = MarkMode.UseTdlt;
    private MonsterSelectMode _selectMode = MonsterSelectMode.AllAll;
    private int _hpLimitPercent = 100;
    private MonsterFilter _filter = MonsterFilter.All;
    private bool _showFps;

    /// <summary>Bản đồ train.</summary>
    public int Map { get => _map; set => SetProperty(ref _map, value); }

    /// <summary>Khu train.</summary>
    public int Zone { get => _zone; set => SetProperty(ref _zone, value); }

    /// <summary>Cơ chế đánh dấu: Use TDLT / Tàn sát / Kích hoạt cờ.</summary>
    public MarkMode MarkMode { get => _markMode; set => SetProperty(ref _markMode, value); }

    /// <summary>Loại quái: Tất cả / Theo vị trí / Theo ID.</summary>
    public MonsterSelectMode SelectMode { get => _selectMode; set => SetProperty(ref _selectMode, value); }

    /// <summary>Giới hạn HP % chung khi đánh.</summary>
    public int HpLimitPercent { get => _hpLimitPercent; set => SetProperty(ref _hpLimitPercent, value); }

    /// <summary>Lọc quái: Tất cả / Chỉ quái thường / Chỉ siêu quái.</summary>
    public MonsterFilter Filter { get => _filter; set => SetProperty(ref _filter, value); }

    /// <summary>Hiển thị FPS.</summary>
    public bool ShowFps { get => _showFps; set => SetProperty(ref _showFps, value); }

    /// <summary>Danh sách quái.</summary>
    public ObservableCollection<MonsterEntry> Monsters { get; } = new();

    /// <summary>Danh sách ID kỹ năng.</summary>
    public ObservableCollection<IdEntry> Skills { get; } = new();

    /// <summary>Cấu hình nhặt / xử lý đồ.</summary>
    public PickupConfig Pickup { get; set; } = new();

    /// <summary>Cấu hình né người / boss.</summary>
    public AvoidConfig Avoid { get; set; } = new();

    /// <summary>Cấu hình cộng chỉ số.</summary>
    public AddStatsConfig AddStats { get; set; } = new();
}
