# Architecture Notes

LAltKey is intentionally split from AltKey as a separate product. The first version copies proven accessibility and tooling infrastructure, then removes Korean-specific input code.

## Runtime

`EnglishInputModule` receives `KeySlot` input from `KeyboardViewModel`, updates the current prefix, emits suggestions through `AutoCompleteService`, and learns words through `EnglishDictionary` when input is committed.

## Tools

`LAltKey.Tools` runs as a separate process. It receives `--data-dir` from the main app so both processes edit the same config, layouts, and learned dictionary files. Reload notifications use LAltKey-specific event names.

## Future Shared Work

Do not create a common library until repeated porting cost is clear. For now, implement shared candidates in one repository, link the source PR from a porting issue, and adapt the behavior intentionally for the target product.