# JorisHoef Selection Suite

JorisHoef Selection Suite installs the complete reusable selection stack in one curated UPM package.

This package is a bundle, not a new selection system. It intentionally avoids runtime code and lets the underlying packages keep their responsibilities:

- ObjectSelection owns world-object selection.
- CoreState owns application and data selection.
- GenericUIItems owns data-driven UI item rendering.
- The bridge packages synchronize both sides through shared keys.
- Visual strategies are optional and decide how selected, normal, or hovered states look.

## Package ID

```text
com.jorishoef.selection-suite
```

## Dependency Graph

```text
ObjectSelection
  |
  v
ObjectSelection-CoreState Bridge
  |
  v
CoreState
  |
  v
GenericUIItems-CoreState Bridge
  |
  v
GenericUIItems
```

CoreState remains the source of truth for application/data selection. The suite does not add a 3-way bridge.

## Installation

Install through the JorisHoef Package Installer by choosing `Selection Suite` in the `Suites` category.

You can also add the suite through Unity Package Manager with a Git URL:

```json
{
  "dependencies": {
    "com.jorishoef.selection-suite": "https://github.com/JorisHoef/Selection-Suite.git#main"
  }
}
```

When installing through the Package Installer, the registered dependency packages are installed first.

## Sample

Import `Selection Demo` manually through Unity Package Manager.

The sample scene demonstrates both directions:

```text
Click world object
-> ObjectSelection selects key
-> ObjectSelection-CoreState bridge updates CoreState
-> GenericUIItems-CoreState bridge updates UI selection
```

```text
Click/select UI item
-> GenericUIItems/CoreState selection changes
-> ObjectSelection-CoreState bridge updates ObjectSelection
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
