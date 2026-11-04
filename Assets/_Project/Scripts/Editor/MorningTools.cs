using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class MorningTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/MorningTest.unity";
        [MenuItem("Qua Thang Nay Da/M2/Create or Open Morning Test")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening M2."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
            EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildBatch()
        {
            MorningChecks.Run();
            Create();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m2-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing -m2-build-output");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M2] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M2 build failed.");
        }
        private static void Create()
        {
            if (File.Exists(ScenePath)) return;
            if (!File.Exists(TravelTools.ScenePath)) throw new FileNotFoundException("Create M1 TravelTest first.");
            var scene = EditorSceneManager.OpenScene(TravelTools.ScenePath);
            Directory.CreateDirectory("Assets/_Project/Data");
            AssetDatabase.Refresh();
            const string configPath = "Assets/_Project/Data/MorningConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<MorningConfig>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<MorningConfig>();
                AssetDatabase.CreateAsset(config, configPath);
            }
            var player = UnityEngine.Object.FindAnyObjectByType<TravelController>();
            if (player == null || player.View == null || player.Scooter == null)
                throw new InvalidOperationException("Travel scene references missing.");
            player.ShowDiagnostics = false;
            var session = new GameObject("Morning Session", typeof(MorningSession)).GetComponent<MorningSession>();
            session.Config = config;
            session.Player = player;
            AddStation("AN SANG", MorningActivity.Breakfast, new Vector3(-4, 1, 0));
            AddStation("LOP HOC", MorningActivity.Study, new Vector3(-5, 1, 7));
            AddStation("CA PHE - LAM", MorningActivity.Work, new Vector3(5, 1, 15));
            AddStation("NGHI NGOI", MorningActivity.Rest, new Vector3(-4, 1, -5));
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save MorningTest.");
            AssetDatabase.SaveAssets();
        }
        private static void AddStation(string label, MorningActivity activity, Vector3 position)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = label;
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(1.5f, 2, 0.3f);
            obj.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M1Scooter.mat");
            var station = obj.AddComponent<ActivityStation>();
            station.Activity = activity;
            station.Label = label;
            var text = new GameObject("Label", typeof(TextMesh)).GetComponent<TextMesh>();
            text.transform.position = position + new Vector3(0, 1.4f, -0.2f);
            text.text = label + "\nF: tuong tac";
            text.fontSize = 48;
            text.characterSize = 0.07f;
            text.anchor = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }
    }
}
