# Decompilation plan

## Target
- Binary: `data/TextAnalysisTool.NET.exe` (217,088 bytes)
- Version / edition: `TextAnalysisTool.NET, Version=1.0.9456.32322` (2025-11-21), the user's **new** replacement build
- Ownership: the user confirmed on 2026-10-08 they own this software; it is MIT-licensed
- Privacy: the user was reminded on 2026-10-08 this repo stays private (no public remote; pre-push hook enforces it)

## Done criterion (agreed with the user at intake)
- [x] A readable, buildable C# source project is recovered from the binary (`managed-decompile`)
- [x] It builds with `dotnet build` on Windows
- [x] It launches and renders **pixel-identical** to the original (verified: forced window geometry, same SHA256)
- [x] Source is the base for the user's own feature extensions
- Cross-platform is **not** wanted (Windows-only WinForms app is the deliverable).

## Recon (`ud scan`)
- Format / arch / bitness: PE32, x86 32-bit, windows-gui, CLR v4.0.30319
- Compiler / toolchain: .NET (C#) — v4.0.30319
- Debug info: PDB path embedded (`D:\a\1\s\Source\obj\Release\TextAnalysisTool.NET.pdb`); .NET metadata intact
- Engine / family: .NET / WinForms application
- Protections: none found

## Prior art
- Official org publishes only binaries (MIT). `jay-mao-cn/TextAnalyzer` and `zholobov/atat` are re-implementations, not relevant to the user's goal.

## Route and why
- Route: `managed-decompile` (ilspycmd), un-obfuscated .NET decompiles to near-source.
- Tools route: `dotnet` (passes `ud tools check --route dotnet`; ilspycmd 8.2.0.7535 installed).

## Module map
| Module | Source folder | Purpose | Status |
|---|---|---|---|
| TextAnalysisTool.NET | src/TextAnalysisTool.NET/ | main app + WinForms UI (CTextAnalysisTool, MainWindow, dialogs, filters, LineCollection) | decompiled, builds, verified |
| TextAnalysisTool.NET.Plugin | src/TextAnalysisTool.NET.Plugin/ | plugin interface | decompiled |
| ToolBox | src/ToolBox/ | utility library | decompiled |
| TextAnalysisTool.NET.Properties | src/TextAnalysisTool.NET.Properties/ | resources | decompiled |

## Third-party libraries identified
| Library | Version | Evidence | Plan |
|---|---|---|---|
| .NET Framework / WinForms | net48 | csproj TargetFramework | keep as-is (Windows target) |
| System.Resources.Extensions | 8.0.0 (NuGet) | non-string .resx resources need it | added as PackageReference |
| none proprietary | — | — | — |

## Target layout
```
DECOMP_PLAN.md / DECOMPLOG.md
src/TextAnalysisTool.NET.csproj   (net48 WinForms; OutputPath -> repo root)
src/TextAnalysisTool.NET/         main app + UI
src/TextAnalysisTool.NET.Plugin/  plugin interface
src/ToolBox/                      utilities
data/TextAnalysisTool.NET.exe     original binary (gitignored)
TextAnalysisTool.NET.exe          build output (gitignored)
```