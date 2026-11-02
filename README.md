# DS Anim Studio（简体中文）

**中文 | [English](README.en.md)**

> 本仓库已完整汉化：源码、内嵌资源、默认停靠布局、构建产物都是简体中文。
> 程序内可在顶部菜单 **语言 → 简体中文 / English** 随时切换回英文，切换后立即生效并写入配置文件。

---

## 基本使用说明（散装游戏文件）

1. 运行程序。
2. 打开菜单 `文件` → `打开`。
3. 载入一个 .anibnd 或 .anibnd.dcx 文件。
4. 填写所需信息，例如游戏可执行文件、ModEngine 目录（如果适用）等。
5. 选择解包后的 ANIBND 要保存到哪里（即「工程目录」）。
6. 在左侧面板选择一条动画 ID。
7. 拖动事件、随便改点什么（右侧下方面板可以编辑高亮的事件）。
8. （可选）玩一下右上角的模型 / 动画查看器。
9. （可选）玩一下菜单栏里的各种选项。
10. 按 Ctrl+S 保存。
11. 确认你的游戏已配置成能加载改过的散装 ANIBND 文件。这属于 Souls Mod 的通用知识，情况太多，这里不展开。

## 基本使用说明（打包的游戏文件）

1. 运行程序。
2. 打开菜单 `文件` → `从打包的游戏数据归档中打开...`。
3. 填写所需信息，例如游戏可执行文件、ModEngine 目录（如果适用）等。
4. 选择你要使用的 ANIBND / CHRBND。
5. 选择解包后的 ANIBND 要保存到哪里（即「工程目录」）。
6. 在左侧面板选择一条动画 ID。
7. 拖动事件、随便改点什么（右侧下方面板可以编辑高亮的事件）。
8. （可选）玩一下右上角的模型 / 动画查看器。
9. （可选）玩一下菜单栏里的各种选项。
10. 按 Ctrl+S 保存。
11. 确认你的游戏已配置成能加载改过的散装 ANIBND 文件。这属于 Souls Mod 的通用知识，情况太多，这里不展开。

## 关于 DS Anim Studio

* 编辑黑暗之魂、血源诅咒、黑暗之魂 3、只狼的 TimeAct Editor 文件。这些文件控制着某条动画在**某一帧**上发生的**一切**，例如：
  * 开启无敌帧。
  * 弹反窗口。
  * 施加「SpEffect」（特殊临时状态，比如戒指效果、中毒、增益、AI 触发等）。
  * 允许动画取消。
  * 设置「YOU DIED」标志与重生。
  * 生成「SFX」/「FFX」（两者指同一类文件：视觉特效）。
  * 播放音效，例如脚步声、挥剑破空声等。
  * 触发攻击行为（对对手造成伤害、消耗玩家耐力等，全部在一个事件里完成）。
  * 触发「子弹」（投射物）行为（发射投射物、消耗玩家耐力等，全部在一个事件里完成）。
  * 触发「通用」行为（和攻击行为类似，但用于更简单的事，例如踩到别人头上造成硬直）。
  * 在挥武器时创建运动模糊。
  * 设置角色不透明度（用于被召唤到别人的世界、死亡等）。
  * 设置角色的攻击自动转向速度。
  * 播放「RumbleCam」文件（相对屏幕震动，例如斯摩的脚步让画面抖动）。
  * 播放额外的动画层（例如葛温的所有动画都带有让衣服随风飘动的动画层，叠加在其他动画之上）。
  * 调整模型渲染遮罩（显示 / 隐藏角色的特定部位）。
  * 以及很多我们甚至还没搞明白的东西。
* 以方块的形式，把事件画成一张真正的图形。
* 和原版文件一样，吸附到 30 fps 的整数倍。
* 如果已载入模板，右键即可给动画添加新事件。
* 高亮事件后按 Delete 键即可删除。
* 可以修改传给每个事件的参数（点选事件后，参数会出现在窗口右侧的面板里）。
* **完整的撤销 / 重做，Ctrl+Z / Ctrl+Y**。
* **完整的复制 / 粘贴**：
  * **Ctrl+C**：复制
  * **Ctrl+V**：粘贴到鼠标位置
  * **Ctrl+Shift+V**：原位粘贴（保留原本的开始 / 结束时间，适合在动画之间复制）
* 直接编辑游戏的 .anibnd 或 .anibnd.dcx 文件，不需要再用 BND 重打包工具。

## 游戏支持情况

| 游戏                                        | 编辑支持 | 已映射 / 已识别的事件 | 动画查看器支持 |
| ---                                         | ---      | ---                   | ---            |
| **黑暗之魂：受死版**                        | **支持** | 大量                  | **支持**       |
| **黑暗之魂：重制版**                        | **支持** | 大量                  | 勉强可用\*...  |
| **黑暗之魂 2**                              | 不支持   | 不支持                | 不支持（永远） |
| **黑暗之魂 2：原罪学者**                    | 不支持（未来可能会变） | 不支持   | 不支持（永远） |
| **黑暗之魂 3**                              | **支持** | 大量                  | **支持**       |
| **血源诅咒**                                | **支持** | 大量                  | **支持**       |
| **只狼：影逝二度**                          | **支持** | 大量                  | **支持**       |
| **艾尔登法环**                              | **支持** | 大量                  | **支持**       |

\* 对 DS1R / 只狼 / 艾尔登法环，已经不再需要做动画降级处理。

## 系统需求（最终用户）

* Windows 7 SP1 / 8.1 / 10 / 11（仅 64 位）
* [.NET Desktop Runtime 6.0 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.12-windows-x64-installer)
* **一块兼容 DirectX 11 的显卡**，哪怕你只是给 DS1: PTDE 做 Mod
  * 注意：虽然艾尔登法环用的是 DirectX 12，但 DS Anim Studio 依然使用 DirectX 11。

## 系统需求（开发者）

* Windows 7 SP1 / 8.1 / 10 / 11（仅 64 位）
* Visual Studio 2022
* [.NET 6.0 SDK (Windows x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.404-windows-x64-installer)
* **一块兼容 DirectX 11 的显卡**，哪怕你只是给 DS1: PTDE 做 Mod
  * 注意：虽然艾尔登法环用的是 DirectX 12，但 DS Anim Studio 依然使用 DirectX 11。

## 开发者说明

* 克隆仓库（使用 `git clone --recursive`）。
* 确认所有 NuGet 包已安装（理论上现在它会自动完成）。
* `生成解决方案` 应当成功。

### 本汉化版的构建方式

```powershell
# 需要 .NET 8 / 9 SDK；项目目标框架为 net9.0-windows7.0
git submodule update --init --recursive
dotnet build DSAnimStudioNETCore.sln -c Release -p:Platform=x64
# 产物：DSAnimStudioNETCore\bin\x64\Release\net9.0-windows7.0\DS ANIM STUDIO.exe
```

### 汉化结构

| 位置 | 作用 |
| --- | --- |
| `DSAnimStudioNETCore/Localization/Loc.cs` | 本地化核心：`Loc.T` / `Loc.TF` / `Loc.Win` / `Loc.StableKey`，语言状态、词条缓存、`##`/`###` 后缀保护 |
| `DSAnimStudioNETCore/EmbRes/Lang/zh-Hans.json` | 1303 条词条（英文原文 → 简体中文），编译进程序集 |
| `DSAnimStudioNETCore/EmbRes/Help.zh-Hans.txt` | 帮助窗口的中文文本 |
| `DSAnimStudioNETCore/EmbRes/Credits.zh-Hans.txt` | 启动画面滚动的中文版权信息 |
| `DSAnimStudioNETCore/EmbRes/imgui_default_V5.ini` / `imgui_default_Legacy.ini` | 中文默认停靠布局（窗口标题使用 `<中文>###<英文原名>`） |
| `DSAnimStudioNETCore/ImguiOSD/MenuBar.075.Language.cs` | 顶部「语言」菜单 |
| `DSAnimStudioNETCore/Localization/WinformsLoc.cs` | WinForms 控件树递归本地化（窗体标题、菜单、下拉框、表头、树节点） |
| `DSAnimStudio_Config.json` | 新增 `UILanguage` 字段（0 = 简体中文，1 = English），下次启动沿用 |

自定义词条：在程序目录下新建 `Lang\zh-Hans.json`（与内嵌资源同格式），即可覆盖任意条目，无需重新编译。

### 汉化范围

| 部分 | 处理方式 |
| --- | --- |
| ImGui 全部窗口 / 菜单 / 对话框 / 提示 / HUD | 全部经 `Loc.T` / `Loc.TF` 取词 |
| WinForms 12 个窗体、全部 `MessageBox` 标题与正文 | `WinformsLoc.Apply(this)` 递归处理 + `Loc.T` 包裹 |
| 内嵌 `Help.txt` / `Credits.txt` | 提供 `*.zh-Hans.txt` 中文版并按语言选择 |
| 默认停靠布局 `imgui_default_*.ini` | 用 `###` 锚点重生成，窗口标题中文化且切换语言不乱序 |
| 字体图集 | 启动时预烤 NotoSansMonoCJK + 全部译文用字 |
| 源码注释 | 全仓 C# 注释已翻译为简体中文 |
| 构建产物 | Release 编译即为中文界面，无额外步骤 |

英文模式的保持：`Language` 菜单自身保留 `English` 一项；`##`/`###` 内部 ID、`nameof()`、`DCX_*`、着色器 / 材质键名、`Ctrl+Z` 等快捷键提示、`%.2fpx` 等 ImGui 格式串、`Microsoft Sans Serif` 字体名、FMOD 官方错误码文本按原样保留。

## 特别感谢

* [TKGP](https://github.com/JKAnderson) —— 在 TAE 格式上有若干发现，并制作了本程序依赖的 SoulsFormats。
* [Pav](https://github.com/JohrnaJohrna) —— 为各款游戏映射了海量的 TAE 事件。
* [Katalash](https://github.com/katalash) —— 在理解动画文件方面给了大量帮助。
* [PredatorCZ](https://github.com/PredatorCZ) —— 完整逆向出了 Havok 样条压缩动画。
* [Horkrux](https://github.com/horkrux) —— 逆向出非 PC 平台纹理所用的头部与 swizzling。
* StaydMcButtermuffin —— 花了很多小时帮我写和调着色器，并逆向了一些黑暗之魂 3 的基础着色器来辅助这个过程。他还帮我在新版本程序中理解只狼与艾尔登法环的着色器。

## 使用的库

* [SoulsAssetPipeline](https://github.com/Meowmaritus/SoulsAssetPipeline)
* [SoulsFormatsNEXT](https://github.com/soulsmods/SoulsFormatsNEXT)
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* 一份修改过的 MonoGame Framework，我给它加了对更新纹理类型的支持。
* [HavokLib](https://github.com/PredatorCZ/HavokLib) 的一小部分，具体是样条压缩动画的解压器，已改写为 C#。
* [Horkrux 维护的、我那份 Wulf's BND Rebuilder 的 fork](https://github.com/horkrux/DeS-BNDBuild) 的一小部分，具体是 PS4 与 PS3 纹理的头部化与 deswizzling，已改写为 C# 并改为直接把纹理读进 MonoGame 而不是存成文件。
