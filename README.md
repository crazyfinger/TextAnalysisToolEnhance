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

## Known gaps
- Only the default startup screen is verified so far. Deep features still need behavioural checks.
- Feature branch `feature/refine-filter` adds a "Refine" input box that ANDs an extra filter on top of the
  filtered text (being re-applied to this new source).