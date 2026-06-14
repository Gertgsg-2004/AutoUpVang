using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameAssistantPro.Core;

/// <summary>
/// Lớp nền cho mọi model/viewmodel có hỗ trợ data binding (INotifyPropertyChanged).
/// Tự cài đặt để không phụ thuộc thư viện MVVM bên ngoài.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gán giá trị cho field và phát sự kiện đổi nếu khác giá trị cũ.</summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
