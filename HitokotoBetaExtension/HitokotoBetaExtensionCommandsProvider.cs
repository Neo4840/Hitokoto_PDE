// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HitokotoBetaExtension;

public partial class HitokotoBetaExtensionCommandsProvider : CommandProvider, ICommandProvider3
{
    private readonly ICommandItem[] _commands;
    private readonly Settings _settings;
    private ListItem? _hitokotoDockItem;
    private ListItem? _refreshDockItem;

    public HitokotoBetaExtensionCommandsProvider()
    {
        DisplayName = "一言";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        _settings = new Settings();

        // ----- 1. 随机模式开关 -----
        _settings.Add(new ToggleSetting("randomMode", "随机模式", "开启后完全随机获取句子，忽略下方类型选择", true));

        // ----- 2. 句子类型多选（使用 ToggleSetting）-----
        var typeChoices = new (string Key, string Label, string ApiCode)[]
        {
            ("type_anime", "动画", "a"),
            ("type_comic", "漫画", "b"),
            ("type_game", "游戏", "c"),
            ("type_literature", "文学", "d"),
            ("type_original", "原创", "e"),
            ("type_web", "来自网络", "f"),
            ("type_other", "其他", "g"),
            ("type_movie", "影视", "h"),
            ("type_poetry", "诗词", "i"),
            ("type_163music", "网易云", "j"),
            ("type_philosophy", "哲学", "k"),
            ("type_joke", "抖机灵", "l"),
        };

        foreach (var t in typeChoices)
        {
            _settings.Add(new ToggleSetting(t.Key, t.Label, "", false)); // 默认全不选
        }

        // ----- 3. 短句开关（7字以内）-----
        _settings.Add(new ToggleSetting("shortOnly", "只获取7字以内的句子", "开启后只会返回长度 ≤7 的句子", false));

        // 挂载设置页面
        this.Settings = _settings;

        // 创建主页面（传入 settings）
        var page = new HitokotoBetaExtensionPage(_settings);
        _commands = [new CommandItem(page) { Title = DisplayName }];

        // 初始化 Dock 项
        var placeholderCommand = new CopyTextCommand("加载中...");
        _hitokotoDockItem = new ListItem(placeholderCommand)
        {
            Title = "一言：加载中...",
            Subtitle = "正在获取",
        };
        var refreshCommand = new RefreshDockCommand(this);
        _refreshDockItem = new ListItem(refreshCommand)
        {
            Title = "",
            Icon = new IconInfo("\uE72C")
        };

        SharedHitokotoModel.OnCurrentChanged += UpdateHitokotoDockItem;
    }

    // 根据当前设置构建 API 请求的 URL
    public static string BuildApiUrl(Settings settings)
    {
        var queryParams = new List<string>();

        // 随机模式
        bool randomMode = settings.GetSetting<bool>("randomMode");
        if (!randomMode)
        {
            // 收集启用的类型
            var typeKeys = new[]
            {
                "type_anime", "type_comic", "type_game", "type_literature",
                "type_original", "type_web", "type_other", "type_movie",
                "type_poetry", "type_163music", "type_philosophy", "type_joke"
            };
            var enabledTypes = typeKeys
                .Where(key => settings.GetSetting<bool>(key))
                .Select(key =>
                {
                    return key switch
                    {
                        "type_anime" => "a",
                        "type_comic" => "b",
                        "type_game" => "c",
                        "type_literature" => "d",
                        "type_original" => "e",
                        "type_web" => "f",
                        "type_other" => "g",
                        "type_movie" => "h",
                        "type_poetry" => "i",
                        "type_163music" => "j",
                        "type_philosophy" => "k",
                        "type_joke" => "l",
                        _ => ""
                    };
                })
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();

            // 如果用户关闭了随机模式但一个类型都没选，则默认增加“动画”类型
            if (enabledTypes.Count == 0)
            {
                enabledTypes.Add("a");
            }

            foreach (var t in enabledTypes)
            {
                queryParams.Add($"c={t}");
            }
        }

        // 短句模式
        bool shortOnly = settings.GetSetting<bool>("shortOnly");
        if (shortOnly)
        {
            queryParams.Add("max_length=7");
        }

        string url = "https://v1.hitokoto.cn";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }
        return url;
    }

    private void UpdateHitokotoDockItem()
    {
        var current = SharedHitokotoModel.Current;
        string displayText = current?.Hitokoto ?? "获取失败";
        if (displayText.Length > 30)
            displayText = displayText.Substring(0, 27) + "...";

        string subtitle = "";
        if (current != null && !string.IsNullOrEmpty(current.Hitokoto))
        {
            if (!string.IsNullOrEmpty(current.FromWho) && !string.IsNullOrEmpty(current.From))
                subtitle = $"—— {current.FromWho}《{current.From}》";
            else if (!string.IsNullOrEmpty(current.From))
                subtitle = $"——《{current.From}》";
            else if (!string.IsNullOrEmpty(current.FromWho))
                subtitle = $"—— {current.FromWho}";
            else
                subtitle = "出处未知";
        }
        else
        {
            subtitle = "获取失败，点击刷新";
        }

        var copyCommand = new CopyTextCommand(current?.Hitokoto ?? "");
        _hitokotoDockItem = new ListItem(copyCommand)
        {
            Title = displayText,
            Subtitle = subtitle,
        };
        RaiseItemsChanged();
    }

    public async Task RefreshDockHitokotoAsync()
    {
        string url = BuildApiUrl(_settings);
        var item = await HitokotoFetcher.FetchAsync(url);
        SharedHitokotoModel.Current = item;
    }

    public ICommandItem[] GetDockBands()
    {
        var band = new WrappedDockItem(
            new IListItem[] { _hitokotoDockItem, _refreshDockItem },
            "hitokoto_dock_band",
            "一言"
        );
        return new ICommandItem[] { band };
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    private sealed class RefreshDockCommand : InvokableCommand
    {
        private readonly HitokotoBetaExtensionCommandsProvider _provider;
        public RefreshDockCommand(HitokotoBetaExtensionCommandsProvider provider)
        {
            _provider = provider;
            Name = "";
        }
        public override ICommandResult Invoke()
        {
            _ = _provider.RefreshDockHitokotoAsync();
            return CommandResult.KeepOpen();
        }
    }
}