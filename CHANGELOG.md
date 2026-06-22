# Changelog

## 1.0.3 - 2026-06-22

- Updated exact selection stack dependencies for the accepted stable release line.

## 1.0.2 - 2026-06-22

- Updated exact UI Binding stack dependencies to `com.deucarian.ui-binding` `1.1.0` and `com.deucarian.ui-binding.core-state-integration` `1.0.2`.

## 1.0.1 - 2026-06-17

- Updated suite dependencies, docs, samples, and validation tooling to use Integration package IDs instead of Bridge package IDs.
- Migration: replace old suite manifest references to Core State bridge packages with `com.deucarian.ui-binding.core-state-integration` and `com.deucarian.object-selection.core-state-integration`.

## Unreleased

- Updated the Selection Demo sample to use the stable three-argument `GenericUIContainer<T, TKey>` constructor and sample-only UI row tinting.
- Updated the Selection Demo sample to use ObjectSelection visual strategies for world tint/scale.
- Updated the Selection Demo sample to use GenericUIItems visual strategies for selected UI row tinting.
- Documented optional visual strategies and the separation between selection state and visual representation.

## 1.0.0 - 2026-06-06

- Added the initial JorisHoef Selection Suite package.
- Declared dependencies on Core State, Generic UI Items, Object Selection, and both Core State integration packages.
- Added the `Selection Demo` sample scene for the complete reusable selection stack.
