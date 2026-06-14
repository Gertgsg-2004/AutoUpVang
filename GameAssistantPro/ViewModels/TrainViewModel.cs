using System.Windows.Input;
using GameAssistantPro.Core;
using GameAssistantPro.Models;

namespace GameAssistantPro.ViewModels;

/// <summary>ViewModel cho tab Train (đánh quái, nhặt đồ, né người, cộng chỉ số).</summary>
public class TrainViewModel : ObservableObject
{
    private MonsterEntry? _selectedMonster;
    private IdEntry? _selectedSkill;
    private IdEntry? _selectedPickup;
    private IdEntry? _selectedDrop;
    private NameEntry? _selectedAvoidName;
    private IdEntry? _selectedAvoidZone;

    public TrainConfig Config { get; }

    public MonsterEntry? SelectedMonster { get => _selectedMonster; set => SetProperty(ref _selectedMonster, value); }
    public IdEntry? SelectedSkill { get => _selectedSkill; set => SetProperty(ref _selectedSkill, value); }
    public IdEntry? SelectedPickup { get => _selectedPickup; set => SetProperty(ref _selectedPickup, value); }
    public IdEntry? SelectedDrop { get => _selectedDrop; set => SetProperty(ref _selectedDrop, value); }
    public NameEntry? SelectedAvoidName { get => _selectedAvoidName; set => SetProperty(ref _selectedAvoidName, value); }
    public IdEntry? SelectedAvoidZone { get => _selectedAvoidZone; set => SetProperty(ref _selectedAvoidZone, value); }

    // Đánh quái
    public ICommand AddMonsterCommand { get; }
    public ICommand DeleteMonsterCommand { get; }
    public ICommand AddSkillCommand { get; }
    public ICommand DeleteSkillCommand { get; }

    // Nhặt / xử lý đồ
    public ICommand AddPickupCommand { get; }
    public ICommand DeletePickupCommand { get; }
    public ICommand AddDropCommand { get; }
    public ICommand DeleteDropCommand { get; }

    // Né người / boss
    public ICommand AddAvoidNameCommand { get; }
    public ICommand DeleteAvoidNameCommand { get; }
    public ICommand AddAvoidZoneCommand { get; }
    public ICommand DeleteAvoidZoneCommand { get; }

    public TrainViewModel(TrainConfig config)
    {
        Config = config;

        AddMonsterCommand = new RelayCommand(() => Config.Monsters.Add(new MonsterEntry()));
        DeleteMonsterCommand = new RelayCommand(
            () => { if (SelectedMonster != null) Config.Monsters.Remove(SelectedMonster); },
            () => SelectedMonster != null);

        AddSkillCommand = new RelayCommand(() => Config.Skills.Add(new IdEntry()));
        DeleteSkillCommand = new RelayCommand(
            () => { if (SelectedSkill != null) Config.Skills.Remove(SelectedSkill); },
            () => SelectedSkill != null);

        AddPickupCommand = new RelayCommand(() => Config.Pickup.Items.Add(new IdEntry()));
        DeletePickupCommand = new RelayCommand(
            () => { if (SelectedPickup != null) Config.Pickup.Items.Remove(SelectedPickup); },
            () => SelectedPickup != null);

        AddDropCommand = new RelayCommand(() => Config.Pickup.DropList.Add(new IdEntry()));
        DeleteDropCommand = new RelayCommand(
            () => { if (SelectedDrop != null) Config.Pickup.DropList.Remove(SelectedDrop); },
            () => SelectedDrop != null);

        AddAvoidNameCommand = new RelayCommand(() => Config.Avoid.Names.Add(new NameEntry()));
        DeleteAvoidNameCommand = new RelayCommand(
            () => { if (SelectedAvoidName != null) Config.Avoid.Names.Remove(SelectedAvoidName); },
            () => SelectedAvoidName != null);

        AddAvoidZoneCommand = new RelayCommand(() => Config.Avoid.Zones.Add(new IdEntry()));
        DeleteAvoidZoneCommand = new RelayCommand(
            () => { if (SelectedAvoidZone != null) Config.Avoid.Zones.Remove(SelectedAvoidZone); },
            () => SelectedAvoidZone != null);
    }
}
