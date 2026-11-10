using System;
using System.IO;
using System.Linq;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static QuaThangNayDa.Editor.ArtSliceTools;

namespace QuaThangNayDa.Editor
{
    public static class ArtDetailTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/ArtMorningDetail.unity";

        [MenuItem("Qua Thang Nay Da/M5/Open Detailed Morning (latest)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        public static void Create()
        {
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); Validate(); return; }
            var scene = EditorSceneManager.OpenScene(ArtSliceTools.ScenePath);
            var room = GameObject.Find("M5 - Student room").transform;
            var cafe = GameObject.Find("M5 - Cafe frontage").transform;
            var plaster = Surface("Plaster", "#D7C6A5");
            var teal = Surface("Teal", "#386D69");
            var wood = Surface("Wood", "#986647");
            var dark = Surface("Metal", "#343C40");
            var linen = Surface("Linen", "#E8DCC2");
            var brick = Surface("Clay", "#B96945");
            var paving = Surface("Paving", "#B6ADA0");
            var glass = Surface("WindowBlue", "#698D99");

            foreach (var root in new[] { room, cafe })
            {
                Piece(root, "Sidewalk", new Vector3(0, .04f, -3.8f), new Vector3(8, .08f, 1.6f), paving);
                for (int n = 0; n <= 16; n++)
                    Piece(root, "Paving joint", new Vector3(-4 + n * .5f, .082f, -3.8f), new Vector3(.012f, .003f, 1.6f), dark, solid: false);
                foreach (float x in new[] { -3.75f, 3.75f })
                    Piece(root, "Front column", new Vector3(x, 1.45f, -2.9f), new Vector3(.25f, 2.9f, .3f), plaster);
                Piece(root, "Roof lip", new Vector3(0, 3.49f, -.1f), new Vector3(8.25f, .12f, 6.4f), brick);
                for (int n = 0; n < 17; n++)
                    Piece(root, "Roof seam", new Vector3(-4 + n * .5f, 3.56f, -.1f), new Vector3(.035f, .025f, 6.4f), wood, solid: false);
                Piece(root, "Rear skirting", new Vector3(0, .18f, 2.77f), new Vector3(7.6f, .25f, .035f), teal, solid: false);
                Piece(root, "Rain pipe", new Vector3(3.6f, 1.5f, -3.1f), new Vector3(.07f, 1.5f, .07f), dark, PrimitiveType.Cylinder, false);
                // Opaque inset panes; no transparent shader sorting or reflection cost.
                Piece(root, "Window frame", new Vector3(-2.4f, 1.9f, 2.75f), new Vector3(1.8f, 1.15f, .08f), wood, solid: false);
                Piece(root, "Window pane", new Vector3(-2.4f, 1.9f, 2.69f), new Vector3(1.65f, 1, .02f), glass, solid: false);
                Piece(root, "Window mullion", new Vector3(-2.4f, 1.9f, 2.66f), new Vector3(.05f, 1, .03f), linen, solid: false);
                Piece(root, "Window crossbar", new Vector3(-2.4f, 1.9f, 2.66f), new Vector3(1.65f, .05f, .03f), linen, solid: false);
                for (int n = 0; n < 3; n++)
                {
                    var bulb = Piece(root, "Pendant shade", new Vector3(-2.5f + n * 2.5f, 2.5f, .1f), new Vector3(.35f, .09f, .35f), linen, PrimitiveType.Cylinder, false);
                    Piece(root, "Pendant cord", bulb.transform.localPosition + Vector3.up * .4f, new Vector3(.015f, .7f, .015f), dark, solid: false);
                }
            }

            // A real doorway, 1.5 m wide, with an open leaf outside the route.
            Piece(room, "Front wall left", new Vector3(-2.25f, 1.25f, -2.9f), new Vector3(3, 2.5f, .18f), plaster);
            Piece(room, "Front wall right", new Vector3(2.25f, 1.25f, -2.9f), new Vector3(3, 2.5f, .18f), plaster);
            foreach (float x in new[] { -.8f, .8f })
                Piece(room, "Door jamb", new Vector3(x, 1.2f, -3.02f), new Vector3(.08f, 2.4f, .12f), teal);
            Piece(room, "Door header", new Vector3(0, 2.4f, -3.02f), new Vector3(1.7f, .1f, .12f), teal);
            Piece(room, "Open door", new Vector3(.84f, 1.17f, -2.2f), new Vector3(.08f, 2.3f, 1.4f), teal);
            Piece(room, "Door handle", new Vector3(.76f, 1.05f, -1.65f), new Vector3(.12f, .04f, .15f), dark, solid: false);
            Piece(room, "Welcome mat", new Vector3(0, .09f, -3.5f), new Vector3(1.3f, .02f, .6f), wood, solid: false);
            var breakfast = Piece(room, "Breakfast tray", new Vector3(1.05f, .85f, 2), new Vector3(.45f, .04f, .36f), linen);
            Piece(room, "Bread", new Vector3(1.05f, .92f, 2), new Vector3(.3f, .1f, .13f), brick, PrimitiveType.Sphere, false);
            Piece(room, "Tea cup", new Vector3(2.1f, .92f, 2), new Vector3(.12f, .09f, .12f), linen, PrimitiveType.Cylinder, false);
            Piece(room, "Wardrobe", new Vector3(3.25f, 1, 1.65f), new Vector3(.8f, 2, 1.4f), wood);
            MoveActivity(MorningActivity.Breakfast, breakfast, "AN SANG - khay tren ban");
            MoveActivity(MorningActivity.Rest, room.Find("Mattress").gameObject, "NGHI NGOI - giuong");
            MoveActivity(MorningActivity.Work, cafe.Find("Counter").gameObject, "CA PHE - nhan ca tai quay");
            // The counter top must expose the same interaction as the front face.
            var top = cafe.Find("Counter top").gameObject.AddComponent<ActivityStation>();
            top.Activity = MorningActivity.Work; top.Label = "CA PHE - nhan ca tai quay";
            var manager = UnityEngine.Object.FindObjectsByType<ActivityStation>()
                .Single(s => s.Activity == MorningActivity.CoverShift);
            manager.transform.position = cafe.TransformPoint(new Vector3(2.7f, 1, 1.8f));
            var managerLabel = GameObject.Find("Manager label");
            if (managerLabel != null) managerLabel.SetActive(false);
            Sign(cafe, "F: NHAN CA TAI QUAY", new Vector3(0, .8f, 1.235f), .035f);
            Sign(room, "F: AN SANG TREN BAN / NGHI TAI GIUONG", new Vector3(0, 2.65f, -3.08f), .025f);
            var obstacle = GameObject.Find("Brake test obstacle");
            if (obstacle != null) obstacle.SetActive(false);
            Physics.SyncTransforms();
            Validate();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save detailed M5");
            AssetDatabase.SaveAssets();
        }

        private static void MoveActivity(MorningActivity activity, GameObject target, string label)
        {
            var original = UnityEngine.Object.FindObjectsByType<ActivityStation>()
                .Single(s => s.Activity == activity);
            original.gameObject.SetActive(false); // Keep original data available in the new scene.
            var station = target.AddComponent<ActivityStation>();
            station.Activity = activity; station.Label = label;
        }

        private static void Validate()
        {
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            if (session == null || session.Phone == null || session.Saves == null || session.Player == null)
                throw new InvalidOperationException("Detailed M5 missing runtime references");
            foreach (var activity in new[] { MorningActivity.Breakfast, MorningActivity.Rest, MorningActivity.Study, MorningActivity.Work, MorningActivity.CoverShift })
            {
                var stations = UnityEngine.Object.FindObjectsByType<ActivityStation>().Where(s => s.Activity == activity).ToArray();
                if (stations.Length == 0 || stations.Any(s => s.GetComponent<Collider>() == null))
                    throw new InvalidOperationException("Missing interaction collider: " + activity);
            }
            var room = GameObject.Find("M5 - Student room").transform;
            Physics.SyncTransforms();
            foreach (float z in new[] { -3.7f, -3f, -2.3f })
            {
                Vector3 foot = room.TransformPoint(new Vector3(0, .45f, z));
                if (Physics.CheckCapsule(foot, foot + Vector3.up * 1.1f, .31f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    throw new InvalidOperationException("Room entrance obstructed");
            }
            Debug.Log("[M5 detail] Scene references, interaction colliders and doorway clearance passed.");
        }
    }
}
