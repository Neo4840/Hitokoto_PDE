// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HitokotoBetaExtension;

internal sealed partial class HitokotoBetaExtensionPage : ListPage
{
    private readonly Settings _settings;
    private HitokotoItem? _currentHitokoto;
    private bool _isLoading = true;
    private string? _errorMessage;

    public HitokotoBetaExtensionPage(Settings settings)
    {
        _settings = settings;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "一言";
        Name = "Open";

        _settings.SettingsChanged += OnSettingsChanged;
        _ = LoadHitokotoAsync();
    }

    private void OnSettingsChanged(object? sender, Settings e)
    {
        _ = RefreshAsync();
    }

    private async Task LoadHitokotoAsync()
    {
        IsLoading = true;
        _isLoading = true;
        _errorMessage = null;
        RaiseItemsChanged();

        string url = HitokotoBetaExtensionCommandsProvider.BuildApiUrl(_settings);
        _currentHitokoto = await HitokotoFetcher.FetchAsync(url);

        if (_currentHitokoto == null || string.IsNullOrEmpty(_currentHitokoto.Hitokoto))
        {
            _errorMessage = "获取一言失败，请检查网络后重试";
        }
        else
        {
            SharedHitokotoModel.Current = _currentHitokoto;
        }

        IsLoading = false;
        _isLoading = false;
        RaiseItemsChanged();
    }

    public async Task RefreshAsync()
    {
        await LoadHitokotoAsync();
    }

    public override IListItem[] GetItems()
    {
        if (_isLoading)
        {
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = "加载中...",
                    Subtitle = "正在从一言 API 获取句子"
                }
            };
        }

        if (_errorMessage != null)
        {
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _errorMessage,
                    Subtitle = "请检查网络连接后点击下方按钮重试"
                },
                new ListItem(new RefreshCommand(this))
                {
                    Title = "🔄 重试",
                    Subtitle = "再次获取一言"
                }
            };
        }

        if (_currentHitokoto == null)
        {
            return new IListItem[]
            {
                new ListItem(new RefreshCommand(this))
                {
                    Title = "点击获取一言",
                    Subtitle = "获取一句名言或句子"
                }
            };
        }

        var subtitle = string.IsNullOrEmpty(_currentHitokoto.FromWho) ? _currentHitokoto.From : $"—— {_currentHitokoto.FromWho}《{_currentHitokoto.From}》";
        if (string.IsNullOrEmpty(_currentHitokoto.From) && string.IsNullOrEmpty(_currentHitokoto.FromWho))
        {
            subtitle = "出处未知";
        }

        var items = new System.Collections.Generic.List<ListItem>
        {
            new ListItem(new NoOpCommand())
            {
                Title = _currentHitokoto.Hitokoto,
                Subtitle = subtitle
            },
            new ListItem(new RefreshCommand(this))
            {
                Title = "🔄 换一句",
                Subtitle = "获取新的一言"
            }
        };

        if (!string.IsNullOrEmpty(_currentHitokoto.Uuid))
        {
            var url = $"https://hitokoto.cn?uuid={_currentHitokoto.Uuid}";
            items.Add(new ListItem(new OpenUrlCommand(url))
            {
                Title = "🔗 在 Hitokoto 官网查看详情",
                Subtitle = url
            });
        }

        return items.ToArray();
    }

    private sealed class RefreshCommand : InvokableCommand
    {
        private readonly HitokotoBetaExtensionPage _page;
        public RefreshCommand(HitokotoBetaExtensionPage page)
        {
            _page = page;
            Name = "刷新一言";
            Icon = new IconInfo("\uE72C");
        }
        public override ICommandResult Invoke()
        {
            _page.RefreshAsync().GetAwaiter().GetResult();
            return CommandResult.KeepOpen();
        }
    }
}