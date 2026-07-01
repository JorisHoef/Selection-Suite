# Deucarian Selection Suite

## What this is

`com.deucarian.selection-suite` installs the complete reusable selection stack in one curated UPM package.

This package is a bundle, not a new selection system. It intentionally avoids runtime code and lets the underlying packages keep their responsibilities:

- ObjectSelection owns world-object selection.
- CoreState owns application and data selection.
- UIBinding owns data-driven UI item rendering.
- The integration packages synchronize both sides through shared keys.
- Visual strategies are optional and decide how selected, normal, or hovered states look.

Current package version: `1.0.3`.

## When to use it

- You want the complete reusable selection stack installed together.
- You want Core State, UI Binding, Object Selection, and both Core State integration packages.
- You want the full selection demo sample for world-object selection, data selection, and UI selection.

## When not to use it

- Do not use this package as a new runtime framework; it has no runtime assembly.
- Do not put member-package implementation logic here.
- Do not use the suite when you only need one member package.

## Dependency Graph

```text
ObjectSelection
  |
  v
ObjectSelection-CoreState Integration
  |
  v
CoreState
  |
  v
UIBinding-CoreState Integration
  |
  v
UIBinding
```

CoreState remains the source of truth for application/data selection. The suite does not add a 3-way integration.

## Install

Install through the Deucarian Package Installer by choosing `Selection Suite` in the `Suites` category.

Stable:

```json
"com.deucarian.selection-suite": "https://github.com/Deucarian/Selection-Suite.git#main"
```

Development:

```json
"com.deucarian.selection-suite": "https://github.com/Deucarian/Selection-Suite.git#develop"
```

When installing through the Package Installer, the registered dependency packages are installed first.

## Unity compatibility

Requires Unity 2021.3 or newer.

## Samples

Import `Selection Demo` manually through Unity Package Manager.

The sample scene demonstrates both directions:

```text
Click world object
-> ObjectSelection selects key
-> ObjectSelection-CoreState integration updates CoreState
-> UIBinding-CoreState integration updates UI selection
```

```text
Click/select UI item
-> UIBinding/CoreState selection changes
-> ObjectSelection-CoreState integration updates ObjectSelection
-> world object highlights
```

The demo uses four stable string IDs:

- `cube`
- `sphere`
- `capsule`
- `cylinder`

The sample creates a small UGUI list, a CoreState repository with matching data items, current and previous selection displays, and a simple world-object highlight.

The demo keeps state and visuals separate. ObjectSelection and CoreState decide what key is selected, while sample-only visual code decides how selected world objects and UI rows look. More advanced visuals such as DOTween, custom tween packages, outline renderers, shader effects, Animator states, or VFX can be layered on later without changing the selection stack.

## Runtime

The suite has no runtime assembly. Use the runtime APIs from the dependency packages directly.

## Dependency Versions

This package version depends on `com.deucarian.ui-binding` `1.1.0` and `com.deucarian.ui-binding.core-state-integration` `1.0.3`. Common is resolved transitively through UI Binding; Selection Suite does not reference `com.deucarian.common` directly.

## Troubleshooting

- If the suite installs without expected behavior, verify the member packages are installed and imported samples are present.
- If sample selection does not synchronize, confirm both integration packages are installed and the sample scene is using matching keys.
- If you need custom behavior, extend the member packages or project code rather than adding implementation logic to this suite.

## Validation

Run the shared package validator from the repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Architecture / Contributor Notes

- [AGENTS.md](AGENTS.md) contains repository-specific ownership and Codex guidance.
- Deucarian architecture rules live in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md).
- Capability ownership is tracked in [CAPABILITY_OWNERSHIP.md](https://github.com/Deucarian/Package-Registry/blob/develop/CAPABILITY_OWNERSHIP.md).

## License

See [LICENSE.md](LICENSE.md).

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `Selection Demo` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.
