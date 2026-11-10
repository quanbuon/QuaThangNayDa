# Graph Report - QuaThangNayda  (2026-11-05)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 704 nodes · 1325 edges · 68 communities (42 shown, 26 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 55 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `99f1aaaf`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31
- Community 32
- Community 34
- Community 35
- Community 36
- Community 37
- Community 38
- Community 39
- Community 40
- Community 41
- Community 42
- Community 43
- Community 44
- Community 45
- Community 46
- Community 47
- Community 48
- Community 49
- Community 50
- Community 51
- Community 52
- Community 53
- Community 54
- Community 55
- Community 56
- Community 57
- Community 58
- Community 59
- Community 60
- Community 61
- Community 62
- Community 63
- Community 64
- Community 65

## God Nodes (most connected - your core abstractions)
1. `MorningSimulation` - 43 edges
2. `MorningSession` - 26 edges
3. `TravelController` - 26 edges
4. `ArcadeScooter` - 25 edges
5. `ActivityStation` - 22 edges
6. `QuaThangNayDa.Core` - 20 edges
7. `MorningActivity` - 19 edges
8. `TravelInput` - 17 edges
9. `ShiftCommitment` - 17 edges
10. `QuaThangNayDa.Editor` - 16 edges

## Surprising Connections (you probably didn't know these)
- `ActivityStation` --references--> `MorningActivity`  [EXTRACTED]
  Assets/_Project/Scripts/Runtime/Activities/ActivityStation.cs → Assets/_Project/Scripts/Core/MorningSimulation.cs
- `MorningSession` --references--> `ActivityStation`  [EXTRACTED]
  Assets/_Project/Scripts/Runtime/Activities/MorningSession.cs → Assets/_Project/Scripts/Runtime/Activities/ActivityStation.cs
- `MorningSnapshot` --references--> `MorningActivity`  [EXTRACTED]
  Assets/_Project/Scripts/Core/MorningSnapshot.cs → Assets/_Project/Scripts/Core/MorningSimulation.cs
- `TravelController` --references--> `TravelInput`  [EXTRACTED]
  Assets/_Project/Scripts/Runtime/Player/TravelController.cs → Assets/_Project/Scripts/Runtime/Player/TravelInput.cs
- `TravelController` --references--> `ArcadeScooter`  [EXTRACTED]
  Assets/_Project/Scripts/Runtime/Player/TravelController.cs → Assets/_Project/Scripts/Runtime/Vehicles/ArcadeScooter.cs

## Import Cycles
- None detected.

## Communities (68 total, 26 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.10
Nodes (11): ActivityTargetingChecks, ArtDetailTools, ArtSliceTools, BikeVendorTools, CafeManagerTools, CampusTools, MorningTools, StoryTools (+3 more)

### Community 1 - "Community 1"
Cohesion: 0.12
Nodes (4): ProjectIdentity, QuaThangNayDa.Core, QuaThangNayDa, QuaThangNayDa.Editor

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (12): MorningSession, HasStarted, Simulation, SaveCoordinator, FoundationDiagnostics, RuntimeReport, SaveGame, Envelope (+4 more)

### Community 3 - "Community 3"
Cohesion: 0.05
Nodes (43): dependencies, depth, source, version, dependencies, depth, source, version (+35 more)

### Community 4 - "Community 4"
Cohesion: 0.05
Nodes (43): dependencies, depth, source, version, dependencies, depth, source, url (+35 more)

### Community 5 - "Community 5"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 6 - "Community 6"
Cohesion: 0.12
Nodes (12): CommitmentChoice, CoverShift, None, StayInClass, ShiftCommitment, Active, Choice, Deadline (+4 more)

### Community 7 - "Community 7"
Cohesion: 0.11
Nodes (18): com.unity.modules.animation@1.0.0, com.unity.modules.assetbundle@1.0.0, com.unity.modules.audio@1.0.0, com.unity.modules.imageconversion@1.0.0, com.unity.modules.jsonserialize@1.0.0, com.unity.modules.physics@1.0.0, com.unity.modules.physics2d@1.0.0, com.unity.modules.uielements@1.0.0 (+10 more)

### Community 8 - "Community 8"
Cohesion: 0.11
Nodes (16): MorningSimulation, Activity, ClassMinutes, Commitment, Ended, Energy, Health, Journal (+8 more)

### Community 9 - "Community 9"
Cohesion: 0.13
Nodes (4): MorningConfig, ReadmeEditor, Readme, Section

### Community 10 - "Community 10"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 11 - "Community 11"
Cohesion: 0.13
Nodes (14): com.unity.ide.rider@3.1.0, com.unity.ide.visualstudio@2.0.28, com.unity.inputsystem@1.20.1, com.unity.modules.terrainphysics@1.0.0, com.unity.modules.tilemap@1.0.0, com.unity.modules.umbra@1.0.0, com.unity.modules.vehicles@1.0.0, com.unity.ide.rider (+6 more)

### Community 12 - "Community 12"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 13 - "Community 13"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 14 - "Community 14"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 15 - "Community 15"
Cohesion: 0.18
Nodes (8): MorningActivity, Breakfast, CoverShift, None, Rest, Study, Work, MorningChecks

### Community 16 - "Community 16"
Cohesion: 0.15
Nodes (14): dependencies, com.unity.modules.ai@1.0.0, com.unity.modules.cloth@1.0.0, com.unity.modules.ui@1.0.0, com.unity.modules.unityanalytics@1.0.0, com.unity.modules.unitywebrequesttexture@1.0.0, com.unity.modules.xr@1.0.0, com.unity.render-pipelines.universal@17.6.0 (+6 more)

### Community 17 - "Community 17"
Cohesion: 0.15
Nodes (7): TravelInput, Brake, Interact, Look, Move, Pause, Recover

### Community 18 - "Community 18"
Cohesion: 0.17
Nodes (5): ArcadeScooter, CollisionCount, LastImpactTime, Mounted, SpeedKph

### Community 19 - "Community 19"
Cohesion: 0.35
Nodes (3): MorningRules, MorningSnapshot, SaveChecks

### Community 20 - "Community 20"
Cohesion: 0.25
Nodes (5): TravelController, ControlsBlocked, IsPaused, IsRiding, OverlayOpen

### Community 21 - "Community 21"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 23 - "Community 23"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 24 - "Community 24"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 25 - "Community 25"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 26 - "Community 26"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 29 - "Community 29"
Cohesion: 0.29
Nodes (7): CommitmentState, Active, AwaitingChoice, Completed, Failed, NotOffered, Unavailable

### Community 31 - "Community 31"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.rider

### Community 32 - "Community 32"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.inputsystem

### Community 34 - "Community 34"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.ai

### Community 35 - "Community 35"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.assetbundle

### Community 36 - "Community 36"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.audio@1.0.0

### Community 37 - "Community 37"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.particlesystem

### Community 38 - "Community 38"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.tetgen

### Community 39 - "Community 39"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.umbra

### Community 40 - "Community 40"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unityanalytics

### Community 41 - "Community 41"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequest@1.0.0

### Community 42 - "Community 42"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestassetbundle

### Community 43 - "Community 43"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestaudio

### Community 44 - "Community 44"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequesttexture

### Community 45 - "Community 45"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestwww

### Community 46 - "Community 46"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.video

## Knowledge Gaps
- **55 isolated node(s):** `Breakfast`, `CoverShift`, `None`, `Rest`, `Study` (+50 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 288 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **26 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `dependencies` connect `Community 21` to `Community 3`, `Community 4`, `Community 5`, `Community 7`, `Community 10`, `Community 12`, `Community 13`, `Community 14`, `Community 23`, `Community 24`, `Community 25`, `Community 26`, `Community 31`, `Community 32`, `Community 34`, `Community 35`, `Community 36`, `Community 37`, `Community 38`, `Community 39`, `Community 40`, `Community 41`, `Community 42`, `Community 43`, `Community 44`, `Community 45`, `Community 46`?**
  _High betweenness centrality (0.218) - this node is a cross-community bridge._
- **Are the 5 inferred relationships involving `MorningSimulation` (e.g. with `MorningRules.Copy()` and `MorningRules.Validate()`) actually correct?**
  _`MorningSimulation` has 5 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Breakfast`, `CoverShift`, `None` to the rest of the system?**
  _55 weakly-connected nodes found - possible missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.09899749373433583 - nodes in this community are weakly interconnected._
- **Why does `MorningSimulation` connect `Community 8` to `Community 1`, `Community 2`, `Community 6`, `Community 15`, `Community 19`, `Community 22`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.1204644412191582 - nodes in this community are weakly interconnected._
- **Why does `MorningSession` connect `Community 2` to `Community 0`, `Community 1`, `Community 33`, `Community 8`, `Community 9`, `Community 47`, `Community 20`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._