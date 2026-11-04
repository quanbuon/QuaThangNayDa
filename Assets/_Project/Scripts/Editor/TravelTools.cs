using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class TravelTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/TravelTest.unity";

        [MenuItem("Qua Thang Nay Da/M1/Create or Open Travel Test")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening M1."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void BuildBatch()
        {
            Create();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m1-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing -m1-build-output");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M1] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M1 build failed.");
        }

        private static Material Material(string name, Color color)
        {
            string path = $"Assets/_Project/Art/Materials/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(result, path);
            return result;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 size, Material material, Transform parent = null)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = size;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private static void Create()
        {
            if (File.Exists(ScenePath)) return;
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Art/Materials");
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var road = Material("M1Road", new Color(0.22f, 0.26f, 0.28f));
            var walls = Material("M1Buildings", new Color(0.72f, 0.66f, 0.51f));
            var vehicle = Material("M1Scooter", new Color(0.12f, 0.45f, 0.43f));
            Box("Ground", new Vector3(0, -0.25f, 10), new Vector3(30, 0.5f, 60), road);
            Box("West boundary", new Vector3(-15, 1, 10), new Vector3(1, 2, 60), walls);
            Box("East boundary", new Vector3(15, 1, 10), new Vector3(1, 2, 60), walls);
            Box("North boundary", new Vector3(0, 1, 40), new Vector3(30, 2, 1), walls);
            Box("South boundary", new Vector3(0, 1, -20), new Vector3(30, 2, 1), walls);
            for (int i = 0; i < 4; i++)
            {
                Box("Building left " + i, new Vector3(-10, 2, -8 + i * 12), new Vector3(6, 4, 8), walls);
                Box("Building right " + i, new Vector3(10, 2, -8 + i * 12), new Vector3(6, 4, 8), walls);
            }
            Box("Brake test obstacle", new Vector3(0, 0.6f, 22), new Vector3(3, 1.2f, 1), walls);
            var light = new GameObject("Morning light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.color = new Color(1, 0.94f, 0.82f);
            light.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);

            var scooterObject = new GameObject("Scooter - prototype", typeof(BoxCollider), typeof(Rigidbody), typeof(ArcadeScooter));
            scooterObject.transform.position = new Vector3(0, 0.55f, 0);
            scooterObject.GetComponent<BoxCollider>().size = new Vector3(0.65f, 0.9f, 1.8f);
            var scooter = scooterObject.GetComponent<ArcadeScooter>();
            var body = Box("Body", Vector3.zero, new Vector3(0.6f, 0.6f, 1.6f), vehicle, scooterObject.transform);
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
            var bars = Box("Handlebar", new Vector3(0, 0.6f, 0.55f), new Vector3(0.85f, 0.08f, 0.12f), road, scooterObject.transform);
            UnityEngine.Object.DestroyImmediate(bars.GetComponent<Collider>());
            scooter.Seat = new GameObject("First person seat").transform;
            scooter.Seat.SetParent(scooterObject.transform, false);
            scooter.Seat.localPosition = new Vector3(0, 1.1f, -0.35f);
            scooter.SafeSpot = new GameObject("Scooter recovery point").transform;
            scooter.SafeSpot.position = scooterObject.transform.position;

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.fieldOfView = 70;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.58f, 0.74f, 0.8f);
            var player = new GameObject("Player", typeof(CharacterController), typeof(TravelInput), typeof(TravelController));
            player.transform.position = new Vector3(-2, 0.1f, -2);
            var character = player.GetComponent<CharacterController>();
            character.height = 1.8f;
            character.radius = 0.3f;
            character.center = new Vector3(0, 0.9f, 0);
            character.stepOffset = 0.25f;
            var travel = player.GetComponent<TravelController>();
            travel.View = camera;
            travel.Scooter = scooter;
            camera.transform.position = player.transform.position + Vector3.up * 1.65f;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save TravelTest scene.");
            AssetDatabase.SaveAssets();
        }
    }
}
