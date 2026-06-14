using System.Text.Json.Serialization;
using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Cấu hình một tài khoản đăng nhập.</summary>
public class AccountConfig : ObservableObject
{
    private bool _enabled = true;
    private string _username = "";
    private string _password = "";
    private string _server = "";
    private bool _useProxy;
    private string _proxyHost = "";
    private int _proxyPort;
    private string _proxyUser = "";
    private string _proxyPass = "";
    private string _runtimeStatus = "Chưa chạy";
    private long _runtimePower;

    /// <summary>Bật/tắt tài khoản này trong danh sách chạy.</summary>
    public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

    /// <summary>Tên người dùng.</summary>
    public string Username
    {
        get => _username;
        set { if (SetProperty(ref _username, value)) OnPropertyChanged(nameof(DisplayName)); }
    }

    /// <summary>Mật khẩu (plaintext trong bộ nhớ, dùng cho UI/engine). Không lưu trực tiếp ra file.</summary>
    [JsonIgnore]
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    /// <summary>
    /// Trường thực sự được ghi ra JSON (khóa "Password") — đã mã hóa DPAPI.
    /// Giải mã khi đọc; tương thích ngược nếu file cũ lưu plaintext.
    /// </summary>
    [JsonPropertyName("Password")]
    public string PasswordProtected
    {
        get => CryptoHelper.Protect(_password);
        set => _password = CryptoHelper.Unprotect(value);
    }

    /// <summary>Server đăng nhập.</summary>
    public string Server { get => _server; set => SetProperty(ref _server, value); }

    /// <summary>Dùng máy chủ ủy quyền (proxy) cho tài khoản này.</summary>
    public bool UseProxy { get => _useProxy; set => SetProperty(ref _useProxy, value); }

    public string ProxyHost { get => _proxyHost; set => SetProperty(ref _proxyHost, value); }
    public int ProxyPort { get => _proxyPort; set => SetProperty(ref _proxyPort, value); }
    public string ProxyUser { get => _proxyUser; set => SetProperty(ref _proxyUser, value); }
    public string ProxyPass { get => _proxyPass; set => SetProperty(ref _proxyPass, value); }

    /// <summary>Tên hiển thị trong danh sách (không lưu vào file).</summary>
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Username) ? "(tài khoản mới)" : Username;

    /// <summary>Trạng thái chạy hiện tại (engine cập nhật, không lưu file).</summary>
    [JsonIgnore]
    public string RuntimeStatus { get => _runtimeStatus; set => SetProperty(ref _runtimeStatus, value); }

    /// <summary>SM hiện tại do engine cập nhật (không lưu file).</summary>
    [JsonIgnore]
    public long RuntimePower { get => _runtimePower; set => SetProperty(ref _runtimePower, value); }
}
