using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameAssistantPro.Models;

namespace GameAssistantPro.Services;

/// <summary>Lưu / đọc cấu hình dạng JSON bằng System.Text.Json (không cần thư viện ngoài).</summary>
public class JsonConfigService : IConfigService
{
    private readonly JsonSerializerOptions _options;

    public string ConfigPath { get; }

    public JsonConfigService(string? configPath = null)
    {
        ConfigPath = configPath ?? Path.Combine(AppContext.BaseDirectory, "config.json");
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
            return AppConfig.CreateDefault();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, _options) ?? AppConfig.CreateDefault();
            // Bổ sung các loại bùa chuẩn nếu file cũ còn thiếu.
            config.Function.Buff.EnsureDefaults();
            return config;
        }
        catch (Exception)
        {
            // File hỏng: sao lưu lại rồi dùng cấu hình mặc định để tránh treo app.
            TryBackupCorruptFile();
            return AppConfig.CreateDefault();
        }
    }

    public void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, _options);
        File.WriteAllText(ConfigPath, json);
    }

    private void TryBackupCorruptFile()
    {
        try
        {
            var backup = ConfigPath + $".bad-{DateTime.Now:yyyyMMdd-HHmmss}.json";
            File.Copy(ConfigPath, backup, overwrite: true);
        }
        catch
        {
            // Bỏ qua nếu không sao lưu được.
        }
    }
}
