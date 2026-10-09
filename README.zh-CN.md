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

## 已知缺口
- 目前仅验证了默认启动画面。深层功能仍需要行为层面的对比检查。
- 精炼过滤功能位于 `feature/refine-filter` 分支：在过滤结果之上再叠加一个"Refine"输入框做 AND 过滤（已应用到此新源码）。