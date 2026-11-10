using System;
using System.Collections;
using System.IO;
using UnityEngine;
using QuaThangNayDa.Core;

namespace QuaThangNayDa
{
    /// <summary>M0 diagnostics only. Not the final gameplay HUD or performance benchmark.</summary>
    public sealed class FoundationDiagnostics : MonoBehaviour
    {
        private int _probeCount;
        private GUIStyle _title;
        private GUIStyle _body;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Debug.Log($"[M0] Started | Unity {Application.unityVersion} | " +
                      $"GPU {SystemInfo.graphicsDeviceName} | {SystemInfo.graphicsDeviceType}");
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-m0-smoke") < 0)
                yield break;

            int outputIndex = Array.IndexOf(args, "-m0-output");
            if (outputIndex < 0 || outputIndex + 1 >= args.Length)
            {
                Debug.LogError("[M0] Smoke mode requires -m0-output <directory>.");
                Application.Quit(2);
                yield break;
            }

            string directory = Path.GetFullPath(args[outputIndex + 1]);
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(3);
            yield return new WaitForEndOfFrame();
            var report = new RuntimeReport
            {
                unity = Application.unityVersion,
                graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                graphicsMemoryMb = SystemInfo.graphicsMemorySize,
                width = Screen.width,
                height = Screen.height,
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                debugBuild = Debug.isDebugBuild,
                renderedFrames = Time.frameCount
            };
            File.WriteAllText(Path.Combine(directory, "runtime.json"), JsonUtility.ToJson(report, true));
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "runtime.png"));
            Debug.Log("[M0] Runtime smoke report written. This is not a gameplay FPS benchmark.");
            yield return new WaitForSecondsRealtime(3);
            Application.Quit(0);
        }

        private void OnGUI()
        {
            _title ??= new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold };
            _body ??= new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            float panelWidth = Mathf.Min(690, Screen.width - 48);
            GUILayout.BeginArea(new Rect(24, 24, panelWidth, 390), GUI.skin.box);
            GUILayout.Space(14);
            GUILayout.Label($"{ProjectIdentity.Title.ToUpperInvariant()} / {ProjectIdentity.Milestone}", _title);
            GUILayout.Label("Foundation check - not gameplay or final art", _body);
            GUILayout.Space(18);
            GUILayout.Label($"Unity: {Application.unityVersion}\nGPU: {SystemInfo.graphicsDeviceName}\n" +
                            $"API: {SystemInfo.graphicsDeviceType}\nResolution: {Screen.width} x {Screen.height}", _body);
            GUILayout.Space(18);
            if (GUILayout.Button("DEBUG PROBE - place a breakpoint in DebugProbe()", GUILayout.Height(40)))
                DebugProbe();
            GUILayout.Label($"Probe count: {_probeCount}", _body);
            GUILayout.Label("M1 adds walking and scooter controls. This scene only checks rendering, input and debugging.", _body);
            GUILayout.EndArea();
        }

        public void DebugProbe()
        {
            _probeCount++; // Place a VS Code breakpoint here, then click DEBUG PROBE.
            Debug.Log($"[M0] Debug probe {_probeCount}");
        }

        [Serializable]
        private sealed class RuntimeReport
        {
            public string unity;
            public string graphicsDevice;
            public string graphicsApi;
            public int graphicsMemoryMb;
            public int width;
            public int height;
            public string scene;
            public bool debugBuild;
            public int renderedFrames;
        }
    }
}
