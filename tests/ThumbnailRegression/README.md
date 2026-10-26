# Offline thumbnail regression (#179)

No accounts, cookies, external gallery requests or adult material are used. These are regression hosts, **not full E-Viewer end-to-end coverage**. They link the production source files rather than copying the parser/crop algorithm.

## Console (parser / signed bounds / publication state / download cache)

On Windows with the repository's VS 2022 Build Tools and NuGet restore completed:

```powershell
powershell -ExecutionPolicy Bypass -File tests/ThumbnailRegression/Run.ps1
```

`-CompileOnly` checks compilation without running tests. `-Smoke` runs only four parser checks (offsets, shorthand ordering and quoted URL delimiters). Compilation treats warnings as errors. Roslyn is located alongside MSBuild on PATH (CI), falling back to `C:\BuildTools`; `-BuildTools` overrides this.

The full console run also reads the 26 independent Chrome CSS geometry/rejection review cases in `parser-review-cases.json`. Thirteen supported valid cases must match exact URI/region, one unsupported size-reset combination must be safely rejected, and twelve invalid/unsupported cases must be rejected. The integer-overflow fixture uses `9223372036854775808px`, beyond `long`; `2147483648px` is representable in our signed/uint model and is correctly rejected by image bounds instead. CI runs this console suite once in the existing x64 build job; it never installs or runs the UWP host.

The full suite covers first/middle/last and mixed-size regions, ordinary `<img>`, missing nodes, HTML entities, quotes, reordered/separate CSS declarations, zero offsets, invalid/missing/unsupported CSS, negative/out-of-bounds/overflow bounds, same-URI region invalidation, single-flight and retry, local-file precedence, stale publication rejection, shared downloads and bounded/failed cache behavior. Unsupported backgrounds produce placeholders, never an uncropped sheet.

## UWP native image / BitmapImage integration host

This small app targets the same SDK and `16299` minimum as ExClient. It does **not** reference the complete product or change its solution. It links production `ThumbnailBitmap`, parser, source, state and cache helpers and reuses the console checks.

Build using the existing NuGet CLI and VS MSBuild. Supply a **local development** code-signing certificate (`CN=Opportunity`) from the current user's certificate store; do not modify a project or use release keys:

```powershell
nuget restore tests/ThumbnailRegression/Uwp/ThumbnailRegression.csproj
msbuild tests/ThumbnailRegression/Uwp/ThumbnailRegression.csproj /p:Configuration=Debug /p:Platform=x64 /p:PackageCertificateThumbprint=LOCAL_DEV_THUMBPRINT /p:PackageCertificateKeyFile=
```

Installation/trust/sideload policy changes and UI execution require a separately approved testing session. Build does not install or run this app. Use its generated AppPackages install script in that session, then activate package `EViewer.ThumbnailRegression`, application `App`.

- **Run / reload all checks** creates safe PNG/JPEG red/green/blue sprite fixtures in memory, checks first/middle/last crops against pixels decoded from the original, strict bounds failures, independent concurrent streams, full-image loading and production PNG-to-`BitmapImage` output. The three tiles should differ on first load and reload.
- WebP uses the included safe synthetic `Assets/sprite.webp` (288x96, generated offline with Pillow/libwebp). Decoder-not-found / initialization-failed HRESULTs are explicitly logged as **SKIP**, never PASS. Other failures fail the suite. No new WebP library or newer `WebpDecoderId` dependency is introduced. Passing on one Windows build does not establish Win10 16299 codec coverage.
- **Switch first tile to local file** writes and pixel-checks a safe yellow PNG in LocalState, uses `StorageFile.GetThumbnailAsync` and the production publication-state helper, and rejects a deliberately late remote `BitmapImage`. It logs the thumbnail Type/ContentType/Size. With an Image thumbnail it also checks yellow pixels; Windows may return an Icon instead (observed on Server 2022), in which case image-content validation is explicitly **SKIP**, not PASS. State publication checks remain independent. Reload restores the three sprite tiles. This tests the shared state/helper and local API integration, not full `GalleryImage`/database/network behavior.
- Results, including explicit skips and exceptions, are visible in the host and written to its `LocalState/results.txt`.

The host deliberately has no internet capability. The product's existing `ThumbHelper` still uses the URI overload of `ThumbClient.FetchThumbAsync`; production compilation checks that contract. Live HTTP size limits, gallery database restoration and the full product UI need separate acceptance coverage.
