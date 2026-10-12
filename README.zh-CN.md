# TextAnalysisTool-decomp：本地重建

> 中文版 | [English](README.md)

基于 universal-decompiler 对本人拥有软件做的私有本地重建。**仅供本地管理**：内含从原版派生的代码，未经你本人明确同意不应推送到公共远程（pre-push 守卫会拦截）。原版二进制保留在 `data/`，不被 git 跟踪。

## 这是什么

对 **MIT 许可** 的 TextAnalysisTool.NET **版本 1.0.9456.32322（2025-11-21）** 的 `managed-decompile` 反编译还原。恢复出的 C# 源码作为你自己功能扩展的基础。仅面向 Windows 的 WinForms 应用（无需跨平台）。

## 构建

需要 .NET SDK（9+ 即可）与 `ilspycmd`（仅重新反编译时需要）：

1. 若未安装 .NET SDK，先从 <https://dotnet.microsoft.com/download> 安装。
2. （仅重新反编译时需要）安装 ilspycmd 8.2.0.7535——最新包已损坏：
   `dotnet tool install --global ilspycmd --version 8.2.0.7535`
3. 在仓库根目录构建：
   `dotnet build src/TextAnalysisTool.NET.csproj -c Release`
   - `-c Release` = 发布版构建（省略即为调试版）。
4. 产物：仓库根目录下**自包含的单文件** `TextAnalysisTool.NET.exe`（已被 gitignore，不会提交）。

改完代码后，只需重新执行第 3 步即可。csproj 中的 `<OutputPath>` 已配置为把 exe（及 `.pdb`）直接输出到仓库根目录。

### 单文件打包说明

构建产物是**自包含的单文件** exe——**无需任何额外 `.dll`，也无需 `.exe.config`**。它只依赖 Windows 自带的 .NET Framework 4.8 运行时，因此在任何 Windows 机器上都能开箱即用，和官方 exe 完全一致。

原理：`FilterImage` 图标在 resx 中以 **base64 字符串**保存（运行时解码为 `Bitmap`），而不是二进制序列化对象。所有资源都变成纯字符串后，MSBuild 会采用**经典** `.resources` 格式——其读取器位于 `System.dll` 内部，因此不再需要 `System.Resources.Extensions` 包，也删除了旧的绑定重定向 `.exe.config`。

> **注意：** 不要尝试用 ILRepack 之类工具把 `System.Resources.Extensions.dll` 合并进 exe。内嵌 `.resources` 头是按强命名程序集名 `System.Resources.Extensions, Version=4.0.0.0` 解析读取器的；合并后该程序集不复存在，启动会报 `System.IO.FileLoadException (0x80131040)`。

## 从零重新反编译
- `ilspycmd -p -o build/decompiled data/TextAnalysisTool.NET.exe`
- 把结果移入 `src/`。需要的构建修复见 DECOMPLOG.md：`System.Resources.Extensions` 包引用，以及 `OutputPath` 设置为仓库根目录。

## 运行
- 重建版：`ud run`（或直接运行 `TextAnalysisTool.NET.exe`）
- 原版：`ud run --original`
- 在相同窗口几何下两者渲染逐像素一致（已通过截图哈希验证）。

## 插件：开发 / 编译 / 加载

行列表的右键菜单可以通过插件扩展。任何放在 `TextAnalysisTool.NET.exe` 同目录、命名为 `TATPlugin_*.dll`
的 DLL 都会在启动时被发现（见 Help | Installed plug-ins）。插件原本只用于读取文件；现在 SDK 还允许插件
为当前右键的行提供"加入过滤器"的菜单项（例如安卓日志的 "Filter by TAG/PID"）。

### 工作原理
- 启动时程序扫描自身目录下的 `TATPlugin_*.dll`，反射其中的所有公共类型，把暴露了插件方法的类型包装成
  插件（按**方法名**识别，不要求实现接口）。
- 插件需提供 5 个文件读取方法，另有 2 个可选方法：`GetSettingsDialog()` 与
  `GetLineContextMenuItems(string lineText)`。
- 右键某一行时，程序会对每个插件调用 `GetLineContextMenuItems`（传入该行文本）。每个返回的
  `PluginContextMenuItem` 会成为菜单顶层项；点击后根据 `FilterPattern` 生成一个正则过滤器并立即生效。
  返回空列表的插件不会出现在菜单里——插件无法识别的行类型不会打扰你。

### 公共 SDK 类型
它们位于主程序程序集（命名空间 `TextAnalysisTool.NET.Plugin`）：

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
    public string Text { get; }          // 菜单文字，如 "Filter by tag AudioSystem"
    public string FilterPattern { get; } // 用于保留匹配行的正则
    public bool CaseSensitive { get; }   // 正则过滤器是否区分大小写
    public System.Drawing.Color ForeColor { get; } // 所生成过滤器的文本前景色（Color.Empty 表示默认）
    public System.Drawing.Color BackColor { get; } // 所生成过滤器的背景色
    public string Description { get; }   // 过滤器列表中显示的描述文字
    public bool PromptForColor { get; }  // 点击时是否弹出颜色对话框让用户挑选颜色
    public System.Action ClickAction { get; } // 自定义点击回调
    public System.Collections.Generic.IList<PluginContextMenuItem> SubItems { get; } // 子下拉菜单项
}
```

- `GetFileTypeDescription` / `GetFileTypePattern` / `IsFileTypeSupported` / `GetReaderForFile` 是原有的
  文件读取契约；若插件只提供右键规则，返回 `""`/`false`/`null` 即可。
- `GetSettingsDialog()` 可返回 `null` 表示"没有设置对话框"。
- `GetLineContextMenuItems(lineText)` 可选；不实现则插件不贡献任何菜单项。

### 最小示例
```csharp
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TextAnalysisTool.NET.Plugin;

namespace MyPlugin;

public sealed class MyPlugin : ITextAnalysisToolPlugin {
    public string GetFileTypeDescription() => "";
    public string GetFileTypePattern() => "";
    public bool IsFileTypeSupported(string fileName) => false;
    public TextReader GetReaderForFile(string fileName) => null;
    public Form GetSettingsDialog() => null;

    public IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText) {
        // 只识别你关心的行，否则返回空列表
        if (lineText == null || !lineText.Contains("KEYWORD")) {
            return new List<PluginContextMenuItem>();
        }
        return new List<PluginContextMenuItem> {
            new PluginContextMenuItem("Filter by KEYWORD", "KEYWORD", caseSensitive: false)
        };
    }
}
```

一个真实示例——安卓 logcat 的 tag/PID 过滤——位于 `src/TATPlugin_AndroidLog/`。

### 编译
插件是一个普通的 `net48` 类库，引用主程序 exe（以获得 SDK 类型）。

1. 先构建主程序，确保 exe 存在：
   `dotnet build src/TextAnalysisTool.NET.csproj`
2. 再构建插件：
   `dotnet build src/TATPlugin_AndroidLog/`

csproj 模板（参见 `src/TATPlugin_AndroidLog/TATPlugin_AndroidLog.csproj`）：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>TATPlugin_MyPlugin</AssemblyName>
    <OutputType>Library</OutputType>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>11.0</LangVersion>
    <OutputPath>$(MSBuildProjectDirectory)\..\..\</OutputPath> <!-- 仓库根目录，即 exe 旁 -->
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Windows.Forms" />
    <Reference Include="TextAnalysisTool.NET">
      <HintPath>..\..\TextAnalysisTool.NET.exe</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

> **重要：** 主程序是 SDK 风格工程，默认会递归编译 `src/**/*.cs`。当你在 `src/` 下新增插件目录时，必须在
> `src/TextAnalysisTool.NET.csproj` 里用 `<Compile Remove="TATPlugin_AndroidLog\**" />` 把它排除（示例已
> 添加），否则主程序会把插件源码（及其 `obj/` 生成文件）一起编译，导致重复特性错误（`CS0579`）。

### 加载
把编译产物 `TATPlugin_*.dll` 复制到 `TextAnalysisTool.NET.exe` 同目录（上面的模板已直接输出到那里）。
重启程序，在 Help | Installed plug-ins 中确认。右键一行插件能识别的行，即可在右键菜单顶部看到它的
动作；标记切换项现在收在 "Toggle Marker" 子菜单里。

### 说明
- 插件按方法名识别，因此即使不引用/实现接口也能被加载，但引用 SDK 类型更安全。
- 若插件方法抛异常，该插件会在本次会话被禁用（会弹框说明）；返回 `null` 是被允许的，不会导致禁用。

## 已知缺口
- 目前仅验证了默认启动画面。深层功能仍需要行为层面的对比检查。
- 精炼过滤功能位于 `feature/refine-filter` 分支：在过滤结果之上再叠加一个"Refine"输入框做 AND 过滤（已应用到此新源码）。