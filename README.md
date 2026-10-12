# TextAnalysisTool-decomp: local reconstruction

> English | [中文版](README.zh-CN.md)

A private, local reconstruction of software I own, made with universal-decompiler. **Not for publication:**
it contains code derived from the original and must never be pushed to a public remote (the pre-push hook
refuses). The original binary stays in `data/`, which is not tracked.

## What this is
A `managed-decompile` of the MIT-licensed TextAnalysisTool.NET **Version 1.0.9456.32322 (2025-11-21)**. The
recovered C# source is the base for the user's own feature extensions. Windows-only WinForms (cross-platform
not needed).

## Build
Requires the .NET SDK (9+ works) and `ilspycmd` (only to re-decompile):

1. Install the .NET SDK from <https://dotnet.microsoft.com/download> if you don't have it.
2. (Only needed to re-decompile) install ilspycmd 8.2.0.7535 — the latest package is broken:
   `dotnet tool install --global ilspycmd --version 8.2.0.7535`
3. Build from the repo root:
   `dotnet build src/TextAnalysisTool.NET.csproj -c Release`
   - `-c Release` = release build (omit it for a debug build).
4. Output: a single self-contained `TextAnalysisTool.NET.exe` in the repo root (gitignored, not committed).

That's it — after editing code, just re-run step 3. The project writes the exe (and a `.pdb`) straight into
the repo root via `<OutputPath>` in the csproj.

### Single-file packaging notes

The build produces a **self-contained, single-file** exe — **no extra `.dll`s and no `.exe.config`**. It
depends only on the .NET Framework 4.8 runtime that ships with Windows, so it runs out-of-the-box on any
Windows machine, exactly like the official binary.

How it works: `FilterImage` is stored as a **base64 string** in the resx (decoded to a `Bitmap` at runtime)
instead of a binary-serialized object. Keeping every resource a plain string makes MSBuild emit the
**classic** `.resources` format, whose reader lives inside `System.dll` — so the `System.Resources.Extensions`
package and the old binding-redirect `.exe.config` are no longer required.

> **Caution:** do **not** try to merge `System.Resources.Extensions.dll` into the exe (e.g. with ILRepack).
> The embedded `.resources` header resolves its reader by the strong-named assembly name
> `System.Resources.Extensions, Version=4.0.0.0`; once merged that assembly no longer exists and startup
> fails with `System.IO.FileLoadException (0x80131040)`.

## Re-decompiling from scratch
- `ilspycmd -p -o build/decompiled data/TextAnalysisTool.NET.exe`
- Move the result into `src/`. Build fixes needed (see DECOMPLOG.md): the `System.Resources.Extensions`
  package reference, and `OutputPath` set to the repo root.

## Run
- Rebuilt: `ud run` (or run `TextAnalysisTool.NET.exe`)
- Original: `ud run --original`
- With equal window geometry the two render pixel-identically (verified by screenshot hash).

## Plug-ins: develop / build / load

The line-list context menu is extensible through plug-ins. Any DLL named `TATPlugin_*.dll` placed next to
`TextAnalysisTool.NET.exe` is discovered at startup (see Help | Installed plug-ins). Plug-ins were
originally file readers only; the SDK now also lets a plug-in contribute right-click actions that add a
filter for the line you clicked (e.g. Android "Filter by TAG/PID").

### How it works
- At startup the app scans its own folder for `TATPlugin_*.dll`, reflects over every public type, and
  wraps each one that exposes the plug-in methods (discovery is by method name, not interface matching).
- A plug-in provides the five file-reader methods plus two optional ones:
  `GetSettingsDialog()` and `GetLineContextMenuItems(string lineText)`.
- When you right-click a line, the app calls `GetLineContextMenuItems` on that line's text for every
  plug-in. Each returned `PluginContextMenuItem` becomes a top-level menu item; clicking it creates a
  regex filter from `FilterPattern` and applies it. Plug-ins that return nothing are simply absent from
  the menu, so a plug-in that doesn't recognise a line type never bothers you.

### The public SDK types
These live in the app assembly (namespace `TextAnalysisTool.NET.Plugin`) and are all you need:

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
    public string Text { get; }          // the menu label, e.g. "Filter by tag AudioSystem"
    public string FilterPattern { get; } // a regex that keeps the matching lines
    public bool CaseSensitive { get; }   // whether the regex filter is case-sensitive
    public System.Drawing.Color ForeColor { get; } // text color for the created filter (Color.Empty for default)
    public System.Drawing.Color BackColor { get; } // background color for the created filter
    public string Description { get; }   // description displayed in the filter list
    public bool PromptForColor { get; }  // whether clicking prompts user with a ColorDialog
    public System.Action ClickAction { get; } // custom action callback when clicked
    public System.Collections.Generic.IList<PluginContextMenuItem> SubItems { get; } // child dropdown items
}
```

- `GetFileTypeDescription` / `GetFileTypePattern` / `IsFileTypeSupported` / `GetReaderForFile` are the
  original file-reading contract; implement them (returning `""`/`false`/`null` is fine for a plug-in
  that only provides context-menu rules).
- `GetSettingsDialog()` may return `null` to mean "no settings dialog".
- `GetLineContextMenuItems(lineText)` is optional; if omitted the plug-in never contributes menu items.

### Minimal example
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
        // Recognise only lines you care about; return an empty list otherwise.
        if (lineText == null || !lineText.Contains("KEYWORD")) {
            return new List<PluginContextMenuItem>();
        }
        return new List<PluginContextMenuItem> {
            new PluginContextMenuItem("Filter by KEYWORD", "KEYWORD", caseSensitive: false)
        };
    }
}
```

A real example — Android logcat tag/PID filtering — lives in `src/TATPlugin_AndroidLog/`.

### Build
The plug-in is a plain `net48` class library that references the app exe (for the SDK types).

1. Build the app first so the exe exists:
   `dotnet build src/TextAnalysisTool.NET.csproj`
2. Build the plug-in:
   `dotnet build src/TATPlugin_AndroidLog/`

csproj template (mirror `src/TATPlugin_AndroidLog/TATPlugin_AndroidLog.csproj`):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>TATPlugin_MyPlugin</AssemblyName>
    <OutputType>Library</OutputType>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>11.0</LangVersion>
    <OutputPath>$(MSBuildProjectDirectory)\..\..\</OutputPath> <!-- repo root, next to the exe -->
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

> **Important:** the app project is an SDK-style project that globs `src/**/*.cs`. When you add a plug-in
> folder under `src/`, exclude it from the app build with `<Compile Remove="TATPlugin_AndroidLog\**" />`
> in `src/TextAnalysisTool.NET.csproj` (already done for the example), or the app will compile the
> plug-in source (and its `obj/` files) too and fail with duplicate-attribute errors (`CS0579`).

### Load
Copy the produced `TATPlugin_*.dll` into the same folder as `TextAnalysisTool.NET.exe` (the template
above already outputs it there). Restart the app and check Help | Installed plug-ins. Right-click a line
that your plug-in recognises to see its actions at the top of the context menu; the marker toggles now
live under "Toggle Marker".

### Notes
- Discovery is by method name, so plug-ins need not reference or implement the interface to be loaded,
  but referencing the SDK types makes authoring safe.
- If a plug-in throws from a method it is disabled for that session (a dialog explains); returning `null`
  is allowed and does not disable it.

## Known gaps
- Only the default startup screen is verified so far. Deep features still need behavioural checks.
- Feature branch `feature/refine-filter` adds a "Refine" input box that ANDs an extra filter on top of the
  filtered text (being re-applied to this new source).