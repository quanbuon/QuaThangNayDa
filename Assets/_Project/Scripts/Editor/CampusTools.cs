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
    public static class CampusTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/CampusMorning.unity";
        [MenuItem("Qua Thang Nay Da/M5/Open Campus Morning (latest)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        private static void Bind(GameObject item, MorningActivity activity, string label)
        {
            var station = item.GetComponent<ActivityStation>() ?? item.AddComponent<ActivityStation>();
            station.Activity = activity; station.Label = label;
        }

        public static void Create()
        {
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); Check(); return; }
            var scene = EditorSceneManager.OpenScene(ArtDetailTools.ScenePath);
            var room = GameObject.Find("M5 - Student room").transform;
            foreach (string name in new[] { "Mattress", "Bed frame" })
                Bind(room.Find(name).gameObject, MorningActivity.Rest, "Nghi tren giuong");
            // Both the whole tabletop and the tray are usable, not a 4cm-thick target only.
            Bind(room.Find("Study desk").gameObject, MorningActivity.Breakfast, "An sang tai ban");
            var tray = room.Find("Breakfast tray");
            tray.localPosition = new Vector3(1.25f, .89f, 1.92f);
            tray.localScale = new Vector3(.75f, .1f, .5f);
            tray.GetComponent<Renderer>().sharedMaterial = Surface("Tray", "#D4A34F");
            Bind(tray.gameObject, MorningActivity.Breakfast, "An sang");
            var bread = room.Find("Bread");
            bread.localPosition = new Vector3(1.25f, 1, 1.92f);
            bread.localScale = new Vector3(.46f, .14f, .2f);
            // Remove floating instructions, but keep architectural signage and a smaller cafe menu.
            foreach (var label in UnityEngine.Object.FindObjectsByType<TextMesh>())
            {
                if (label.text.Contains("F:") || label.text.Contains("F: ")) label.gameObject.SetActive(false);
                else if (label.text.Contains("CA PHE SUA")) label.characterSize = .03f;
            }

            var oldSchool = GameObject.Find("Building left 1");
            if (oldSchool == null) throw new InvalidOperationException("School site missing");
            oldSchool.SetActive(false);
            foreach (var station in UnityEngine.Object.FindObjectsByType<ActivityStation>())
                if (station.Activity == MorningActivity.Study) station.gameObject.SetActive(false);
            var school = new GameObject("Classroom building").transform;
            school.position = new Vector3(-10, 0, 4);
            school.rotation = Quaternion.Euler(0, -90, 0);
            var cream = Surface("Plaster", "#D7C6A5");
            var teal = Surface("Teal", "#386D69");
            var wood = Surface("Wood", "#986647");
            var floor = Surface("Tile", "#A59F8E");
            var glass = Surface("WindowBlue", "#698D99");
            Piece(school, "Floor", new Vector3(0, .025f, 0), new Vector3(8, .05f, 6), floor);
            Piece(school, "Back wall", new Vector3(0, 1.7f, 2.9f), new Vector3(8, 3.4f, .2f), cream);
            foreach (float x in new[] { -3.9f, 3.9f })
                Piece(school, "Side wall", new Vector3(x, 1.7f, 0), new Vector3(.2f, 3.4f, 6), cream);
            foreach (float x in new[] { -2.5f, 2.5f })
            {
                Piece(school, "Front wall", new Vector3(x, 1.5f, -2.9f), new Vector3(2.8f, 3, .2f), cream);
                Piece(school, "Window frame", new Vector3(x, 1.9f, -3.02f), new Vector3(1.8f, 1.2f, .05f), teal, solid: false);
                Piece(school, "Window pane", new Vector3(x, 1.9f, -3.06f), new Vector3(1.65f, 1.05f, .02f), glass, solid: false);
                Piece(school, "Window divider", new Vector3(x, 1.9f, -3.08f), new Vector3(.04f, 1.05f, .02f), cream, solid: false);
            }
            Piece(school, "Roof", new Vector3(0, 3.45f, 0), new Vector3(8.3f, .2f, 6.3f), teal);
            Piece(school, "Entry canopy", new Vector3(0, 2.8f, -3.4f), new Vector3(2.5f, .12f, 1.3f), teal);
            Piece(school, "Signboard", new Vector3(0, 3.1f, -3.03f), new Vector3(5, .5f, .06f), teal, solid: false);
            Sign(school, "KHU GIANG DUONG / PHONG 101", new Vector3(0, 3.1f, -3.08f), .043f);
            Piece(school, "Blackboard", new Vector3(0, 1.9f, 2.75f), new Vector3(3, 1.3f, .06f), teal);
            Sign(school, "BUOI HOC SANG", new Vector3(0, 1.95f, 2.7f), .04f);
            foreach (float x in new[] { -2f, 2f })
            foreach (float z in new[] { -1.1f, .7f })
            {
                var desk = Piece(school, "Student desk", new Vector3(x, .76f, z), new Vector3(1.4f, .1f, .6f), wood);
                Bind(desk, MorningActivity.Study, "Ngoi hoc");
                foreach (float offset in new[] { -.55f, .55f })
                    Piece(school, "Desk leg", new Vector3(x + offset, .36f, z), new Vector3(.07f, .72f, .48f), teal);
                var seat = Piece(school, "Student seat", new Vector3(x, .45f, z - .65f), new Vector3(.7f, .1f, .5f), wood);
                Bind(seat, MorningActivity.Study, "Ngoi hoc");
                var back = Piece(school, "Seat back", new Vector3(x, .75f, z - .9f), new Vector3(.7f, .6f, .08f), teal);
                Bind(back, MorningActivity.Study, "Ngoi hoc");
                Piece(school, "Seat support", new Vector3(x, .2f, z - .65f), new Vector3(.4f, .4f, .35f), teal);
            }
            Physics.SyncTransforms(); Check();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save campus scene");
            AssetDatabase.SaveAssets();
        }

        private static void RayCheck(Transform root, Vector3 eye, Vector3 aim, MorningActivity expected)
        {
            Vector3 origin = root.TransformPoint(eye), direction = root.TransformPoint(aim) - origin;
            if (!Physics.Raycast(origin, direction.normalized, out var hit, 3, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                || hit.collider.GetComponent<ActivityStation>() == null
                || hit.collider.GetComponent<ActivityStation>().Activity != expected)
                throw new InvalidOperationException("Interaction ray failed: " + expected + " hit " + (hit.collider == null ? "nothing" : hit.collider.name));
        }

        private static void Check()
        {
            Physics.SyncTransforms();
            var room = GameObject.Find("M5 - Student room").transform;
            RayCheck(room, new Vector3(-.9f, 1.65f, .4f), new Vector3(-2.4f, .55f, .4f), MorningActivity.Rest);
            RayCheck(room, new Vector3(-.9f, .8f, .4f), new Vector3(-2.4f, .25f, .4f), MorningActivity.Rest);
            RayCheck(room, new Vector3(1.25f, 1.65f, .8f), new Vector3(1.25f, .9f, 1.92f), MorningActivity.Breakfast);
            var cafe = GameObject.Find("M5 - Cafe frontage").transform;
            RayCheck(cafe, new Vector3(0, 1.65f, 0), new Vector3(0, .8f, 1.6f), MorningActivity.Work);
            var school = GameObject.Find("Classroom building").transform;
            RayCheck(school, new Vector3(0, 1.65f, -1.1f), new Vector3(2, .76f, -1.1f), MorningActivity.Study);
            foreach (float z in new[] { -3.5f, -2.9f, -2.2f, 0f, 1.5f })
            {
                var lower = school.TransformPoint(new Vector3(0, .45f, z));
                if (Physics.CheckCapsule(lower, lower + Vector3.up * 1.1f, .31f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    throw new InvalidOperationException("School doorway/aisle blocked");
            }
            Debug.Log("[Campus] Bed frame/mattress, breakfast, cafe and study interaction rays passed; school entrance/aisle clear.");
        }
    }
}
