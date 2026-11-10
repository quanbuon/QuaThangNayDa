# FoundationDiagnostics — architecture and M0 smoke flow

Verified 2026-11-05 against the current working tree. No Unity source or scene was modified. No Unity build or player smoke test was executed in this analysis.

## Graph scope and evidence

Graphify 0.9.102 was run from the local repository with `extract --code-only`: 43 code files, 704 nodes, 1,325 edges, 68 communities. Semantic extraction cost: 0 input / 0 output tokens (this does not include the assistant conversation).

The scan excluded Repo, Library, Temp, Logs, obj, .vs, UserSettings, graphify-out, .m0-validation, Build and Builds. It also respected Git ignore rules. It skipped 13 non-code files and 202 unclassified files. The two PowerShell build scripts yielded no symbols. Scene, meta and assembly-definition relationships below were verified directly, not inferred from the graph.

The post-build integrity diagnostic found no missing/dangling endpoints, self-loops or duplicate edges. This does not prove extraction completeness or detect relationships lost before the graph was persisted. Community names remain automatic placeholders.

Graph-confirmed EXTRACTED relationships:

- FoundationDiagnostics inherits MonoBehaviour; contains Awake, Start, OnGUI, DebugProbe and nested RuntimeReport.
- Start calls the RuntimeReport constructor (source line 41).
- OnGUI calls DebugProbe (source line 74).

The shortest undirected path from CreateSceneIfMissing to FoundationDiagnostics traverses Camera, TravelController and MonoBehaviour. This is shared-type connectivity, NOT a call flow. The direct scene creation and ProjectIdentity member accesses are missing as direct graph edges; source evidence takes precedence.

## Initialization and assemblies

1. [Build-M0.ps1](../Tools/Build-M0.ps1), lines 8–21, copies Assets/Packages/ProjectSettings to an isolated validation project and launches Unity with `-executeMethod QuaThangNayDa.Editor.FoundationTools.BuildBatch`.
2. [FoundationTools.BuildBatch](../Assets/_Project/Scripts/Editor/FoundationTools.cs), line 25, ensures the scene exists, then builds Windows x64 Development + AllowDebugging with an explicit scene list containing Bootstrap.
3. [FoundationTools.CreateSceneIfMissing](../Assets/_Project/Scripts/Editor/FoundationTools.cs), line 47, returns without modifying an existing scene. When creating one, line 88 creates `new GameObject("M0 Diagnostics", typeof(FoundationDiagnostics))` and saves it.
4. [Bootstrap.unity](../Assets/_Project/Scenes/Bootstrap.unity), lines 546–563, already contains an active M0 Diagnostics object with an enabled component referencing script GUID `4a5f0d8c1ebf46c5b2a12ac776b04a00`. This matches [FoundationDiagnostics.cs.meta](../Assets/_Project/Scripts/Runtime/Bootstrap/FoundationDiagnostics.cs.meta).
5. Loading that scene instantiates its serialized component; Unity invokes its lifecycle methods. The editor creation method is not a runtime bootstrap service.

Assembly ownership:

- [QuaThangNayDa.Editor.asmdef](../Assets/_Project/Scripts/Editor/QuaThangNayDa.Editor.asmdef): Editor-only; references Runtime and Core; contains FoundationTools.
- [QuaThangNayDa.Runtime.asmdef](../Assets/_Project/Scripts/Runtime/QuaThangNayDa.Runtime.asmdef): contains FoundationDiagnostics; references Core and Unity.InputSystem. FoundationDiagnostics itself does not directly call Input System APIs.
- [QuaThangNayDa.Core.asmdef](../Assets/_Project/Scripts/Core/QuaThangNayDa.Core.asmdef): noEngineReferences; contains [ProjectIdentity](../Assets/_Project/Scripts/Core/ProjectIdentity.cs), whose Title and Milestone are displayed by the diagnostics UI.

Important: [EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset), line 9, currently lists SampleScene, not Bootstrap. The explicit scene list in BuildBatch is what makes the custom M0 build use Bootstrap.

## Runtime flow

Source: [FoundationDiagnostics.cs](../Assets/_Project/Scripts/Runtime/Bootstrap/FoundationDiagnostics.cs).

- Awake (line 16): requests targetFrameRate 60 and logs Unity/GPU/API. This is a target, not a measured FPS guarantee.
- Start (line 23): reads process command-line arguments.
  - Without `-m0-smoke`: ends the coroutine; the diagnostic UI remains available.
  - With `-m0-smoke` but missing `-m0-output` or its following argument: logs an error and requests exit code 2.
  - Otherwise: resolves the output directory and creates it; waits 3 realtime seconds, then WaitForEndOfFrame.
  - Creates RuntimeReport with Unity version, GPU, graphics API/memory, resolution, active scene, debug-build status and frame count.
  - Writes `runtime.json`, requests `runtime.png` screenshot, waits another 3 realtime seconds, then requests exit code 0 (lines 53–57).
- OnGUI (line 60): displays identity/hardware details and the debug button. DebugProbe (line 80) increments a counter and logs it.

Build-M0.ps1 only builds the player; it does not launch the smoke test. A player smoke invocation needs `-m0-smoke -m0-output <directory>` and graphics rendering, because the coroutine waits for end-of-frame and requests a screenshot.

The code does not catch filesystem exceptions or explicitly verify that the screenshot was successfully written before requesting exit 0. A real smoke check should inspect the player exit code, log and both output files. No runtime success is claimed here.

## Reusing the local tool

Run from the Unity project root in PowerShell (no global installation required):

```powershell
& '.\Repo\graphify-8\.venv\Scripts\graphify.exe' query 'foundation diagnostics' --budget 2500
& '.\Repo\graphify-8\.venv\Scripts\graphify.exe' explain 'FoundationDiagnostics' --all
& '.\Repo\graphify-8\.venv\Scripts\graphify.exe' explain 'FoundationDiagnostics.Start()' --all
```

Rebuild/update the code graph using the same exclusions:

```powershell
& '.\Repo\graphify-8\.venv\Scripts\graphify.exe' extract . --code-only --max-workers 2 --exclude 'Repo/**' --exclude 'Library/**' --exclude 'Temp/**' --exclude 'Logs/**' --exclude 'obj/**' --exclude '.vs/**' --exclude 'UserSettings/**' --exclude 'graphify-out/**' --exclude '.m0-validation/**' --exclude 'Build/**' --exclude 'Builds/**'
& '.\Repo\graphify-8\.venv\Scripts\graphify.exe' cluster-only . --no-label
```

Artifacts: [interactive graph](graph.html), [graph data](graph.json), [automated report](GRAPH_REPORT.md). The graph reflects uncommitted working-tree content, not only its stamped commit. Graph outputs are new working-tree files; ignore rules and global agent configuration were not changed.
