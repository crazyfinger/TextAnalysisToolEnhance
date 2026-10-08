# DECOMPLOG（反编译日志）

> 中文版 | [English](DECOMPLOG.md)

本次反编译的日志。**凡未写入此处的信息都会在下次上下文压缩时丢失。** 记录路径、地址、函数名、类型、文件格式、失败原因以及下一步。

## 状态
- 阶段：完成（初始还原）<!-- intake | prior art | recon | tools | plan | inventory | vertical slice | modules | assets | verify | done -->
- 完成标准：见 DECOMP_PLAN.md
- 下一步：将该新源码重新应用精炼过滤功能（补丁保存在 `%TEMP%\opencode\refine-feature.patch`），然后验证并提交。

## 事实（保持最新）
| 项目 | 值 | 来源 |
|---|---|---|
| 二进制 | data/TextAnalysisTool.NET.exe（217,088 字节） | ud scan |
| 程序集版本 | 1.0.9456.32322 (2025-11-21) | AssemblyName |
| 格式 / 架构 / 编译器 | PE32 x86, CLR v4.0.30319, .NET C# | ud scan |
| 路线 | managed-decompile (ilspycmd) | ud scan |
| 重建 exe | TextAnalysisTool.NET.exe（仓库根目录） | dotnet build |
| 启动截图哈希（重建版） | 8283B9CF13B4EF0262B93662ADBD2ADF98DF0203DBF5776C2C81DDD1AD191682 | ud run --shot |
| 启动截图哈希（原版） | 8283B9CF13B4EF0262B93662ADBD2ADF98DF0203DBF5776C2C81DDD1AD191682 | ud run --original --shot |

## 日志
### 2026-10-08（重做——新版本）
- 用户反馈之前反编译的 exe 是旧版；新版为 `D:\tools\TextAnalysisTool.NET\TextAnalysisTool.NET.exe`（Version 1.0.9456.32322，217,088 字节）。
- 完全删除旧反编译仓库；在 `D:\tools\TextAnalysisTool-decomp` 重新创建。
- 将新 exe 拷入 `data/`；`ud scan`：相同的 .NET/WinForms 路线，无保护。
- `ilspycmd -p` 顺利反编译。与旧版相比的新文件：`CTextAnalysisTool.cs`（主控制器现为 `CTextAnalysisTool`，不再是 `TextAnalysisTool`——故无命名空间/类型名冲突），另有 `BooleanDwordConverter.cs`、`CustomCheckbox.cs`、`PasteTextConfirmationDialog.cs`、`PasteTextOverwriteEnum.cs`、`StringColorConverter.cs`、`StringRectangleConverter.cs`、`XmlColor.cs`、`app.manifest`。
- 移入 `src/`；移除 CMake 脚手架。csproj 修复：`System.Resources.Extensions` 8.0.0 + `GenerateResourceUsePreserializedResources`（处理 .resx 非字符串资源）；`OutputPath` -> 仓库根目录。
- `dotnet build src/TextAnalysisTool.NET.csproj -c Release`：0 警告、0 错误，产物在仓库根目录。
- 验证像素保真：在共享注册表键 `HKCU:\Software\DavidAns\TextAnalysisTool.NET` 中强制固定窗口几何（Manual 1024x700），运行重建版与原版——两者 1002x644 截图**逐字节一致（SHA256 8283B9CF...）**。之后恢复注册表值。（之前窗口尺寸不同是 WindowsDefaultBounds/DPI 默认放置的产物，并非反编译缺陷。）
- 旧功能补丁保存在 `%TEMP%\opencode\refine-feature.patch` 以备重新应用。

## 下一步
- 重新应用精炼过滤功能（LineCollection.IntersectWith、FilterCollection.SetRefineFilterText + FilterLines AND、RefineFilterControl、MainWindow 布局/dock）到新源码。类现为 `CTextAnalysisTool`；核对相同的接缝（FilterCollection 构造函数接收 CTextAnalysisTool 等）。
- 重跑反射测试，重新构建，提交。

### 2026-10-08 — 功能：精炼过滤（重新应用到 v1.0.9456.32322）
- 在 `feature/refine-filter` 分支重新应用相同改动。所有接缝一致；仅主控制器类现为 `CTextAnalysisTool`（故处理器仍调用 `m_textAnalysisTool.FilterCollection.SetRefineFilterText(...)`，不变）。
- 新版本还把状态栏从 `StatusBar` 改为 `StatusStrip`——`Controls.AddRange` 和 `ShowOnlyFilteredLinesChanged` 的编辑用到了 `m_statusStrip`，对精炼功能无其他影响。
- 构建：0 警告/错误，产物在仓库根目录。
- 验证：反射测试对新构建全部通过（精炼 'beta'->2/6；alpha∩two->1 'alpha two'）。程序带精炼条正常启动，无崩溃。

### 2026-10-08 — 精炼过滤 UI 修正
- 修复条状控件两端"Refine:"标签和"Clear"按钮文字被截断（原为固定宽度，在高 DPI 下被裁）。标签/勾选框/按钮改为 AutoSize，并为面板添加 Padding。
- 新增两个勾选框："Match case"（大小写敏感）和"Regex"。
- `RefineFilterControl` 现暴露 `CaseSensitive`、`UseRegex`；事件由 `RefineTextChanged` 更名为 `CriteriaChanged`（文字变化或任一勾选框切换时触发）。
- `FilterCollection.SetRefineFilterText(text, caseSensitive, usesRegularExpression)` 将标志传入 `MatchesTextFilter`；`MainWindow` 处理器转发。
- 反射测试验证（大小写敏感 'Alpha'->0，忽略大小写 'Alpha'->2，正则 '\btwo\b'->2）。程序启动正常。

### 2026-10-09 — 精炼过滤性能
- 卡顿原因：每次按键都对整个源文件重跑一遍 accept（`new LineCollection(sourceLines, refine)`）外加并集扫描；且每键立即重滤。显示是虚拟化的，所以问题不在渲染。
- 修复 1：`LineCollection.RemoveLinesNotAcceptedBy(Filter)`——精炼现在只检查已在过滤结果中的行（经 `LineNumbersInCollection`），而非全文件。当 base 过滤器已把大文件收窄时收益巨大。`FilterCollection.FilterLines()` 用它代替构建单独的整源精炼集合 + `IntersectWith`。
- 修复 2：`RefineFilterControl` 用 250ms 的 `Timer` 对输入做防抖——`CriteriaChanged` 只在用户停顿后触发，而非每键。勾选框切换仍即时生效。
- 反射测试验证（含 `RemoveLinesNotAcceptedBy`：'alpha' 2 -> 1 'alpha two'）。程序正常启动。

### 2026-10-09 — 精炼过滤后台线程 + 焦点快捷键
- **后台精炼**：`FilterCollection.FilterLines()` 仍在 UI 线程计算 base 并集（快速的位操作），但精炼 accept 遍历现改在 `ThreadPool` 工作线程上运行。它在深拷贝（`new LineCollection(LineCollection)` 拷贝构造，BitArray 深拷贝）上操作，因此 UI 侧的 base 永不被改动。完成后 `CTextAnalysisTool.OnRefineComputed` 通过 `m_mainWindow.BeginInvoke` 回到 UI 线程，检查代数序号（丢弃更早按键产生的过期结果），再换入精炼集合并刷新显示与"Fil"计数。`SetRefineFilterText` 也会递增代数序号，因此清空/输入会使任何进行中的遍历失效。
- **快捷键**：新增 Edit 菜单项 "Focus Refine Box"（Ctrl+K）。在精炼 TextBox 上切换焦点：第一次按保存之前聚焦的控件并聚焦输入框；第二次按恢复其焦点（若已销毁则回落到行列表）。聚焦时菜单项显示勾选。
- 验证：反射测试全部通过，含拷贝构造独立性（克隆精炼到 1，原 base 仍为 2）。程序正常启动。（异步/线程与焦点切换由用户在运行时实测。）

### 2026-10-09 — 快捷键重排
- "Focus Refine Box" 快捷键由 Ctrl+K 改为 **Ctrl+E**。
- 新增 Edit > "Clear Refine Filter"，绑定 **Ctrl+K**：调用精炼框的 Clear（清除精炼过滤）。新增 `RefineFilterControl.ClearRefine()`；Clear 按钮也复用它。
- File > "Load Filters..." 现绑定 **Ctrl+L**。