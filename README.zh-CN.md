# TextAnalysisToolEnhance

> 中文版 | [English](README.md)

**TextAnalysisToolEnhance** 修改并演进自经典开源工具 **TextAnalysisTool.NET**（by David L. A. Anson）。全过程在先进 **AI 编程助手（Gemini CLI）** 的深度协助与协同工程下完成。在完整保留原版经典的正则/字符串过滤、标记书签（Marker）、颜色渲染等轻量、零延迟全部原生特性的基础上，**特别针对 Android 系统日志与大型分布式系统日志分析**进行了全方位的现代化重构与功能飞跃。

---

## 为什么特别适用于分析 Android 日志？

在 Android 手机系统开发、Framework 调试以及车载/IoT 嵌入式排查中，工程师经常需要面对庞大的 **Bugreport / Bug2Go 压缩包**或多个离散的缓冲区（`main.log`, `system.log`, `events.log`, `crash.log`, `kernel.log` 等）：
- **压缩包免解压直读**：无需先花时间手动解压几十兆甚至上百兆的压缩包，直接将 `.zip` / `.7z` / `.gz` 拖入程序即可解析内部日志成员。
- **多源日志时间戳对齐合并**：各个缓冲区的日志原本彼此分离且时间错开，本工具自动识别微秒级时间戳，一键将多个日志按发生绝对时间严格升序交织归并，并自动加上 `[main.log]`、`[system.log]` 等来源标签，让你在单一视图串行复盘整个事件演进。
- **Android 专属快选与 24 色高对比度色彩体系**：右键任意 logcat 行自动提取进程 PID 与组件 Tag；内置精选的 24 色高对比度鲜艳调色板（支持哈希一致性映射，确保同一 Tag 在整个分析流程中色彩始终固定且清晰醒目，不同 Tag 缤纷区分）。
- **即时精炼过滤 (Refine)**：面对庞大的过滤结果，按 `Ctrl+E` 输入关键字即刻做二次 AND 即时过滤，按 `Ctrl+K` 瞬间还原，无需反复调整底层复杂的规则集。
- **多选行一键区间排除**：拖选多行，右键一键隐藏故障区间外的刷屏行或死循环行（`Hide Lines {min} to {max}`）。

---

## 增强版新增特性一览

相较于官方原版 TextAnalysisTool.NET，本增强版新增了以下核心能力：

1. **精炼过滤（Refine Filter）**：在已有过滤结果之上叠加第二层实时即时关键字过滤（AND 逻辑），支持回车/即时匹配与全局快捷键。
2. **独立标记书签窗口（Marked Lines Display）**：新增独立面板，全局汇总展示被打上 Marker（1~8）的关键行，支持双向同步与双击跳转。
3. **多源日志合并排序与压缩包直读（Open & Merge / Archive Support）**：
   - 原生支持 `.zip`, `.7z`, `.gz` 压缩包解构，无需解压即可直接查看内部文本。
   - 自动识别多源日志中的时间戳，一键将多个日志按时间严格升序穿插合并，并添加来源标签（Source Tag）。
4. **增强型行号过滤器与多选区间隐藏**：
   - 列表多选行时，右键一键生成 `Hide Lines {min} to {max}` 区间排除过滤器。
   - 支持双击行号过滤器直接编辑起始行号（Start line）与终止行号（End line），支持仅前隐藏、仅后隐藏与区间过滤；Excluding 语义严格对齐全局规则；Description 随行号与 Excluding 实时联动。
5. **AndroidLog 插件与自定义颜色系统（`TATPlugin_AndroidLog`）**：
   - 右键快速识别日志行并生成按 PID 或 Tag 过滤的规则。
   - 支持自定义颜色：内置 24 色高对比度鲜艳调色板（支持一致性哈希分配），或指定固定前景色；默认预设高对比度无底色风格。
   - 提供可视化的 WinForms 插件设置对话框并自动持久化至用户注册表。
6. **Windows 资源管理器“发送到 (Send To)”集成**：无需管理员权限，一键加入系统的“发送到”菜单，右键多选任意文件或压缩包一键直达合并窗口。
7. **全功能命令行扩展**：支持多文件输入、压缩包直达、`@responsefile.txt` 列表文件、行号定位（`/Line`）、过滤器导入（`/Filters`）及配置导入（`/Config`）。

---

## 新功能使用指南

### 1. 精炼过滤 (Refine Filter)
用于在复杂的基准过滤器规则下，临时查找或过滤特定关键字，无需在底部过滤器列表中反复新建/删除临时过滤器。
- **快捷键**：
  - `Ctrl + E`：光标快速聚焦到过滤器列表窗口上方的 **Refine** 输入框。
  - `Ctrl + K`：一键清空 Refine 过滤条件，恢复原始过滤视图。
- **使用方法**：
  - 在 Refine 输入框中键入任意字符，主界面过滤列表会**实时即时**仅保留包含该字符的行（不区分大小写，空格分词）。
  - 清空输入框后，即刻恢复为下方过滤器列表原本匹配的所有行。

### 2. 独立标记书签窗口 (Marked Lines Display)
原版中打上标记（Marker 1~8）的行分散在各个位置。本增强版在主界面新增了专用的 **Marked Lines** 独立窗口（位于主界面右侧或下方的分割区域）：
- **使用方法**：
  - 在主视图中按数字键 `1` ~ `8`（或右键菜单 `Toggle Marker`）为当前行打上颜色标记；
  - 被标记的行会**实时汇总显示在 Marked 窗口**中；
  - 在 Marked 窗口中**双击任意一行**，主窗口会自动同步滚动并高亮定位到该原始行；
  - 可通过拖拽分割条（Splitter）自由调整 Marked 区域的宽度或高度。

### 3. 多日志合并排序与压缩包直读 (Open & Merge Logs)
面对分布式日志、Android 多服务日志或 Bug2Go 压缩包时，传统做法需要手工解压再寻找工具合并。
- **使用方法**：
  - **打开方式 A**：点击菜单 `File -> Open...`，在弹出的文件选择器中**同时选中多个文件**或**单个压缩包**；
  - **打开方式 B**：直接从 Windows 资源管理器中拖拽多个文件或压缩包放开到 TAT 窗口；
  - **打开方式 C**：在 Windows 资源管理器中多选文件，右键 `发送到 -> TextAnalysisTool.NET`；
  - 系统会自动呼出 **“Open / Merge Logs”** 对话框：
    - **单选打开**：双击列表中的任意文件，直接单独打开该文件查看。
    - **合并排序**：勾选想要合并的多个文件，确认下方自动生成的输出路径，点击 **Merge**：
      - 程序会自动并发解析各个文件中的时间戳（支持微秒级精度）；
      - 将多个日志文件严格按时间升序穿插合并为一个新的排好序的日志文件；
      - 每行日志前自动追加 `[source_tag]` 染色标识，轻松分辨各行日志来自哪个源文件；
      - 合并完成后自动载入并在主界面呈现。

### 4. 增强型行号规则与多选区间隐藏
- **多行选中一键区间隐藏**：
  - 在主日志列表中通过 `Shift + 点击` 或鼠标拖选选中任意连续/非连续的多行；
  - 鼠标右键，菜单顶部将动态出现：
    ```text
    Hide Lines 120 to 185
    ---------------------------
    Hide Lines Before This
    ...
    ```
  - 点击后自动计算所选集合中的最小行与最大行，生成范围排除过滤器，一键隐藏该区间内的全部干扰行。
- **双击编辑行号规则 (`FilterDialog`)**：
  - 在底部过滤器列表中双击行号过滤器（如 `[<n]`、`[>n]` 或 `[n..m]`），弹窗自动切换至 **Line number range** 类型；
  - 提供专属输入行：
    ```text
    Start line: [   160 ]   End line: [   180 ]   (leave empty for unbounded)
    ```
  - **规则语义（与全局 Excluding 完全一致）**：
    - 填 `Start=160, End为空`：勾选 Excluding 隐藏 `< 160`（保留从 160 开始的内容）；取消勾选仅显示 `< 160`；
    - 填 `Start=160, End=180`：勾选 Excluding 隐藏 `160..180` 之间的内容；取消勾选仅显示 `160..180`；
    - 填 `Start为空, End=180`：勾选 Excluding 隐藏 `> 180`（保留截至 180 的内容）；取消勾选仅显示 `> 180`；
  - **Description 实时联动**：修改行号或切换 Excluding 时，下方 Description 文本框即时联动变化；若用户手动输入了自定义备注，则会优先保留用户备注。

### 5. Android 日志插件与自定义色彩系统 (`TATPlugin_AndroidLog`)
- **一键快捷添加**：
  - 右键任意 Android logcat 格式的日志行，菜单顶部直接显示：
    - `Filter by PID {pid}`（携带颜色小图章图标）
    - `Filter by tag {tag}`（携带颜色小图章图标）
    - `More Android Filter Options >`（子菜单提供选色弹窗、强制随机色、默认黑模式等）
- **自定义色彩模式**：
  - 打开菜单 `Edit -> Plug-in Settings -> AndroidLogPlugin`（或从右键子菜单中点击 `Plug-in Settings...`）；
  - **Tag / PID 独立配置**：
    - **Default**：默认黑色/无色；
    - **Random Color**：自动从内置 24 色高对比度鲜艳调色板中挑选色彩。勾选 `Consistent color for same Tag / PID` 后，相同名称的 Tag 将始终保证分配到相同颜色，不同 Tag 颜色各异；
    - **Specified Color**：自定义固定前景色与背景色（默认预设高对比度蓝色 PID 与绿色 Tag，无背景色）；
  - 设置自动保存至当前用户的 Windows 注册表，软件重启后依然生效。

### 6. Windows 资源管理器“发送到 (Send To)”集成
- **配置方法**：
  - 点击主菜单 `Help -> Add to Windows "Send To" Menu`（或 `Edit -> Preferences -> More >> -> Add to Windows "Send To" Menu`）；
  - 提示添加成功后，即可在 Windows 文件资源管理器中右键任意多个日志或压缩包，选择 `发送到 -> TextAnalysisTool.NET`。
  - 如需取消，随时点击 `Remove from Windows "Send To" Menu` 即可安全移除。

### 7. 命令行参数参考
`TextAnalysisTool.NET.exe` 支持以下命令行调用方式：
```bat
REM 1. 极速打开单个日志文件
TextAnalysisTool.NET.exe "C:\logs\main.log"

REM 2. 打开单个压缩包（直接进入 Open/Merge 窗口选择成员）
TextAnalysisTool.NET.exe "C:\logs\bugreport.zip"

REM 3. 多文件合并（直接进入 Open/Merge 窗口全选合并）
TextAnalysisTool.NET.exe "C:\logs\main.log" "C:\logs\system.log" "C:\logs\events.log"

REM 4. 通过文件列表批量加载
TextAnalysisTool.NET.exe @"C:\temp\filelist.txt"

REM 5. 加载日志的同时套用预设过滤器并定位到指定行
TextAnalysisTool.NET.exe /Filters:"C:\rules\audio.tat" /Line:2500 "C:\logs\main.log"

REM 6. 直接读取剪贴板内容
TextAnalysisTool.NET.exe /Clipboard

REM 7. 导入首选项配置
TextAnalysisTool.NET.exe /Config:"C:\configs\pref.xml"

REM 8. 静默添加/移除 Windows“发送到”菜单
TextAnalysisTool.NET.exe /SendTo:add
TextAnalysisTool.NET.exe /SendTo:remove
```

---

## 构建与运行

### 环境需求
- **.NET SDK**（9.0 或更高版本）
- 目标运行时：**.NET Framework 4.8**（Windows 10 / 11 操作系统自带，零环境依赖）

### 编译构建
1. 在仓库根目录下执行构建命令：
   ```bash
   dotnet build src/TextAnalysisTool.NET.csproj -c Release
   dotnet build src/TATPlugin_AndroidLog/TATPlugin_AndroidLog.csproj -c Release
   ```
2. 构建产物：
   - 仓库根目录下的 `TextAnalysisTool.NET.exe`（单文件、绿色免安装、无额外三方 dll 依赖）；
   - 仓库根目录下的 `TATPlugin_AndroidLog.dll`（配套 Android 日志分析扩展插件）。

### 单文件打包原理
构建产物是纯粹的**自包含单文件** exe——**无需任何额外 `.dll`，也无需 `.exe.config`**。
原理说明：`FilterImage` 图标在 resx 中以 **base64 字符串**保存并在运行时解码，避免了二进制序列化依赖。MSBuild 采用经典 `.resources` 格式内嵌到 exe 内部，免除了对 `System.Resources.Extensions.dll` 的额外引用。

---

## 插件开发 SDK

行列表的右键菜单可以通过外部插件自由扩展。任何放置在 `TextAnalysisTool.NET.exe` 同目录下、命名为 `TATPlugin_*.dll` 的动态库均会在启动时被自动加载并呈现于 `Help | Installed plug-ins`。

### 公共接口契约
位于命名空间 `TextAnalysisTool.NET.Plugin`：
```csharp
public interface ITextAnalysisToolPlugin {
    string GetFileTypeDescription();
    string GetFileTypePattern();
    bool IsFileTypeSupported(string fileName);
    System.IO.TextReader GetReaderForFile(string fileName);
    System.Windows.Forms.Form GetSettingsDialog();
    System.Collections.Generic.IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText);
}

public sealed class PluginContextMenuItem {
    public string Text { get; }          // 菜单显示文字
    public string FilterPattern { get; } // 正则匹配表达式
    public bool CaseSensitive { get; }   // 是否区分大小写
    public System.Drawing.Color ForeColor { get; } // 过滤结果文本前景色
    public System.Drawing.Color BackColor { get; } // 过滤结果文本背景色
    public string Description { get; }   // 过滤器列表显示的描述备注
    public bool PromptForColor { get; }  // 点击时是否弹出颜色对话框
    public System.Action ClickAction { get; } // 自定义动作回调
    public System.Collections.Generic.IList<PluginContextMenuItem> SubItems { get; } // 下拉子菜单
}
```

参考实现代码请查看 `src/TATPlugin_AndroidLog/` 目录。
