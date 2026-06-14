namespace GameAssistantPro.Models;

/// <summary>Cơ chế đánh dấu khi train.</summary>
public enum MarkMode
{
    UseTdlt,    // Use TDLT
    TanSat,     // Tàn sát
    KichHoatCo  // Kích hoạt cờ
}

/// <summary>Cách chọn quái để đánh.</summary>
public enum MonsterSelectMode
{
    AllAll,     // Tất cả
    ByPosition, // Theo vị trí
    ById        // Theo ID
}

/// <summary>Lọc loại quái.</summary>
public enum MonsterFilter
{
    All,        // Tất cả
    OnlyNormal, // Chỉ quái thường
    OnlySuper   // Chỉ siêu quái
}

/// <summary>Cơ chế nhặt đồ.</summary>
public enum PickupMode
{
    All,     // Tất cả
    ById,    // Theo ID
    ByType,  // Theo loại
    Exclude  // Loại trừ
}

/// <summary>Thời hạn bùa.</summary>
public enum BuffDuration
{
    OneHour,    // 1 giờ
    EightHours, // 8 giờ
    OneMonth    // 1 tháng
}

/// <summary>Chế độ mua bùa.</summary>
public enum BuffBuyMode
{
    KeepBuying, // Hết mua tiếp
    Once        // 1 lần
}
