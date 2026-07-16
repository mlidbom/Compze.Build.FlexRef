# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

## [0.8.0] - 2026-07-16

### Added
- `<ExcludeDirectory Path="..." />` config element. Directories listed this way are skipped by both the
  project and solution scans. This lets a repository point FlexRef away from a location that duplicates
  projects already present elsewhere in the tree — most commonly a directory junction or symbolic link that
  resolves back inside the repository (for example a docs website that junctions documented projects in for
  co-located docs). Without it, each such project is discovered twice: FlexRef injects a spurious
  `ProjectReference` into every consumer and rewrites the duplicated project's references relative to the
  second location. Genuine symbolic links and junctions in directories that are not excluded remain fully
  scannable.

## [0.7.0] - 2026-05-29

### Changed
- Migrated the CLI tool to .NET 10 (from .NET 8) and updated `Microsoft.Build` to 18.x. Running the
  tool now requires the **.NET 10 SDK** instead of the .NET 8 SDK. This fixes
  `InvalidOperationException: No instances of MSBuild could be detected` on machines that have the
  .NET 10 SDK but no .NET 8 SDK. `RollForward=Major` is enabled so the tool keeps working on machines
  that only have a newer-major SDK installed. The framework a project targets is unaffected — FlexRef
  still operates on solutions whose projects target any framework.

## [0.6.2] - 2026-03-05

### Fixed
- Documented NCrunch limitation

## [0.6.1] - 2026-03-01

### Fixed
- Added package icon
- Readme updates

## [0.6.0] - 2026-02-28

### Changed
- Fix intermittent problem caused by NCrunch temporary csproj files

## [0.5.0-alpha.1] - 2026-02-22

### Changed
- Refactored into a dotnet tool that makes everything automatic

## [0.2.0-alpha.3] - 2026-02-21

### Changed
- Renamed to Compze.Build.FlexRef

## [0.2.0-alpha.2] - 2026-02-21

### Added
- Updates to readme and internal structure updates.

## [0.2.0-alpha.1] - 2026-02-21

### Added
- Initial public release
- Solution-aware `.slnx` parsing — auto-detects which projects are in the current solution
- Conditional `PackageReference` / `ProjectReference` switching based on solution membership
- NCrunch compatibility via `CustomBuildProperties` flags
- CLI / CI override support via `/p:` properties or environment variables
- Example workspace with `Acme.Full.slnx` and `Acme.AppOnly.slnx` demonstrating both modes
- NuGet content package distributing the `.props` file
- GitHub Actions CI and publish workflows
