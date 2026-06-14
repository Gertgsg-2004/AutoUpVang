using System;
using System.Globalization;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>Tiện ích kiểm tra khung giờ ON (Thời gian ON: HH:mm - HH:mm).</summary>
public static class ScheduleHelper
{
    /// <summary>True nếu thời điểm <paramref name="now"/> nằm trong khung giờ ON.</summary>
    public static bool IsWithin(FunctionConfig fn, DateTime now)
    {
        if (!TryParse(fn.OnTimeStart, out var start) || !TryParse(fn.OnTimeEnd, out var end))
            return true; // cấu hình giờ không hợp lệ -> coi như luôn ON

        var t = now.TimeOfDay;
        if (start <= end)
            return t >= start && t <= end;

        // Khung giờ vắt qua nửa đêm (vd 22:00 - 06:00)
        return t >= start || t <= end;
    }

    private static bool TryParse(string? value, out TimeSpan result)
        => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result);
}
