# Game Assistant Pro

Tool **Windows Desktop** (C# / .NET 8 / **WPF**, kiến trúc **MVVM**) cho công cụ hỗ trợ game.
Gồm **giao diện cấu hình đầy đủ**, **lưu/đọc cấu hình JSON (mật khẩu mã hóa DPAPI)** và
**khung engine đa luồng** chạy được end‑to‑end bằng client mô phỏng.

> ⚠️ WPF chỉ build & chạy trên **Windows**. Engine hiện dùng `SimulatedGameClient` (mô phỏng).
> Phần kết nối game thật là một lớp `IGameClient` do bạn ghép vào — repo **không** chứa giao thức server.

---

## 1. Yêu cầu
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) hoặc Visual Studio 2022 (workload *.NET Desktop Development*)

## 2. Build & chạy
```bat
dotnet build GameAssistantPro.sln -c Release
dotnet run --project GameAssistantPro/GameAssistantPro.csproj
```
Hoặc mở `GameAssistantPro.sln` bằng Visual Studio → F5.

## 2b. Đóng gói — giải nén chạy thẳng (không cần cài .NET)
Tạo bản **self-contained, single-file** (giống mẫu *Train Basic V32*: 1 file `.exe` + vài DLL `*_cor3.dll`,
giải nén là chạy ngay, máy đích **không cần cài .NET runtime**).

**Cách 1 — script (khuyên dùng):**
```bat
publish.bat            :: 64-bit (win-x64)
publish.bat win-x86    :: 32-bit
```
Kết quả:
- Thư mục chạy trực tiếp: `publish\GameAssistantPro\GameAssistantPro.exe`
- File nén để chia sẻ: `GameAssistantPro-win-x64.zip` → người khác **giải nén → bấm `.exe` là chạy**.

**Cách 2 — Visual Studio:** chuột phải project **GameAssistantPro → Publish** → chọn profile **win-x64**.

**Cách 3 — CLI thủ công:**
```bat
dotnet publish GameAssistantPro\GameAssistantPro.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish\GameAssistantPro
```
Ghi chú:
- File `config.json` sẽ nằm **cạnh `.exe`** khi chạy (mật khẩu đã mã hóa DPAPI).
- Bản self-contained khá nặng (~70–100 MB) vì gói sẵn runtime — đổi lại không cần cài .NET.
- Muốn đổi tên file `.exe`: sửa `<AssemblyName>` trong `GameAssistantPro.csproj`.

## 3. Cấu trúc dự án (MVVM)
```
GameAssistantPro/
├── App.xaml(.cs)            # Khởi động + theme tối, converters
├── Core/                    # Nền tảng: ObservableObject, RelayCommand, Converters, CryptoHelper (DPAPI)
├── Models/                  # AppConfig + Account/Launch/Trade/Train/Function (+ nested) — serialize JSON
├── Services/                # IConfigService, JsonConfigService (config.json)
├── Engine/                  # ⭐ Khung auto đa luồng
│   ├── IGameClient.cs       #   ĐIỂM CẮM giao thức game thật
│   ├── SimulatedGameClient.cs#  Client mô phỏng (log theo cấu hình, tăng SM giả lập)
│   ├── BotContext.cs        #   Account + Config + hàm log
│   ├── BotRunner.cs         #   1 luồng/tài khoản: connect → login → vòng lặp (trade/attack/pickup/functions)
│   ├── BotManager.cs        #   Quản lý nhiều runner, gom log, sự kiện AllStopped
│   ├── ScheduleHelper.cs    #   Khung giờ ON
│   └── BotStatus.cs
├── ViewModels/              # Main + per-tab VM
└── Views/                   # MainWindow + 5 tab (Tài khoản, Giao dịch, Train, Chức năng, Nhật ký)
```

## 4. Tính năng
- **4 tab cấu hình** đầy đủ theo spec: Tài khoản · Giao dịch · Train · Chức năng (vé vàng/NRJ, mua bùa…).
- **Tab Nhật ký:** bảng trạng thái từng tài khoản (Đang chạy/Đã dừng/Lỗi + SM) và **log realtime**.
- **Engine đa luồng:** mỗi tài khoản 1 luồng nền, dừng bằng `CancellationToken`, tôn trọng **giờ ON**,
  **thoát khi đủ SM**. **Chạy/Dừng theo từng tài khoản** (nút ▶/⏹ mỗi dòng) hoặc **tất cả**.
- **Ổn định & theo dõi:** **tự khởi động lại khi lỗi** (backoff theo số lần thử); **thống kê** mỗi tài khoản
  (SM/giờ, số vòng, số lỗi, thời gian chạy); **lọc log theo tài khoản** và **xuất log ra file**.
- **Lưu/đọc `config.json`**; **mật khẩu mã hóa DPAPI** (theo user Windows hiện tại), tương thích ngược file cũ.

## 5. Ghép game thật (việc còn lại)
Engine đang chạy với `SimulatedGameClient`. Để auto game thật:
1. Viết lớp `RealGameClient : IGameClient` — cài đặt `ConnectAsync/LoginAsync/Trade/Attack/Pickup/Functions/GetPower`
   bằng giao thức mạng của game (socket, packet…). Đọc cấu hình qua `ctx.Config`, báo tiến trình qua `ctx.Log(...)`.
2. Trong `ViewModels/MainViewModel.cs`, đổi factory:
   ```csharp
   _manager = new BotManager(() => new RealGameClient());
   ```
Không cần sửa UI hay model — toàn bộ binding/log/trạng thái dùng lại được ngay.

## 6. Giới hạn / lưu ý
- Repo **không** chứa và **không** reverse‑engineer giao thức server. Phần đó bạn tự cung cấp.
- Mật khẩu mã hóa bằng DPAPI **theo máy + user Windows** → copy `config.json` sang máy/người dùng khác sẽ cần nhập lại.
- Chưa build kiểm thử trên Windows trong môi trường tạo code (Linux) — hãy chạy `dotnet build` để xác nhận.

## 7. Hướng phát triển tiếp
- `RealGameClient` theo protocol thật; import/export cấu hình; lọc log theo tài khoản; thông báo Telegram/Discord.
