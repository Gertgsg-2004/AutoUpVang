using GameAssistantPro.Core;

namespace GameAssistantPro.Models;

/// <summary>Phần tử ID đơn giản (dùng cho danh sách kỹ năng, danh sách vứt, danh sách khu...).</summary>
public class IdEntry : ObservableObject
{
    private int _id;
    public int Id { get => _id; set => SetProperty(ref _id, value); }

    public IdEntry() { }
    public IdEntry(int id) => _id = id;
}

/// <summary>Phần tử tên đơn giản (dùng cho danh sách tên người/boss cần né).</summary>
public class NameEntry : ObservableObject
{
    private string _name = "";
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public NameEntry() { }
    public NameEntry(string name) => _name = name;
}
