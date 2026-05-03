using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HitokotoBetaExtension;

/// <summary>
/// Dock 栏上显示的一言条目（一个按钮，点击复制）
/// </summary>
internal sealed partial class HitokotoDockButton : InvokableCommand, INotifyPropChanged
{
    private string _text = "加载一言...";
    private string _subtitle = "点击刷新";

    public event TypedEventHandler<object, IPropChangedEventArgs>? PropChanged;

    public HitokotoDockButton()
    {
        Name = "一言语句";
        Icon = new IconInfo("\uE8D1"); // 引用图标
    }

    public string DisplayText
    {
        get => _text;
        private set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged(nameof(Name)); // Name 会显示在 Dock 上
                OnPropertyChanged(nameof(Subtitle));
            }
        }
    }

    public string Subtitle
    {
        get => _subtitle;
        private set
        {
            if (_subtitle != value)
            {
                _subtitle = value;
                OnPropertyChanged(nameof(Subtitle));
            }
        }
    }

    public override ICommandResult Invoke()
    {
        // 复制句子文本到剪贴板
        if (!string.IsNullOrEmpty(_text) && _text != "加载一言...")
        {
            ClipboardHelper.SetText(_text);
            // 可选：显示一个短暂的 Toast 提示
            var toast = new ToastStatusMessage($"已复制：{_text}");
            toast.Show();
        }
        return CommandResult.KeepOpen();
    }

    public void UpdateSentence(string hitokotoText)
    {
        // 截断过长的句子以适应 Dock 栏宽度（约 30 个字符）
        var display = hitokotoText.Length > 28 ? hitokotoText[..25] + "..." : hitokotoText;
        DisplayText = display;
        Subtitle = "点击复制";
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropChanged?.Invoke(this, new PropChangedEventArgs(propertyName));
    }
}