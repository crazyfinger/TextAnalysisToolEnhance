# 反编译计划

> 中文版 | [English](DECOMP_PLAN.md)

## 目标
- 二进制：`data/TextAnalysisTool.NET.exe`（217,088 字节）
- 版本 / 版本类型：`TextAnalysisTool.NET, Version=1.0.9456.32322`（2025-11-21），用户**新的**替换版构建
- 所有权：用户于 2026-10-08 确认拥有该软件；MIT 许可
- 隐私：已于 2026-10-08 提醒用户本仓库应保持私有（不得推送公共远程；pre-push 守卫强制）

## 完成标准（intake 时与用户约定）
- [x] 从二进制恢复出可读、可构建的 C# 源码工程（`managed-decompile`）
- [x] 可在 Windows 上通过 `dotnet build` 构建
- [x] 能启动并与原版**逐像素一致**渲染（已验证：强制窗口几何，SHA256 相同）
- [x] 源码作为用户自行功能扩展的基础
- 跨平台**不需要**（交付物为仅 Windows 的 WinForms 应用）。

## 侦察（`ud scan`）
- 格式 / 架构 / 位宽：PE32, x86 32 位, windows-gui, CLR v4.0.30319
- 编译器 / 工具链：.NET (C#) — v4.0.30319
- 调试信息：嵌入 PDB 路径（`D:\a\1\s\Source\obj\Release\TextAnalysisTool.NET.pdb`）；.NET 元数据完整
- 引擎 / 家族：.NET / WinForms 应用
- 保护：未发现

## 先例
- 官方组织只发布二进制（MIT）。`jay-mao-cn/TextAnalyzer` 与 `zholobov/atat` 是重实现，与本用户目标无关。

## 路线与原因
- 路线：`managed-decompile`（ilspycmd），未混淆的 .NET 可还原出接近源码的结果。
- 工具路线：`dotnet`（`ud tools check --route dotnet` 通过；已安装 ilspycmd 8.2.0.7535）。

## 模块图
| 模块 | 源码目录 | 用途 | 状态 |
|---|---|---|---|
| TextAnalysisTool.NET | src/TextAnalysisTool.NET/ | 主程序 + WinForms UI（CTextAnalysisTool、MainWindow、对话框、过滤器、LineCollection） | 已反编译、可构建、已验证 |
| TextAnalysisTool.NET.Plugin | src/TextAnalysisTool.NET.Plugin/ | 插件接口 | 已反编译 |
| ToolBox | src/ToolBox/ | 工具库 | 已反编译 |
| TextAnalysisTool.NET.Properties | src/TextAnalysisTool.NET.Properties/ | 资源 | 已反编译 |

## 识别到的第三方库
| 库 | 版本 | 依据 | 计划 |
|---|---|---|---|
| .NET Framework / WinForms | net48 | csproj TargetFramework | 保持原样（Windows 目标） |
| System.Resources.Extensions | 8.0.0 (NuGet) | 非字符串 .resx 资源需要 | 以 PackageReference 添加 |
| 无专有库 | — | — | — |

## 目标布局
```
DECOMP_PLAN.md / DECOMPLOG.md
src/TextAnalysisTool.NET.csproj   (net48 WinForms; OutputPath -> 仓库根目录)
src/TextAnalysisTool.NET/         主程序 + UI
src/TextAnalysisTool.NET.Plugin/  插件接口
src/ToolBox/                      工具
data/TextAnalysisTool.NET.exe     原版二进制（已 gitignore）
TextAnalysisTool.NET.exe          构建产物（已 gitignore）
```