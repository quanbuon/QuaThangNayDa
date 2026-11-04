using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuaThangNayDa.Editor
{
    /// <summary>Explicit local menu/batch tools. No network, background listener or auto-run.</summary>
    public static class FoundationTools
    {
        public const string BootstrapPath = "Assets/_Project/Scenes/Bootstrap.unity";

        [MenuItem("Qua Thang Nay Da/M0/Create or Open Foundation Scene")]
        public static void OpenFoundation()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            CreateSceneIfMissing();
            EditorSceneManager.OpenScene(BootstrapPath);
        }

        public static void BuildBatch()
        {
            CreateSceneIfMissing();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m0-build-output");
            if (index < 0 || index + 1 >= args.Length)
                throw new ArgumentException("Missing -m0-build-output <exe path>.");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { BootstrapPath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M0] Build {report.summary.result}; errors={report.summary.totalErrors}; " +
                      $"warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("M0 Windows build failed; inspect the Unity build log.");
        }

        private static void CreateSceneIfMissing()
        {
            if (File.Exists(BootstrapPath))
                return; // Never overwrite an existing scene.
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Art/Materials");
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0, 3, -8), Quaternion.Euler(12, 0, 0));
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.58f, 0.74f, 0.80f);

            var lightObject = new GameObject("Morning Key", typeof(Light));
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.color = new Color(1f, 0.92f, 0.8f);
            lightObject.transform.rotation = Quaternion.Euler(45, -25, 0);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader was not found.");
            const string materialPath = "Assets/_Project/Art/Materials/M0Surface.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { color = new Color(0.63f, 0.70f, 0.64f) };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Diagnostic Platform - Not Final Art";
            floor.transform.localScale = new Vector3(12, 0.2f, 12);
            floor.GetComponent<Renderer>().sharedMaterial = material;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Render Check";
            marker.transform.position = new Vector3(2.7f, 0.9f, 1.2f);
            marker.transform.localScale = new Vector3(1, 1.6f, 1);
            marker.GetComponent<Renderer>().sharedMaterial = material;
            new GameObject("M0 Diagnostics", typeof(FoundationDiagnostics));
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);
            if (!EditorSceneManager.SaveScene(scene, BootstrapPath))
                throw new IOException("Could not save the M0 scene.");
            AssetDatabase.SaveAssets();
            Debug.Log("[M0] Created foundation scene; original SampleScene is unchanged.");
        }
    }
}
