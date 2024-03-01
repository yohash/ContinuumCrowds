# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.2] - 2024-03-01

### Changed

- Modified logic in `DynamicGlobalFields` that incorrectly computed a tile's corner, and instead reference the variable in the `Tile` class itself.

## [0.2.1] - 2024-03-01

### Changed

- Modified logic in `DynamicGlobalFields` that assumed square tiles to consider both x and y dimensions.

## [0.2.0] - 2024-02-26

### Changed

- The `CCValues` static reference is no longer hard-coded to the `Resources/` file-system. Rather, the `public static CCValues Instance` is left open. The user may define it from a local `ScriptableObject` instance, or leave it blank, in which case any call to the singleton will return the class `default`

- All classes and associated scripts had any prepending `CC` script clarifiers removed. Instead, we will allow contextual information to infer the namespace membership. (ie. `ContinuumCrowds.EikonalSolver` clearly references the solver for the continuum crowds system)