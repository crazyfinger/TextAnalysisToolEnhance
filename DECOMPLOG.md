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

### 2026-10-08 — feature: Refine filter (re-applied to v1.0.9456.32322)
- Re-applied the same changes on branch `feature/refine-filter`. All seams were identical; only the main
  controller class is now `CTextAnalysisTool` (so the handler calls
  `m_textAnalysisTool.FilterCollection.SetRefineFilterText(...)`, unchanged).
- New version also changed the status bar from `StatusBar` to `StatusStrip` — the `Controls.AddRange` and
  `ShowOnlyFilteredLinesChanged` edits used `m_statusStrip`, no other impact on the refine feature.
- Build: 0 warnings/errors, output at repo root.
- Verified: reflection harness all PASS against the new build (refine 'beta'->2/6; alpha∩two->1 'alpha two').
  App launches with the refine strip, no crash.

### 2026-10-08 — refine filter UI polish
- Fixed label "Refine:" and "Clear" button text truncated at the two ends of the strip (they were fixed-width
  and clipped at higher DPI). Switched label/checkboxes/button to AutoSize and added panel Padding.
- Added two checkboxes: "Match case" (case-sensitive) and "Regex".
- `RefineFilterControl` now exposes `CaseSensitive`, `UseRegex`; event renamed `RefineTextChanged` ->
  `CriteriaChanged` (raised on text change or either checkbox toggle).
- `FilterCollection.SetRefineFilterText(text, caseSensitive, usesRegularExpression)` passes the flags into
  the `MatchesTextFilter`; `MainWindow` handler forwards them.
- Verified by reflection harness (case-sensitive 'Alpha'->0, ignore-case 'Alpha'->2, regex '\btwo\b'->2).
  App launches, no crash.

### 2026-10-09 — refine filter performance
- Lag cause: every keystroke re-ran a full-source accept pass (`new LineCollection(sourceLines, refine)`)
  plus the union scan; and it re-filtered immediately per key. Display is virtualized, so it was not the
  rendering.
- Fix 1: `LineCollection.RemoveLinesNotAcceptedBy(Filter)` — the refine now inspects ONLY the lines already
  in the filter result (via `LineNumbersInCollection`), not the whole file. Big win when the base filters
  already narrow a large file. `FilterCollection.FilterLines()` uses it instead of building a separate
  full-source refine collection + `IntersectWith`.
- Fix 2: `RefineFilterControl` debounces typing with a 250 ms `Timer` — `CriteriaChanged` fires only after
  the user pauses, not per key. Checkbox toggles still apply immediately.
- Verified by reflection harness (incl. `RemoveLinesNotAcceptedBy`: 'alpha' 2 -> 1 'alpha two'). App
  launches, no crash.

### 2026-10-09 — refine filter on a background thread + focus shortcut
- **Background refine**: `FilterCollection.FilterLines()` still computes the base union on the UI thread
  (fast bit ops), but the refine accept pass now runs on a `ThreadPool` worker. It operates on a deep copy
  (`new LineCollection(LineCollection)` copy ctor, deep-copied BitArray) so the UI-side base is never
  mutated. When done, `CTextAnalysisTool.OnRefineComputed` marshals back to the UI thread via
  `m_mainWindow.BeginInvoke`, checks a generation counter (stale results from older keystrokes are dropped),
  then swaps in the refined collection and refreshes the display + "Fil" count. `SetRefineFilterText` bumps
  the generation too, so clearing/typing invalidates any in-flight pass.
- **Shortcut**: new Edit menu item "Focus Refine Box" (Ctrl+K). It toggles focus onto the refine TextBox and
  back: first press saves the previously focused control and focuses the box; second press restores focus to
  it (falls back to the line list if gone). Menu item shows a check when the box is focused.
- Verified: reflection harness all PASS incl. copy-ctor independence (clone refined to 1, original base still
  2). App launches, no crash. (Async/threading and focus toggle are exercised at runtime by the user.)

### 2026-10-09 — shortcut rework
- "Focus Refine Box" shortcut changed Ctrl+K -> **Ctrl+E**.
- New Edit > "Clear Refine Filter" on **Ctrl+K**: calls the refine box's Clear (clears the refine filter).
  Added `RefineFilterControl.ClearRefine()`; the Clear button uses it too.
- File > "Load Filters..." now has **Ctrl+L**.