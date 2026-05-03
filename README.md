# 一言插件 for PowerToys 命令面板

在 PowerToys 命令面板中快速获取、复制一言（Hitokoto）句子，支持 Dock 栏常驻显示与刷新。

![PowerToys 设置界面截图](/settings.png)
*PowerToys 设置界面截图*

![Dock 实装截图](/dockpreview.png)
*Dock 栏实装截图*

## 功能

- 🔍 命令面板中一键获取随机一言句子
- ⚙️ 设置页面支持：
  - 随机模式 / 多类型筛选（动画、文学、哲学等）
  - 仅获取 7 字以内短句
- 🖱️ Dock 栏显示当前句子（点击复制），右侧刷新按钮
- 🔗 可选跳转 Hitokoto 查看完整信息

## 安装

1. 确保已安装 [PowerToys](https://github.com/microsoft/PowerToys)（≥0.90 版本）
2. Release页面下载发布包（`.msixbundle` 或 `.exe` 安装程序）
3. 运行安装程序，按提示完成安装
4. 打开 PowerToys 命令面板（默认 `Win + Alt + 空格`），输入 `reload` 刷新扩展列表

## 使用

- 命令面板中输入 `一言` 打开主界面，点击 `换一句` 刷新
- 修改设置：`命令面板设置` → `扩展` → `已安装` → `一言` → 齿轮图标
- Dock 栏：启用 `命令面板设置` → `扩展坞` → 确保“一言”带已开启

## 项目结构

- `HitokotoBetaExtension.cs` – 扩展入口
- `HitokotoBetaExtensionCommandsProvider.cs` – 命令提供者 + Dock 支持
- `HitokotoBetaExtensionPage.cs` – 主列表页面
- `HitokotoFetcher.cs` – HTTP 请求与 JSON 解析
- `SharedHitokotoModel.cs` – Page 与 Dock 共享数据

## 许可证

[AGPL-3.0](LICENSE)

## 致谢

- [Hitokoto 一言 API](https://hitokoto.cn)
- [PowerToys 命令面板扩展 SDK](https://learn.microsoft.com/windows/powertoys/command-palette)
