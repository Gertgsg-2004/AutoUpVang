namespace GameAssistantPro.Engine;

/// <summary>Trạng thái một luồng chạy tài khoản.</summary>
public enum BotStatus
{
    Idle,        // Chưa chạy
    Connecting,  // Đang kết nối
    Running,     // Đang chạy
    Stopping,    // Đang dừng
    Stopped,     // Đã dừng
    Error        // Lỗi
}
