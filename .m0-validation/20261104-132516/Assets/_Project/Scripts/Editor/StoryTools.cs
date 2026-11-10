using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class StoryTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/StoryMorning.unity";
        [MenuItem("Qua Thang Nay Da/M3/Create or Open Phone Story")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening M3."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
            EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildBatch()
        {
            MorningChecks.Run();
            CommitmentChecks.Run();
            Create();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m3-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing -m3-build-output");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M3] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M3 build failed.");
        }
        private static void Create()
        {
            if (File.Exists(ScenePath)) return;
            if (!File.Exists(MorningTools.ScenePath)) throw new FileNotFoundException("Create M2 first.");
            var scene = EditorSceneManager.OpenScene(MorningTools.ScenePath);
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            if (session == null || session.Player == null || session.Config == null) throw new InvalidOperationException("M2 references missing.");
            const string configPath = "Assets/_Project/Data/StoryMorningConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<MorningConfig>(configPath);
            if (config == null)
            {
                config = UnityEngine.Object.Instantiate(session.Config);
                config.EnableStory = true;
                AssetDatabase.CreateAsset(config, configPath);
            }
            session.Config = config;
            var phone = new GameObject("Student Phone", typeof(StudentPhone)).GetComponent<StudentPhone>();
            phone.Session = session;
            session.Phone = phone;
            var manager = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            manager.name = "Cafe manager - placeholder NPC";
            manager.transform.position = new Vector3(4, 1, 12);
            manager.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M1Scooter.mat");
            var station = manager.AddComponent<ActivityStation>();
            station.Activity = MorningActivity.CoverShift;
            station.Label = "QUAN LY - LAM THAY";
            var label = new GameObject("Manager label", typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.position = manager.transform.position + new Vector3(0, 1.4f, -0.2f);
            label.text = "QUAN LY\nF: gap va lam thay";
            label.fontSize = 48;
            label.characterSize = 0.07f;
            label.anchor = TextAnchor.MiddleCenter;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save M3.");
            AssetDatabase.SaveAssets();
        }
    }
}
