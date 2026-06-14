using GameAssistantPro.Models;

namespace GameAssistantPro.Services;

/// <summary>Dịch vụ lưu / đọc cấu hình ứng dụng.</summary>
public interface IConfigService
{
    /// <summary>Đường dẫn file cấu hình.</summary>
    string ConfigPath { get; }

    /// <summary>Đọc cấu hình (trả về mặc định nếu chưa có file).</summary>
    AppConfig Load();

    /// <summary>Lưu cấu hình ra file.</summary>
    void Save(AppConfig config);
}
