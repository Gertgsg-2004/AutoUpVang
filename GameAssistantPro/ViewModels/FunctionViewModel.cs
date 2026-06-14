using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel cho tab Chức năng (chung, vé vàng/NRJ, mua bùa).</summary>
public class FunctionViewModel : ObservableObject
{
    public FunctionConfig Config { get; }

    /// <summary>Các map vào cho vé vàng (155 / 166).</summary>
    public int[] EnterMapOptions { get; } = { 155, 166 };

    public FunctionViewModel(FunctionConfig config)
    {
        Config = config;
        // Đảm bảo đủ danh sách bùa chuẩn để hiển thị.
        Config.Buff.EnsureDefaults();
    }
}
