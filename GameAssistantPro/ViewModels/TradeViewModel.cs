using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel cho tab Giao dịch.</summary>
public class TradeViewModel : ObservableObject
{
    private TradeMapEntry? _selectedMapEntry;
    private TradeSet? _selectedSet;
    private int _setStarItemId;
    private int _setNormalItemId;

    public TradeConfig Config { get; }

    /// <summary>Danh sách tài khoản để chọn (dùng chung).</summary>
    public ObservableCollection<AccountConfig> Accounts { get; }

    public TradeMapEntry? SelectedMapEntry
    {
        get => _selectedMapEntry;
        set => SetProperty(ref _selectedMapEntry, value);
    }

    public TradeSet? SelectedSet
    {
        get => _selectedSet;
        set
        {
            if (SetProperty(ref _selectedSet, value) && value is not null)
            {
                // Đổ dữ liệu set đang chọn lên ô nhập để tiện cập nhật.
                SetStarItemId = value.StarItemId;
                SetNormalItemId = value.NormalItemId;
            }
        }
    }

    /// <summary>ID đồ sao (ô nhập theo Set).</summary>
    public int SetStarItemId { get => _setStarItemId; set => SetProperty(ref _setStarItemId, value); }

    /// <summary>ID đồ thường (ô nhập theo Set).</summary>
    public int SetNormalItemId { get => _setNormalItemId; set => SetProperty(ref _setNormalItemId, value); }

    public ICommand AddMapEntryCommand { get; }
    public ICommand DeleteMapEntryCommand { get; }
    public ICommand AddOrUpdateSetCommand { get; }
    public ICommand DeleteSetCommand { get; }

    public TradeViewModel(AppConfig config)
    {
        Config = config.Trade;
        Accounts = config.Accounts;

        AddMapEntryCommand = new RelayCommand(AddMapEntry);
        DeleteMapEntryCommand = new RelayCommand(DeleteMapEntry, () => SelectedMapEntry != null);
        AddOrUpdateSetCommand = new RelayCommand(AddOrUpdateSet);
        DeleteSetCommand = new RelayCommand(DeleteSet, () => SelectedSet != null);
    }

    private void AddMapEntry()
    {
        Config.MapEntries.Add(new TradeMapEntry());
        RenumberMapEntries();
    }

    private void DeleteMapEntry()
    {
        if (SelectedMapEntry is null) return;
        Config.MapEntries.Remove(SelectedMapEntry);
        RenumberMapEntries();
    }

    private void AddOrUpdateSet()
    {
        var existing = Config.Sets.FirstOrDefault(s => s.StarItemId == SetStarItemId);
        if (existing is not null)
        {
            existing.NormalItemId = SetNormalItemId;
        }
        else
        {
            Config.Sets.Add(new TradeSet
            {
                StarItemId = SetStarItemId,
                NormalItemId = SetNormalItemId
            });
            RenumberSets();
        }
    }

    private void DeleteSet()
    {
        if (SelectedSet is null) return;
        Config.Sets.Remove(SelectedSet);
        RenumberSets();
    }

    private void RenumberMapEntries()
    {
        for (int i = 0; i < Config.MapEntries.Count; i++)
            Config.MapEntries[i].Stt = i + 1;
    }

    private void RenumberSets()
    {
        for (int i = 0; i < Config.Sets.Count; i++)
            Config.Sets[i].Stt = i + 1;
    }
}
