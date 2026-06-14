using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GameAssistantPro;

public partial class App : Application
{
    public App()
    {
        // Bắt mọi lỗi không xử lý để hiện popup + ghi file thay vì tắt im lặng.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Report(e.Exception, "UI");
        e.Handled = true; // không cho tắt ngay để người dùng đọc được lỗi
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        => Report(e.ExceptionObject as Exception, "AppDomain");

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Report(e.Exception, "Task");
        e.SetObserved();
    }

    private static void Report(Exception? ex, string source)
    {
        if (ex is null) return;

        var detail = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({source})\n{ex}\n\n";
        try
        {
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "error.log"), detail, Encoding.UTF8);
        }
        catch
        {
            // bỏ qua nếu không ghi được file
        }

        try
        {
            MessageBox.Show(
                ex.Message + "\n\n(Chi tiết đã ghi vào file error.log cạnh GameAssistantPro.exe)",
                "Game Assistant Pro - Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            // bỏ qua nếu không hiện được hộp thoại
        }
    }
}
