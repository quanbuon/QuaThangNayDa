using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class SaveTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/SavedMorning.unity";
        [MenuItem("Qua Thang Nay Da/M4/Create or Open Saved Morning")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening M4."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildBatch()
        {
            MorningChecks.Run(); CommitmentChecks.Run(); SaveChecks.Run(); Create();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m4-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing -m4-build-output");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M4] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M4 build failed.");
        }
        private static void Create()
        {
            if (File.Exists(ScenePath)) return;
            if (!File.Exists(StoryTools.ScenePath)) throw new FileNotFoundException("Create M3 first.");
            var scene = EditorSceneManager.OpenScene(StoryTools.ScenePath);
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            if (session == null || session.Phone == null || session.Player == null) throw new InvalidOperationException("M3 references missing.");
            var saves = new GameObject("Save Coordinator", typeof(SaveCoordinator)).GetComponent<SaveCoordinator>();
            saves.Session = session; session.Saves = saves;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save M4 scene.");
            AssetDatabase.SaveAssets();
        }
    }
}
