# TextAnalysisTool-decomp：本地重建

> 中文版 | [English](README.md)

基于 universal-decompiler 对本人拥有软件做的私有本地重建。**仅供本地管理**：内含从原版派生的代码，未经你本人明确同意不应推送到公共远程（pre-push 守卫会拦截）。原版二进制保留在 `data/`，不被 git 跟踪。

## 这是什么

对 **MIT 许可** 的 TextAnalysisTool.NET **版本 1.0.9456.32322（2025-11-21）** 的 `managed-decompile` 反编译还原。恢复出的 C# 源码作为你自己功能扩展的基础。仅面向 Windows 的 WinForms 应用（无需跨平台）。

## 构建

需要 .NET SDK（9+ 即可）与 `ilspycmd`（仅重新反编译时需要）：
- `dotnet tool install --global ilspycmd --version 8.2.0.7535`（最新包已损坏，需用该版本）
- `dotnet build src/TextAnalysisTool.NET.csproj -c Release`
- 产物：仓库根目录下的 `TextAnalysisTool.NET.exe`（已被 gitignore）

## 从零重新反编译
- `ilspycmd -p -o build/decompiled data/TextAnalysisTool.NET.exe`
- 把结果移入 `src/`。需要的构建修复见 DECOMPLOG.md：`System.Resources.Extensions` 包引用，以及 `OutputPath` 设置为仓库根目录。

## 运行
- 重建版：`ud run`（或直接运行 `TextAnalysisTool.NET.exe`）
- 原版：`ud run --original`
- 在相同窗口几何下两者渲染逐像素一致（已通过截图哈希验证）。

## 已知缺口
- 目前仅验证了默认启动画面。深层功能仍需要行为层面的对比检查。
- 精炼过滤功能位于 `feature/refine-filter` 分支：在过滤结果之上再叠加一个"Refine"输入框做 AND 过滤（已应用到此新源码）。