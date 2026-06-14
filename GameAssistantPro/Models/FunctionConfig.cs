using System.Collections.ObjectModel;
using System.Linq;
using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Cấu hình vé vàng / NRJ.</summary>
public class GoldTicketConfig : ObservableObject
{
    private bool _enabled;
    private int _enterMap = 155;
    private bool _energyE10;
    private bool _buyGoldTicket;
    private bool _goldBarNrj;
    private long _buyWhenGoldGte = 200_000_000;

    /// <summary>Bật vé vàng.</summary>
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

    /// <summary>Map vào (155 / 166).</summary>
    public int EnterMap { get => _enterMap; set => SetProperty(ref _enterMap, value); }

    /// <summary>Năng lượng E10.</summary>
    public bool EnergyE10 { get => _energyE10; set => SetProperty(ref _energyE10, value); }

    /// <summary>Mua vé vàng.</summary>
    public bool BuyGoldTicket { get => _buyGoldTicket; set => SetProperty(ref _buyGoldTicket, value); }

    /// <summary>Thỏi vàng NRJ.</summary>
    public bool GoldBarNrj { get => _goldBarNrj; set => SetProperty(ref _goldBarNrj, value); }

    /// <summary>Mua khi vàng &gt;= ngưỡng (mặc định 200.000.000).</summary>
    public long BuyWhenGoldGte { get => _buyWhenGoldGte; set => SetProperty(ref _buyWhenGoldGte, value); }
}

/// <summary>Một loại bùa.</summary>
public class BuffItem : ObservableObject
{
    private string _name = "";
    private bool _enabled;
    private int _quantity = 1;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
    public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    public BuffItem() { }
    public BuffItem(string name) => _name = name;
}

/// <summary>Cấu hình kích hoạt mua bùa.</summary>
public class BuffConfig : ObservableObject
{
    private bool _enabled;
    private BuffDuration _duration = BuffDuration.OneHour;
    private BuffBuyMode _buyMode = BuffBuyMode.KeepBuying;

    /// <summary>Danh sách các loại bùa chuẩn.</summary>
    public static readonly string[] StandardBuffNames =
    {
        "Trí tuệ", "Mạnh mạnh", "Da trâu", "Oai hùng",
        "Bất tử", "Dẻo dai", "Thu hút", "TT x3", "TT x4"
    };

    /// <summary>Kích hoạt mua bùa.</summary>
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

    /// <summary>Thời hạn: 1 giờ / 8 giờ / 1 tháng.</summary>
    public BuffDuration Duration { get => _duration; set => SetProperty(ref _duration, value); }

    /// <summary>Chế độ: Hết mua tiếp / 1 lần.</summary>
    public BuffBuyMode BuyMode { get => _buyMode; set => SetProperty(ref _buyMode, value); }

    /// <summary>Danh sách bùa kèm số lượng.</summary>
    public ObservableCollection<BuffItem> Buffs { get; } = new();

    /// <summary>Bổ sung các loại bùa chuẩn còn thiếu (gọi sau khi tải cấu hình).</summary>
    public void EnsureDefaults()
    {
        foreach (var name in StandardBuffNames)
        {
            if (!Buffs.Any(b => b.Name == name))
                Buffs.Add(new BuffItem(name));
        }
    }
}

/// <summary>Cấu hình tab Chức năng.</summary>
public class FunctionConfig : ObservableObject
{
    private bool _exitWhenEnoughSm;
    private long _targetSm;
    private bool _drawKiHouse;
    private int _kiHousePercent = 50;
    private int _delayXmapMs = 500;
    private int _deathReturnMs = 3000;
    private int _returnAfterMs = 1000;
    private bool _useItem;
    private bool _useGlt;
    private bool _useDefault;
    private bool _buyMask;
    private int _maskQty;
    private bool _buyClover;
    private int _cloverQty;
    private bool _splitMerge;
    private bool _follow;
    private string _followTarget = "";
    private bool _autoAskBeans;
    private bool _buyGoldIngot;
    private string _onTimeStart = "00:00";
    private string _onTimeEnd = "23:59";
    private bool _autoRestart;
    private int _restartDelaySeconds = 10;
    private int _maxRestarts;

    /// <summary>Thoát khi đủ SM (sức mạnh).</summary>
    public bool ExitWhenEnoughSm { get => _exitWhenEnoughSm; set => SetProperty(ref _exitWhenEnoughSm, value); }
    public long TargetSm { get => _targetSm; set => SetProperty(ref _targetSm, value); }

    /// <summary>Về nhà KI khi &lt; (%).</summary>
    public bool DrawKiHouse { get => _drawKiHouse; set => SetProperty(ref _drawKiHouse, value); }
    public int KiHousePercent { get => _kiHousePercent; set => SetProperty(ref _kiHousePercent, value); }

    /// <summary>Delay khi Xmap (ms).</summary>
    public int DelayXmapMs { get => _delayXmapMs; set => SetProperty(ref _delayXmapMs, value); }

    /// <summary>Chết về sau (ms).</summary>
    public int DeathReturnMs { get => _deathReturnMs; set => SetProperty(ref _deathReturnMs, value); }

    /// <summary>Quay lại sau (ms).</summary>
    public int ReturnAfterMs { get => _returnAfterMs; set => SetProperty(ref _returnAfterMs, value); }

    public bool UseItem { get => _useItem; set => SetProperty(ref _useItem, value); }
    public bool UseGlt { get => _useGlt; set => SetProperty(ref _useGlt, value); }
    public bool UseDefault { get => _useDefault; set => SetProperty(ref _useDefault, value); }

    /// <summary>Mua khẩu trang + số lượng.</summary>
    public bool BuyMask { get => _buyMask; set => SetProperty(ref _buyMask, value); }
    public int MaskQty { get => _maskQty; set => SetProperty(ref _maskQty, value); }

    /// <summary>Mua cỏ 4 lá + số lượng.</summary>
    public bool BuyClover { get => _buyClover; set => SetProperty(ref _buyClover, value); }
    public int CloverQty { get => _cloverQty; set => SetProperty(ref _cloverQty, value); }

    /// <summary>Tách / hợp nhất.</summary>
    public bool SplitMerge { get => _splitMerge; set => SetProperty(ref _splitMerge, value); }

    /// <summary>Chế độ đi theo.</summary>
    public bool Follow { get => _follow; set => SetProperty(ref _follow, value); }
    public string FollowTarget { get => _followTarget; set => SetProperty(ref _followTarget, value); }

    /// <summary>Auto xin đậu.</summary>
    public bool AutoAskBeans { get => _autoAskBeans; set => SetProperty(ref _autoAskBeans, value); }

    /// <summary>Mua thỏi vàng.</summary>
    public bool BuyGoldIngot { get => _buyGoldIngot; set => SetProperty(ref _buyGoldIngot, value); }

    /// <summary>Thời gian ON (giờ bắt đầu), định dạng HH:mm.</summary>
    public string OnTimeStart { get => _onTimeStart; set => SetProperty(ref _onTimeStart, value); }

    /// <summary>Thời gian ON (giờ kết thúc), định dạng HH:mm.</summary>
    public string OnTimeEnd { get => _onTimeEnd; set => SetProperty(ref _onTimeEnd, value); }

    /// <summary>Tự khởi động lại khi gặp lỗi / rớt mạng.</summary>
    public bool AutoRestart { get => _autoRestart; set => SetProperty(ref _autoRestart, value); }

    /// <summary>Thời gian chờ cơ sở giữa các lần thử lại (giây). Backoff = giây × lần thử.</summary>
    public int RestartDelaySeconds { get => _restartDelaySeconds; set => SetProperty(ref _restartDelaySeconds, value); }

    /// <summary>Số lần thử lại tối đa (0 = không giới hạn).</summary>
    public int MaxRestarts { get => _maxRestarts; set => SetProperty(ref _maxRestarts, value); }

    /// <summary>Cấu hình vé vàng / NRJ.</summary>
    public GoldTicketConfig GoldTicket { get; set; } = new();

    /// <summary>Cấu hình mua bùa.</summary>
    public BuffConfig Buff { get; set; } = new();
}
