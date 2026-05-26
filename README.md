# LArtKey

LArtKey is an English-only Windows on-screen keyboard derived from AltKey. It keeps the accessibility, switch scanning, word prediction, clipboard, AI actions, and external editor tooling, while removing the Korean composition engine and Korean-specific layouts.

## Features

- English text input with Unicode output and word prediction.
- User-learned word frequency and bigram prediction.
- Accessible keyboard navigation, switch scanning, high contrast themes, large text scaling, and optional key readout.
- Layout editor, user dictionary editor, header shortcut editor, profile mapping editor, and AI prompt editor in `LArtKey.Tools`.
- Portable ZIP and installer-based distribution.

## Projects

- `LArtKey/` - main WPF keyboard app.
- `LArtKey.Tools/` - standalone editor tools.
- `LArtKey.Tests/` - xUnit regression tests.
- `installer/` - Inno Setup script.
- `.github/workflows/` - CI and release workflows.

## Build

```powershell
dotnet build LArtKey.Tools/LArtKey.Tools.csproj -v minimal
dotnet test LArtKey.Tests/LArtKey.Tests.csproj
```

## Product Separation

LArtKey uses its own executable names, application data folder, mutex, reload events, installer names, and GitHub release endpoint. It can be installed alongside AltKey without sharing settings or learned dictionary data.

Shared feature work should be tracked intentionally. Label issues as `lartkey-only`, `altkey-port-candidate`, or `shared-candidate`; port only the changes that make sense for the other product.