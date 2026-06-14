using System;
using GameAssistantPro.Models;

namespace GameAssistantPro.Engine;

/// <summary>
/// Bối cảnh chạy cho một tài khoản: tài khoản, toàn bộ cấu hình, và hàm ghi log.
/// Được truyền vào IGameClient để client đọc cấu hình và báo log.
/// </summary>
public class BotContext
{
    public AccountConfig Account { get; }
    public AppConfig Config { get; }

    /// <summary>Ghi một dòng log (đã gắn với tài khoản tương ứng).</summary>
    public Action<string> Log { get; }

    public BotContext(AccountConfig account, AppConfig config, Action<string> log)
    {
        Account = account;
        Config = config;
        Log = log;
    }
}
