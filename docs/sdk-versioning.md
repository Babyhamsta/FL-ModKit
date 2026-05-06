# SDK Versioning

The SDK version is exposed as the const `SdkInfo.Version` and embedded in the package zip name (`FL-ModKit-v<version>-sdk.zip`).

## Semver promise

We follow [SemVer 2.0](https://semver.org/) with the standard split:

- **Patch (0.x.Y)**: Bug fixes, internal refactors. Public API unchanged. Drop-in replacement.
- **Minor (0.X.0)**: Additive public API changes. Existing call sites still compile and behave the same. New attributes, new helpers, new optional parameters with defaults.
- **Major (X.0.0)**: Breaking public API changes. Migration notes shipped in `CHANGELOG.md` plus a dedicated `docs/migration-X.Y.md` page.

Pre-release suffixes (`-alpha`, `-beta`) are not supported by the manifest validator yet. If you need them, pin `MinSdkVersion` to the previous stable release.

## Mod version constraints

Mods declare their compatibility through `[ModKitManifest(MinSdkVersion = "X.Y.Z")]`. The validator parses with `System.Version.TryParse`, so:

- `0.1` is accepted (interpreted as `0.1.0`).
- `0.1.0` is accepted.
- `0.1.0.0` is accepted (the trailing revision is compared too).
- `0.1.0-alpha` is rejected. Use `0.1.0` and document the alpha requirement separately.

Inter-mod dependencies follow the same parser, with an optional `>=` minimum-version constraint:

```csharp
Dependencies = new[] {
    "babyhamsta.modkit-core",                // any version
    "third-party.helper>=1.0.0",             // 1.0.0 or newer
    "thirdparty.othermod>=1.2"               // parsed as 1.2.0
}
```

The manifest validator enforces these on every register/unregister, so a dependent mod automatically becomes valid the moment its dependency loads.

## Release history

| Version | Date | Highlights |
|---------|------|------------|
| 0.1.0 | 2026-05-05 | Initial public release. |
