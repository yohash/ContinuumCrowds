# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2024-02-26

### Changed

- The `CCValues` static reference is no longer hard-coded to the `Resources/` file-system. Rather, the `public static CCValues Instance` is left open. The user may define it from a local `ScriptableObject` instance, or leave it blank, in which case any call to the singleton will return the class `default`

- All classes and associated scripts had any prepending `CC` script clarifiers removed. Instead, we will allow contextual information to infer the namespace membership. (ie. `ContinuumCrowds.EikonalSolver` clearly references the solver for the continuum crowds system)