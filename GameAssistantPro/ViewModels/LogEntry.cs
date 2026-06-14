using System;

namespace GameAssistantPro.ViewModels;

/// <summary>Một dòng nhật ký có cấu trúc (để lọc theo tài khoản và xuất file).</summary>
public class LogEntry
{
    public DateTime Time { get; init; }
    public string Account { get; init; } = "";
    public string Message { get; init; } = "";

    public string Display => $"[{Time:HH:mm:ss}] {Account}: {Message}";
}
