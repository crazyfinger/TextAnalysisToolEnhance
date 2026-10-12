# TextAnalysisToolEnhance

> English | [中文版](README.zh-CN.md)

**TextAnalysisToolEnhance** is an enhanced, modernized edition modified and evolved from the classic open-source tool **TextAnalysisTool.NET** (by David L. A. Anson). The entire project was developed and refined under the deep engineering assistance and pair-programming of an advanced **AI coding assistant (Gemini CLI)**. While preserving 100% of the classic lightweight, zero-latency regex filtering, color highlighting, and marker capabilities, it introduces crucial modern upgrades **specially tailored for Android system logs and large-scale distributed log analysis**.

---

## Why It Excels for Android Log Analysis

During Android OS, Framework, and embedded IoT development, engineers frequently handle massive **Bugreport / Bug2Go archives** or disparate log buffers (`main.log`, `system.log`, `events.log`, `crash.log`, `kernel.log`, etc.):
- **Direct Archive Reading**: No need to manually extract large archives. Simply open or drag-and-drop `.zip`, `.7z`, or `.gz` files into the tool to inspect text members inside.
- **Timestamp Synchronization & Log Merging**: Log streams from multiple buffers are naturally fragmented and out-of-sync. The tool automatically detects microsecond-precision timestamps, interleaves and chronologically sorts multiple logs in strict ascending order, and prefixes each line with colored `[source_tag]` labels to reconstruct the full event timeline in a single view.
- **Android-Tailored Quick Filtering & 24-Color High-Contrast Palette**: Right-click any logcat line to automatically extract PID and Tag. The built-in 24-color vibrant palette provides deterministic hash mapping (ensuring the same Tag always receives the exact same vivid color throughout analysis, while distinct Tags pop out with strong contrast).
- **Instant Refine Filtering**: Narrow down huge filter result sets on the fly by pressing `Ctrl+E` to type AND keywords, and press `Ctrl+K` to clear instantly without touching your underlying filter rules.
- **Multi-Line Selection Range Hiding**: Select multiple lines and right-click `Hide Lines {min} to {max}` to instantly silence noisy loops or spammy background services.

---

## Enhanced Features Overview

Compared to the official original TextAnalysisTool.NET, this edition adds:

1. **Refine Filter**: Apply a second-layer real-time keyword filter (AND logic) on top of already-filtered lines, featuring instant matching and dedicated shortcuts (`Ctrl+E`, `Ctrl+K`).
2. **Dedicated Marked Lines Window**: A dedicated panel that summarizes all lines tagged with Markers (1~8) globally, supporting dual-view synchronization and double-click navigation.
3. **Multi-Source Log Merge & Archive Direct Reading (Open & Merge / Archive Support)**:
   - Native support for `.zip`, `.7z`, and `.gz` archives — inspect inner log files directly without manual pre-extraction.
   - Automatic timestamp parsing across multiple files to merge and chronologically sort logs in strict ascending order, appending customizable `[source_tag]` prefixes.
4. **Enhanced Line Number Filtering & Multi-Line Selection Hide Range**:
   - Right-click multi-selected lines to instantly create a `Hide Lines {min} to {max}` exclusion filter.
   - Double-click any line number filter to edit start and end bounds in `FilterDialog`; strict semantic alignment of `Excluding` parameter; real-time description synchronization.
5. **AndroidLog Plugin with Custom Color System (`TATPlugin_AndroidLog`)**:
   - Right-click logcat lines to generate filters by PID or Tag.
   - Built-in 24-color high-contrast vibrant palette with consistent hash mapping per Tag/PID name, or custom fixed colors; defaults to high-contrast foreground color with no background.
   - Visual WinForms settings dialog with automatic per-user Registry persistence.
6. **Windows Explorer "Send To" Integration**: One-click registration to the Windows "Send to" menu without requiring administrator privileges; select multiple files or archives in Explorer to directly launch the Open & Merge dialog.
7. **Comprehensive Command Line Interface**: Supports multiple input files, archive opening, `@responsefile.txt` listfiles, line jump (`/Line`), filter loading (`/Filters`), and configuration import (`/Config`).

---

## Feature User Guide

### 1. Refine Filter
Quickly drill down into filtered results without having to create, configure, and later delete temporary filters in the bottom list.
- **Shortcuts**:
  - `Ctrl + E`: Move focus directly to the **Refine** text box located above the filter list window.
  - `Ctrl + K`: Clear the refine filter text and instantly restore the full filtered view.
- **Usage**:
  - Type any text into the Refine box; the main list immediately narrows down to lines matching the refine query (case-insensitive, space-separated terms).
  - Clear the box to return to the original filtered lines.

### 2. Dedicated Marked Lines Window (Marked Lines Display)
In the original application, marked lines (Markers 1~8) were scattered throughout the view. This edition provides a dedicated **Marked Lines** window (split vertically or horizontally):
- **Usage**:
  - Press number keys `1` ~ `8` (or use the right-click menu `Toggle Marker`) on any line to apply a color marker;
  - All marked lines are aggregated in real time inside the Marked Lines window;
  - **Double-click any line** in the Marked Lines window to scroll and highlight that exact line in the main view;
  - Drag the splitter bar to adjust the size of the Marked pane.

### 3. Multi-Source Log Merge & Archive Direct Reading (Open & Merge Logs)
Merge distributed logs or inspect bugreport archives effortlessly:
- **Usage**:
  - **Method A**: Click `File -> Open...` and select **multiple files** or an **archive**;
  - **Method B**: Drag and drop multiple files or an archive from Windows Explorer into the window;
  - **Method C**: In Windows Explorer, right-click selected files and choose `Send to -> TextAnalysisTool.NET`;
  - The **"Open / Merge Logs"** dialog will appear:
    - **Single File Open**: Double-click any item in the list to open it directly.
    - **Merge & Sort**: Check the desired files, verify the generated output path, and click **Merge**:
      - Timestamps in all files are concurrently parsed with microsecond precision;
      - Lines are interleaved in chronological order into a unified output file;
      - Each line is annotated with a `[source_tag]` prefix for instant visual attribution;
      - The merged result is automatically loaded into the main view upon completion.

### 4. Enhanced Line Number Filtering & Multi-Selection Hide Range
- **Multi-Line Selection Hide Range**:
  - In the main log view, `Shift + Click` or drag to select multiple lines;
  - Right-click to see the dynamic option:
    ```text
    Hide Lines 120 to 185
    ---------------------------
    Hide Lines Before This
    ...
    ```
  - Clicking this option creates a range exclusion filter that hides all lines between the minimum and maximum selected line numbers.
- **Double-Click Edit Line Number Filter (`FilterDialog`)**:
  - Double-click any line number filter (`[<n]`, `[>n]`, or `[n..m]`) in the filter list to open the editor in **Line number range** mode;
  - Configure the range using the dedicated inputs:
    ```text
    Start line: [   160 ]   End line: [   180 ]   (leave empty for unbounded)
    ```
  - **Semantics (Aligned with Global Excluding Logic)**:
    - `Start=160, End empty`: Checked Excluding hides `< 160` (keeps from 160 onwards); Unchecked displays `< 160`;
    - `Start=160, End=180`: Checked Excluding hides lines `160..180`; Unchecked displays `160..180`;
    - `Start empty, End=180`: Checked Excluding hides `> 180` (keeps up to 180); Unchecked displays `> 180`;
  - **Real-Time Description Sync**: Changing line numbers or toggling Excluding updates the Description field automatically unless custom text was manually provided.

### 5. AndroidLog Plugin with Custom Colors (`TATPlugin_AndroidLog`)
- **Quick-Add Filters**:
  - Right-click any Android logcat line to see top-level actions:
    - `Filter by PID {pid}` (with color swatch icon)
    - `Filter by tag {tag}` (with color swatch icon)
    - `More Android Filter Options >` (submenu with on-the-fly ColorDialog picker, random color, default black)
- **Custom Color Configuration**:
  - Open `Edit -> Plug-in Settings -> AndroidLogPlugin` (or choose `Plug-in Settings...` from the right-click submenu);
  - **Independent PID / Tag Settings**:
    - **Default**: Classic black / no color;
    - **Random Color**: Automatically picks from a curated 24-color high-contrast palette. When `Consistent color for same Tag / PID` is checked, the same tag name always receives the same deterministic color;
    - **Specified Color**: Pick custom foreground and background colors (defaults to high-contrast blue PID and green Tag with no background);
  - Settings are persisted across sessions in the Windows Registry under `HKCU`.

### 6. Windows Explorer "Send To" Integration
- **Setup**:
  - Click `Help -> Add to Windows "Send To" Menu` (or `Edit -> Preferences -> More >> -> Add to Windows "Send To" Menu`);
  - Select multiple logs or archives in Windows Explorer, right-click, and select `Send to -> TextAnalysisTool.NET`.
  - Click `Remove from Windows "Send To" Menu` at any time to remove the shortcut.

### 7. Command Line Usage
`TextAnalysisTool.NET.exe` supports the following CLI patterns:
```bat
REM 1. Open a single log file
TextAnalysisTool.NET.exe "C:\logs\main.log"

REM 2. Open an archive (routes to Open/Merge dialog)
TextAnalysisTool.NET.exe "C:\logs\bugreport.zip"

REM 3. Merge multiple files (routes to Open/Merge dialog)
TextAnalysisTool.NET.exe "C:\logs\main.log" "C:\logs\system.log" "C:\logs\events.log"

REM 4. Load from a response listfile
TextAnalysisTool.NET.exe @"C:\temp\filelist.txt"

REM 5. Apply filters and jump to a line number
TextAnalysisTool.NET.exe /Filters:"C:\rules\audio.tat" /Line:2500 "C:\logs\main.log"

REM 6. Load text from clipboard
TextAnalysisTool.NET.exe /Clipboard

REM 7. Import preferences configuration
TextAnalysisTool.NET.exe /Config:"C:\configs\pref.xml"

REM 8. Silently register or unregister Windows Send To shortcut
TextAnalysisTool.NET.exe /SendTo:add
TextAnalysisTool.NET.exe /SendTo:remove
```

---

## Build and Run

### Requirements
- **.NET SDK** (9.0 or later)
- Target Runtime: **.NET Framework 4.8** (pre-installed on Windows 10 and 11)

### Build Commands
```bash
dotnet build src/TextAnalysisTool.NET.csproj -c Release
dotnet build src/TATPlugin_AndroidLog/TATPlugin_AndroidLog.csproj -c Release
```
Outputs:
- `TextAnalysisTool.NET.exe` in repository root (standalone, portable, zero external DLL dependencies);
- `TATPlugin_AndroidLog.dll` in repository root.

### Single-File Packaging
The executable is completely **self-contained** without requiring `.exe.config` or external packages. The `FilterImage` icon is stored as base64 in the resx and decoded at runtime, allowing MSBuild to use the classic `.resources` reader built into `System.dll`.

---

## Plug-in SDK

The context menu in the line list is extensible through external plug-ins. Any DLL named `TATPlugin_*.dll` placed next to `TextAnalysisTool.NET.exe` is automatically loaded and displayed under `Help | Installed plug-ins`.

### Public Interface
Located in namespace `TextAnalysisTool.NET.Plugin`:
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
    public string Text { get; }
    public string FilterPattern { get; }
    public bool CaseSensitive { get; }
    public System.Drawing.Color ForeColor { get; }
    public System.Drawing.Color BackColor { get; }
    public string Description { get; }
    public bool PromptForColor { get; }
    public System.Action ClickAction { get; }
    public System.Collections.Generic.IList<PluginContextMenuItem> SubItems { get; }
}
```

See `src/TATPlugin_AndroidLog/` for a reference implementation.
