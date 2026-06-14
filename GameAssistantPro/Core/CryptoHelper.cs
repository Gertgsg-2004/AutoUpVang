using System;
using System.Security.Cryptography;
using System.Text;

namespace GameAssistantPro.Core;

/// <summary>
/// Mã hóa/giải mã chuỗi bằng DPAPI (Windows Data Protection, theo người dùng hiện tại).
/// Dùng để không lưu mật khẩu dạng plaintext trong config.json.
/// An toàn lỗi: nếu DPAPI không khả dụng hoặc dữ liệu là plaintext cũ thì trả về nguyên trạng.
/// </summary>
public static class CryptoHelper
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("GameAssistantPro::v1");

    /// <summary>Mã hóa plaintext -> base64 (DPAPI). Lỗi thì trả về chính plaintext.</summary>
    public static string Protect(string? plain)
    {
        if (string.IsNullOrEmpty(plain)) return "";
        try
        {
            var data = Encoding.UTF8.GetBytes(plain);
            var enc = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(enc);
        }
        catch
        {
            return plain;
        }
    }

    /// <summary>Giải mã base64(DPAPI) -> plaintext. Nếu không phải dữ liệu mã hóa thì coi như plaintext cũ.</summary>
    public static string Unprotect(string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return "";
        try
        {
            var enc = Convert.FromBase64String(stored);
            var data = ProtectedData.Unprotect(enc, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return stored; // tương thích ngược với config cũ lưu plaintext
        }
    }
}
