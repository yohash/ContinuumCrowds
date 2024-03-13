# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.4] - 2024-03-13

### Added

- Created a new Tile Interface `IContinuumTile` to replace direct Tile dependency, and allow implementations some flexibility
- Added a defensive maneuver in `DynamicGlobalFields.UpdateTile()` to ensure unit dictionary contains the key before accessing

### Removed

- Removed the `updateId` as an input to `DynamicGlobalFields.UpdateTile()`, and allow the caller to track update IDs on their tiles locally
- Removed `Tile.MarkComplete()` and `Tile.ShouldUpdate()` as derived dependencies, unecessary for completeness of this core algorithm

### Changed

- Changed all `TileExtension` over to use the new `IContinuumTile`

## [0.3.3] - 2024-03-13

### Removed

- Removed two more unused scripts `Portal.cs` and `IPathable.cs`.

### Changed

- Converted several files over to share the `Yohash.ContinuumCrowds` common namespace.

## [0.3.2] - 2024-03-11

### Removed

- Removed `Path`, as it was not a dependency of any scripts in the core algorithm. This put limitations on package usage.

## [0.3.1] - 2024-03-07

### Changed

- Renamed `IUnit` to `IContinuumUnit`,  to recover some contextual clarity
- Removed several unused variables in `IContinuumUnit` whose dependencies left when `Unit.cs` and `Solution.cs` were removed

## [0.3.0] - 2024-03-07

### Changed

- Changes all references to `Unit` in `DynamicGlobalFields` to `IUnit`, as was the intent of the interface

### Removed

- Removed `Unit.cs` in effort to reduce unecessary dependencies
- Removed `Solution.cs` in effort to reduce unecessary dependencies
- Removed variable "Constants.m_driverSeatOffset"

## [0.2.7] - 2024-03-06

### Changed

- Changed `IUnit.UniqueId` to `IUnit.Id` for brevity
- Changed `DynamicGlobalFields` flow speed calculation to clamp to 0, as per the reference text

## [0.2.6] - 2024-03-04

### Added

- Added `IUnit` parameter for unit mass
- Reference documentation

### Changed

- Changed `IUnit` methods to parameters for cleaner appearance
- Scaled the density field by a unit's mass


### Fixed

- Modified the density value computation method to ensure stable simulations, and meet requirements posed in the reference

## [0.2.5] - 2024-03-04

### Added

- Added a handle in `Unit.DestinationReached()` to set status appropriately
- Added self-null check in `Path` before grabbing `Value` from the linked list of destinations

### Changed

- Changed `UnitPath` to `Path` to lessen name redundancy

## [0.2.4] - 2024-03-01

### Fixed

- Corrected a flawed method to convert global points to local tile coords in `DynamicGlobalFields`.

## [0.2.3] - 2024-03-01

### Fixed

- Corrected a bug that incorrectly computed a tile's indeces from global coordinates by ensuring we weight the corners.

## [0.2.2] - 2024-03-01

### Fixed

- Corrected logic in `DynamicGlobalFields` that incorrectly computed a tile's corner, and instead reference the variable in the `Tile` class itself.

## [0.2.1] - 2024-03-01

### Fixed

- Corrected logic in `DynamicGlobalFields` that assumed square tiles to consider both x and y dimensions.

## [0.2.0] - 2024-02-26

### Changed

- The `CCValues` static reference is no longer hard-coded to the `Resources/` file-system. Rather, the `public static CCValues Instance` is left open. The user may define it from a local `ScriptableObject` instance, or leave it blank, in which case any call to the singleton will return the class `default`

- All classes and associated scripts had any prepending `CC` script clarifiers removed. Instead, we will allow contextual information to infer the namespace membership. (ie. `ContinuumCrowds.EikonalSolver` clearly references the solver for the continuum crowds system)