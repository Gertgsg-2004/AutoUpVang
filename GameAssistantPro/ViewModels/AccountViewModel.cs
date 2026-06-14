using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel cho tab Tài khoản (cấu hình tài khoản + cài đặt mở game).</summary>
public class AccountViewModel : ObservableObject
{
    private AccountConfig? _selectedAccount;
    private string _captcha = "";

    /// <summary>Danh sách tài khoản (dùng chung với AppConfig).</summary>
    public ObservableCollection<AccountConfig> Accounts { get; }

    /// <summary>Cài đặt mở game (kích thước, độ trễ).</summary>
    public LaunchSettings Launch { get; }

    public AccountConfig? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    /// <summary>Captcha nhập lúc đăng nhập (không lưu xuống file).</summary>
    public string Captcha
    {
        get => _captcha;
        set => SetProperty(ref _captcha, value);
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    public AccountViewModel(AppConfig config)
    {
        Accounts = config.Accounts;
        Launch = config.Launch;

        AddCommand = new RelayCommand(Add);
        DeleteCommand = new RelayCommand(Delete, () => SelectedAccount != null);

        SelectedAccount = Accounts.FirstOrDefault();
    }

    private void Add()
    {
        var account = new AccountConfig();
        Accounts.Add(account);
        SelectedAccount = account;
    }

    private void Delete()
    {
        if (SelectedAccount is null)
            return;

        int index = Accounts.IndexOf(SelectedAccount);
        Accounts.Remove(SelectedAccount);
        SelectedAccount = Accounts.Count > 0
            ? Accounts[Math.Min(index, Accounts.Count - 1)]
            : null;
    }
}
