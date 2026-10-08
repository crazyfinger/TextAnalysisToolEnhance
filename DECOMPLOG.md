# DECOMPLOG

The journal of this decompilation. **Anything that isn't written here is lost at the next context
compaction.** Log paths, addresses, function names, types, file formats, what failed and why, and the next
step.

## Status
- Phase: done (initial recovery) <!-- intake | prior art | recon | tools | plan | inventory | vertical slice | modules | assets | verify | done -->
- Done criterion: see DECOMP_PLAN.md
- Next step: re-apply the Refine-filter feature to this new source (patch saved at
  `%TEMP%\opencode\refine-feature.patch`), then verify and commit.

## Facts (keep current)
| What | Value | Source |
|---|---|---|
| Binary | data/TextAnalysisTool.NET.exe (217,088 bytes) | ud scan |
| Assembly version | 1.0.9456.32322 (2025-11-21) | AssemblyName |
| Format / arch / compiler | PE32 x86, CLR v4.0.30319, .NET C# | ud scan |
| Route | managed-decompile (ilspycmd) | ud scan |
| Rebuilt exe | TextAnalysisTool.NET.exe (repo root) | dotnet build |
| Startup-shot hash (rebuilt) | 8283B9CF13B4EF0262B93662ADBD2ADF98DF0203DBF5776C2C81DDD1AD191682 | ud run --shot |
| Startup-shot hash (original) | 8283B9CF13B4EF0262B93662ADBD2ADF98DF0203DBF5776C2C81DDD1AD191682 | ud run --original --shot |

## Log
### 2026-10-08 (redo — new version)
- User reported the previously decompiled exe was the OLD build; the new one is
  `D:\tools\TextAnalysisTool.NET\TextAnalysisTool.NET.exe` (Version 1.0.9456.32322, 217,088 bytes).
- Deleted the old decomp repo entirely; re-created fresh at `D:\tools\TextAnalysisTool-decomp`.
- Copied new exe to `data/`; `ud scan`: same .NET/WinForms route, no protection.
- `ilspycmd -p` decompiled cleanly. New files vs old build: `CTextAnalysisTool.cs` (main controller is now
  `CTextAnalysisTool`, no longer `TextAnalysisTool` — so no namespace/type-name collision), plus
  `BooleanDwordConverter.cs`, `CustomCheckbox.cs`, `PasteTextConfirmationDialog.cs`,
  `PasteTextOverwriteEnum.cs`, `StringColorConverter.cs`, `StringRectangleConverter.cs`, `XmlColor.cs`,
  `app.manifest`.
- Moved to `src/`; removed CMake scaffold. csproj fixes: `System.Resources.Extensions` 8.0.0 +
  `GenerateResourceUsePreserializedResources` (for .resx non-string resources); `OutputPath` -> repo root.
- `dotnet build src/TextAnalysisTool.NET.csproj -c Release`: 0 warnings, 0 errors, output at repo root.
- Verified pixel-fidelity: forced a fixed window geometry (Manual 1024x700) in the shared registry key
  `HKCU:\Software\DavidAns\TextAnalysisTool.NET`, ran rebuilt and original — both 1002x644 screenshots are
  **byte-identical (SHA256 8283B9CF...)**. Restored the registry values afterwards.
  (Earlier differing window sizes were a WindowsDefaultBounds/DPI default-placement artifact, not a
  decompilation defect.)
- Old feature patch preserved at `%TEMP%\opencode\refine-feature.patch` for re-application.

## Next
- Re-apply the Refine-filter feature (LineCollection.IntersectWith, FilterCollection.SetRefineFilterText +
  FilterLines AND, RefineFilterControl, MainWindow layout/dock) to this new source. The class is now
  `CTextAnalysisTool`; verify the same seams exist (FilterCollection ctor takes CTextAnalysisTool, etc.).
- Re-run the reflection harness, rebuild, commit.