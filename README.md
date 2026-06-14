# Game Assistant Pro

Tool **Windows Desktop** (C# / .NET 8 / **WPF**, kiến trúc **MVVM**) làm bảng cấu hình
cho công cụ hỗ trợ game. Bản hiện tại (milestone 1) tập trung vào **toàn bộ giao diện**
và **lưu / đọc cấu hình JSON** — chưa nối engine auto vào game.

> ⚠️ WPF chỉ build & chạy được trên **Windows**. Không build được trên Linux/macOS.

---

## 1. Yêu cầu

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (hoặc Visual Studio 2022 17.8+ với workload *.NET Desktop Development*)

## 2. Build & chạy

**Bằng dòng lệnh:**

```bat
dotnet build GameAssistantPro.sln -c Release
dotnet run --project GameAssistantPro/GameAssistantPro.csproj
```

**Bằng Visual Studio:** mở `GameAssistantPro.sln`, đặt `GameAssistantPro` làm startup project, nhấn **F5**.

## 3. Cấu trúc dự án (MVVM)

```
GameAssistantPro/
├── App.xaml(.cs)            # Khởi động + theme tối, converters dùng chung
├── Core/                    # Lớp nền không phụ thuộc thư viện ngoài
│   ├── ObservableObject.cs  # INotifyPropertyChanged (SetProperty)
│   ├── RelayCommand.cs      # ICommand cho MVVM
│   └── Converters.cs        # EnumBoolean / InverseBoolean / BoolToVisibility
├── Models/                  # Toàn bộ cấu hình (serialize JSON)
│   ├── AppConfig.cs         # Gốc: Accounts + Launch + Trade + Train + Function
│   ├── AccountConfig.cs     # Tài khoản (user/pass/server/proxy)
│   ├── LaunchSettings.cs    # Kích thước + độ trễ mở game
│   ├── TradeConfig.cs       # Tab Giao dịch (+ TradeMapEntry, TradeSet)
│   ├── TrainConfig.cs       # Tab Train (+ Monster/Pickup/Avoid/AddStats)
│   ├── FunctionConfig.cs    # Tab Chức năng (+ GoldTicket, Buff)
│   ├── Common.cs / Enums.cs # IdEntry, NameEntry + các enum
├── Services/
│   ├── IConfigService.cs
│   └── JsonConfigService.cs # Lưu/đọc config.json (tự sao lưu file hỏng)
├── ViewModels/              # MainViewModel + VM cho từng tab
└── Views/                   # MainWindow + AccountView/TradeView/TrainView/FunctionView
```

Luồng dữ liệu: `Views` (XAML) ⇄ binding ⇄ `ViewModels` ⇄ `Models` (`AppConfig`) ⇄ `JsonConfigService` ⇄ `config.json`.

## 4. Tính năng đã có

- **Tab Tài khoản:** danh sách tài khoản (thêm/sửa/xóa), user/pass/server, máy chủ ủy quyền (proxy),
  ô captcha, kích thước & độ trễ mở game, nút **Save config**.
- **Tab Giao dịch:** kích hoạt bản đồ giao dịch, chọn tài khoản/Map/Khu, bảng `STT · ID map · ID normal`,
  nhập theo **Set** (ID đồ sao / ID đồ thường) + thêm/cập nhật/xóa set.
- **Tab Train:** đánh quái (Map/Khu, cơ chế đánh dấu, loại quái, lọc quái, giới hạn HP %, FPS, d/s quái, d/s kỹ năng);
  nhặt/xử lý đồ (cơ chế nhặt, d/s nhặt, d/s vứt, Auto Vứt/Default); né người/boss (d/s tên, d/s khu); cộng chỉ số (HP/MP/SD).
- **Tab Chức năng:** chức năng chung (thoát khi đủ SM, về nhà KI, các delay, Use Item/GLT/Default, mua khẩu trang/cỏ 4 lá,
  tách-hợp nhất/đi theo, auto xin đậu, thời gian ON…); vé vàng/NRJ (map 155/166, E10, mua khi vàng ≥ ngưỡng);
  mua bùa (thời hạn 1h/8h/1 tháng, chế độ, bảng 9 loại bùa + số lượng).
- **Lưu/đọc cấu hình:** `config.json` cạnh file thực thi; nút **Lưu cấu hình** / **Tải lại** trên thanh công cụ.

## 5. Giới hạn của bản này

- **Chưa có engine auto** kết nối game — nút *Bắt đầu/Dừng* hiện chỉ là demo UI. Phần logic/giao thức game sẽ
  được ghép ở milestone sau (kiến trúc đã tách `Services`/VM để dễ cắm vào).
- **Mật khẩu lưu dạng plaintext** trong `config.json` (tool nội bộ). Cân nhắc mã hóa (DPAPI) nếu cần.

## 6. Hướng phát triển tiếp

- Định nghĩa `IBotEngine` + runner đa luồng cho từng tài khoản.
- Mã hóa mật khẩu, import/export cấu hình, log realtime, hiển thị trạng thái từng tài khoản.
