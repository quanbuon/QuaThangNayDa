using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    // Authored geometry only: no third-party models, textures or runtime generation.
    public static class ArtSliceTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/ArtMorning.unity";
        private const string Materials = "Assets/_Project/Art/Materials/";

        [MenuItem("Qua Thang Nay Da/M5/Create or Open Art Morning")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening M5."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void BuildBatch()
        {
            MorningChecks.Run(); CommitmentChecks.Run(); SaveChecks.Run(); Create();
            ArtDetailTools.Create();
            CampusTools.Create();
            ActivityTargetingChecks.Run();
            StreetBreakfastTools.Create();
            BikeVendorTools.Create();
            CafeManagerTools.Create();
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-m5-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing M5 output");
            string output = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { CafeManagerTools.ScenePath }, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"[M5] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M5 build failed");
        }

        internal static Material Surface(string name, string hex)
        {
            string path = Materials + "M5" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing");
            ColorUtility.TryParseHtmlString(hex, out Color color);
            material = new Material(shader) { color = color };
            material.SetFloat("_Smoothness", 0.18f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        internal static GameObject Piece(Transform root, string name, Vector3 at, Vector3 size,
            Material material, PrimitiveType shape = PrimitiveType.Cube, bool solid = true)
        {
            var item = GameObject.CreatePrimitive(shape);
            item.name = name;
            item.transform.SetParent(root, false);
            item.transform.localPosition = at;
            item.transform.localScale = size;
            item.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            return item;
        }

        internal static void Sign(Transform root, string text, Vector3 position, float scale)
        {
            var label = new GameObject(text, typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.SetParent(root, false);
            label.transform.localPosition = position;
            label.text = text; label.fontSize = 64; label.characterSize = scale;
            label.anchor = TextAnchor.MiddleCenter; label.color = new Color(1, 0.94f, 0.8f);
        }

        private static void Create()
        {
            if (File.Exists(ScenePath)) return; // Never replace user-edited art.
            if (!File.Exists(SaveTools.ScenePath)) throw new FileNotFoundException("Create M4 first");
            var scene = EditorSceneManager.OpenScene(SaveTools.ScenePath);
            var saves = UnityEngine.Object.FindAnyObjectByType<SaveCoordinator>();
            if (saves == null) throw new InvalidOperationException("M4 save coordinator missing");
            saves.UseArtPreviewSlots = true;
            var cream = Surface("Plaster", "#D7C6A5");
            var teal = Surface("Teal", "#386D69");
            var wood = Surface("Wood", "#986647");
            var orange = Surface("Clay", "#B96945");
            var green = Surface("Leaves", "#55734D");
            var fabric = Surface("Linen", "#E8DCC2");
            var dark = Surface("Metal", "#343C40");
            var floor = Surface("Tile", "#A59F8E");
            for (int side = 0; side < 2; side++)
            {
                bool cafe = side == 1;
                string oldName = cafe ? "Building right 2" : "Building left 0";
                var old = GameObject.Find(oldName);
                if (old == null) throw new InvalidOperationException("Missing expected block: " + oldName);
                UnityEngine.Object.DestroyImmediate(old);
                var root = new GameObject(cafe ? "M5 - Cafe frontage" : "M5 - Student room").transform;
                root.position = new Vector3(cafe ? 10 : -10, 0, cafe ? 16 : -8);
                root.rotation = Quaternion.Euler(0, cafe ? 90 : -90, 0);
                Piece(root, "Floor", new Vector3(0, .025f, 0), new Vector3(7.8f, .05f, 6), floor);
                Piece(root, "Back wall", new Vector3(0, 1.65f, 2.9f), new Vector3(8, 3.3f, .2f), cream);
                foreach (float x in new[] { -3.9f, 3.9f })
                    Piece(root, "Side wall", new Vector3(x, 1.65f, 0), new Vector3(.2f, 3.3f, 6), cream);
                Piece(root, "Roof", new Vector3(0, 3.35f, 0), new Vector3(8.2f, .18f, 6.2f), cream);
                Piece(root, "Front fascia", new Vector3(0, 2.9f, -2.95f), new Vector3(8, .7f, .2f), teal);
                Sign(root, cafe ? "CA PHE  /  GOC SINH VIEN" : "NHA TRO  /  PHONG 01", new Vector3(0, 2.9f, -3.07f), .06f);
                for (int p = 0; p < 2; p++)
                {
                    float x = p == 0 ? -3.3f : 3.3f;
                    Piece(root, "Terracotta planter", new Vector3(x, .25f, -3.4f), new Vector3(.5f, .25f, .5f), orange, PrimitiveType.Cylinder);
                    Piece(root, "Plant crown", new Vector3(x, .85f, -3.4f), new Vector3(.65f, 1.1f, .65f), green, PrimitiveType.Sphere, false);
                }
                if (cafe)
                {
                    Piece(root, "Counter", new Vector3(0, .55f, 1.6f), new Vector3(3.5f, 1.1f, .7f), teal);
                    Piece(root, "Counter top", new Vector3(0, 1.13f, 1.6f), new Vector3(3.7f, .08f, .85f), wood);
                    Piece(root, "Menu board", new Vector3(0, 2, 2.76f), new Vector3(2.2f, 1.1f, .08f), dark, solid: false);
                    Sign(root, "CA PHE SUA\nTRA DAO\nBANH MI", new Vector3(0, 2, 2.7f), .065f);
                    foreach (float x in new[] { -2.5f, 2.5f })
                    {
                        Piece(root, "Table", new Vector3(x, .75f, -.5f), new Vector3(1, .1f, 1), wood);
                        Piece(root, "Table leg", new Vector3(x, .37f, -.5f), new Vector3(.12f, .74f, .12f), dark);
                        foreach (float z in new[] { -1.3f, .3f })
                            Piece(root, "Stool", new Vector3(x, .23f, z), new Vector3(.45f, .46f, .45f), teal);
                    }
                    for (int a = 0; a < 8; a++)
                        Piece(root, "Striped awning", new Vector3(-3.5f + a, 2.55f, -3.5f), new Vector3(1, .07f, 1.2f), a % 2 == 0 ? fabric : orange, solid: false);
                }
                else
                {
                    Piece(root, "Bed frame", new Vector3(-2.4f, .23f, .4f), new Vector3(1.6f, .46f, 2.2f), wood);
                    Piece(root, "Mattress", new Vector3(-2.4f, .53f, .4f), new Vector3(1.55f, .2f, 2.1f), fabric);
                    Piece(root, "Blanket", new Vector3(-2.4f, .65f, -.1f), new Vector3(1.57f, .05f, 1.15f), teal, solid: false);
                    Piece(root, "Pillow", new Vector3(-2.4f, .68f, 1.1f), new Vector3(.8f, .15f, .4f), fabric, solid: false);
                    Piece(root, "Study desk", new Vector3(1.6f, .77f, 2.1f), new Vector3(1.8f, .1f, .75f), wood);
                    foreach (float x in new[] { .85f, 2.35f })
                        Piece(root, "Desk support", new Vector3(x, .38f, 2.1f), new Vector3(.1f, .76f, .65f), wood);
                    Piece(root, "Laptop screen", new Vector3(1.6f, 1.05f, 2.2f), new Vector3(.55f, .4f, .04f), dark, solid: false);
                    Piece(root, "Laptop base", new Vector3(1.6f, .84f, 2), new Vector3(.55f, .04f, .4f), dark, solid: false);
                    Piece(root, "Desk stool", new Vector3(1.6f, .24f, 1.2f), new Vector3(.5f, .48f, .5f), teal);
                }
            }
            var light = GameObject.Find("Morning light").GetComponent<Light>();
            light.color = new Color(1, .91f, .77f); light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(42, -35, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.6f, .7f, .78f);
            RenderSettings.ambientEquatorColor = new Color(.54f, .51f, .44f);
            RenderSettings.ambientGroundColor = new Color(.28f, .26f, .23f);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save M5");
            AssetDatabase.SaveAssets();
        }
    }
}
