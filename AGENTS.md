# LArtKey Agent Guide

LArtKey is an English-only Windows on-screen keyboard. All source comments, user-facing strings, documentation, issue templates, and release notes should be written in English.
(However, during the development process, responses must be tailored to the user's language. Ex: Respond to Korean requests in Korean.)

## Priorities

1. Accessibility comes first. Prefer changes that make the keyboard easier to operate, read, scan, or recover from mistakes.
2. Keep LArtKey independent from AltKey. Do not reintroduce Korean composition, Hangul-specific layouts, IME-mode toggles, or shared `%AppData%\AltKey` data paths.
3. Preserve the separate `LArtKey.Tools` process architecture for layout, dictionary, shortcut, AI prompt, and profile editors.
4. Use `NumericAdjuster` for numeric UI settings instead of sliders.
5. Use UTF-8 reads/writes for text files and avoid broad rewrite scripts unless the change is intentionally mechanical.

## Build And Test

- Main build: `dotnet build C:\Users\UITAEK\LArtKey\LArtKey.Tools\LArtKey.Tools.csproj -v minimal`
- Tests: `dotnet test C:\Users\UITAEK\LArtKey\LArtKey.Tests\LArtKey.Tests.csproj`

## English Input Rules

- `EnglishInputModule` is the only input language module.
- `EnglishDictionary`, `WordFrequencyStore("en")`, and `BigramFrequencyStore("en")` own word prediction and learning.
- Korean-only classes such as Hangul composers, Korean dictionaries, and Jamo resolvers must not be restored.

## Distribution Rules

- App data folder: `%AppData%\LArtKey`.
- Main executable: `LArtKey.exe`.
- Tools executable: `LArtKey.Tools.exe`.
- Installer assets: `LArtKey-Setup-v*.exe` and `LArtKey-Portable-v*.zip`.
- Update checks must target the LArtKey GitHub repository.