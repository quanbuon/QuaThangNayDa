---
type: "query"
date: "2026-11-05T06:43:35.182135+00:00"
question: "Map FoundationDiagnostics initialization, assembly ownership and M0 smoke flow"
contributor: "graphify"
outcome: "useful"
source_nodes: ["FoundationDiagnostics", "RuntimeReport", "ProjectIdentity", "FoundationTools"]
---

# Q: Map FoundationDiagnostics initialization, assembly ownership and M0 smoke flow

## Answer

Expanded graph vocabulary: foundation diagnostics runtime report project identity. EXTRACTED graph confirms MonoBehaviour inheritance, Start constructing RuntimeReport and OnGUI calling DebugProbe. Direct source verification: FoundationTools.CreateSceneIfMissing creates M0 Diagnostics at FoundationTools.cs:88; Bootstrap.unity:561 has its script GUID. BuildBatch explicitly builds Bootstrap despite EditorBuildSettings selecting SampleScene. Runtime Start validates smoke/output flags, waits, writes report and screenshot, then requests exit. Graph paths through shared Unity types are NOT call chains; PS1, asmdef and scene evidence requires direct inspection. Full evidence and limitations: graphify-out/FOUNDATION_MAP.md.

## Outcome

- Signal: useful

## Source Nodes

- FoundationDiagnostics
- RuntimeReport
- ProjectIdentity
- FoundationTools