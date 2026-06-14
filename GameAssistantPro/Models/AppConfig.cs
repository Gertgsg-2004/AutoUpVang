using System.Collections.ObjectModel;
using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Cấu hình gốc của toàn ứng dụng (được lưu ra file JSON).</summary>
public class AppConfig : ObservableObject
{
    /// <summary>Danh sách tài khoản.</summary>
    public ObservableCollection<AccountConfig> Accounts { get; } = new();

    /// <summary>Cài đặt mở game (kích thước, độ trễ).</summary>
    public LaunchSettings Launch { get; set; } = new();

    /// <summary>Cấu hình tab Giao dịch.</summary>
    public TradeConfig Trade { get; set; } = new();

    /// <summary>Cấu hình tab Train.</summary>
    public TrainConfig Train { get; set; } = new();

    /// <summary>Cấu hình tab Chức năng.</summary>
    public FunctionConfig Function { get; set; } = new();

    /// <summary>Tạo cấu hình mặc định (đã bổ sung sẵn danh sách bùa chuẩn).</summary>
    public static AppConfig CreateDefault()
    {
        var cfg = new AppConfig();
        cfg.Function.Buff.EnsureDefaults();
        return cfg;
    }
}
