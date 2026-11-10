using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static QuaThangNayDa.Editor.ArtSliceTools;

namespace QuaThangNayDa.Editor
{
    public static class StreetBreakfastTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/StreetMorning.unity";
        [MenuItem("Qua Thang Nay Da/M5/Open Street Morning - Xoi Cart (latest)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        private static Transform Group(Transform parent, string name, MorningActivity activity, string label, params string[] parts)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            var station = group.gameObject.AddComponent<ActivityStation>();
            station.Activity = activity; station.Label = label;
            foreach (string partName in parts)
            {
                var part = parent.Find(partName);
                if (part == null) throw new InvalidOperationException("Missing part: " + partName);
                var old = part.GetComponent<ActivityStation>();
                if (old != null) UnityEngine.Object.DestroyImmediate(old);
                if (part.GetComponent<Collider>() == null) part.gameObject.AddComponent<BoxCollider>();
                part.SetParent(group, true);
            }
            return group;
        }

        public static void Create()
        {
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); Check(); return; }
            var scene = EditorSceneManager.OpenScene(CampusTools.ScenePath);
            var room = GameObject.Find("M5 - Student room").transform;
            Group(room, "Bed interaction", MorningActivity.Rest, "Nghi tren giuong",
                "Bed frame", "Mattress", "Blanket", "Pillow");
            var cafe = GameObject.Find("M5 - Cafe frontage").transform;
            Group(cafe, "Counter interaction", MorningActivity.Work, "Nhan ca tai quay", "Counter", "Counter top");
            foreach (var station in UnityEngine.Object.FindObjectsByType<ActivityStation>())
                if (station.Activity == MorningActivity.Breakfast) UnityEngine.Object.DestroyImmediate(station);
            foreach (string name in new[] { "Breakfast tray", "Bread" }) room.Find(name).gameObject.SetActive(false);

            var cart = new GameObject("Xoi street cart").transform;
            cart.position = new Vector3(-4.5f, 0, -2.5f);
            cart.rotation = Quaternion.Euler(0, -90, 0);
            var breakfast = cart.gameObject.AddComponent<ActivityStation>();
            breakfast.Activity = MorningActivity.Breakfast; breakfast.Label = "Mua xoi - an sang";
            var red = Surface("CartRed", "#C52229");
            var yellow = Surface("CartYellow", "#EAC445");
            var steel = Surface("CartSteel", "#B6C3C5");
            steel.SetFloat("_Metallic", .65f); steel.SetFloat("_Smoothness", .5f);
            EditorUtility.SetDirty(steel);
            var rubber = Surface("CartRubber", "#252B2D");
            Piece(cart, "Red cabinet", new Vector3(0, .85f, 0), new Vector3(1.55f, .8f, .8f), red);
            Piece(cart, "Steel worktop", new Vector3(0, 1.28f, 0), new Vector3(1.7f, .08f, .92f), steel);
            Piece(cart, "Side serving shelf", new Vector3(1.05f, 1.26f, 0), new Vector3(.45f, .06f, .7f), steel);
            Piece(cart, "Lower rail", new Vector3(0, .43f, 0), new Vector3(1.7f, .08f, .9f), steel);
            foreach (float x in new[] { -.77f, .77f })
            foreach (float z in new[] { -.38f, .38f })
                Piece(cart, "Display post", new Vector3(x, 1.8f, z), new Vector3(.035f, 1.05f, .035f), steel);
            Piece(cart, "Display shelf", new Vector3(0, 1.83f, .1f), new Vector3(1.55f, .035f, .65f), steel);
            Piece(cart, "Red header", new Vector3(0, 2.38f, 0), new Vector3(1.8f, .38f, .94f), red);
            Sign(cart, "XOI NONG - BANH MI", new Vector3(0, 2.42f, -.48f), .028f);
            Piece(cart, "Yellow menu panel", new Vector3(.38f, .88f, -.407f), new Vector3(.7f, .65f, .02f), yellow);
            Sign(cart, "XOI\nNONG", new Vector3(-.35f, .9f, -.422f), .048f);
            Sign(cart, "PATE\nTRUNG\nTHIT", new Vector3(.38f, .9f, -.425f), .022f);
            foreach (float x in new[] { -.85f, .85f })
            {
                var wheel = Piece(cart, "Wheel", new Vector3(x, .37f, .12f), new Vector3(.7f, .065f, .7f), rubber, PrimitiveType.Cylinder);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var hub = Piece(cart, "Wheel hub", new Vector3(x * 1.02f, .37f, .12f), new Vector3(.5f, .07f, .5f), steel, PrimitiveType.Cylinder);
                hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            Piece(cart, "Parking foot", new Vector3(0, .23f, -.3f), new Vector3(.06f, .46f, .06f), steel);
            Piece(cart, "Steamer pot", new Vector3(.45f, 1.53f, 0), new Vector3(.43f, .21f, .43f), steel, PrimitiveType.Cylinder);
            Piece(cart, "Steamer lid", new Vector3(.45f, 1.77f, 0), new Vector3(.47f, .07f, .47f), steel, PrimitiveType.Sphere);
            Piece(cart, "Lid knob", new Vector3(.45f, 1.83f, 0), new Vector3(.08f, .07f, .08f), rubber, PrimitiveType.Sphere);
            Piece(cart, "Food tray", new Vector3(-.35f, 1.36f, 0), new Vector3(.6f, .08f, .55f), yellow);
            Physics.SyncTransforms(); Check();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save street morning");
            AssetDatabase.SaveAssets();
        }

        private static void Check()
        {
            Physics.SyncTransforms();
            var room = GameObject.Find("M5 - Student room").transform;
            var bed = room.Find("Bed interaction");
            var owner = bed.GetComponent<ActivityStation>();
            foreach (var collider in bed.GetComponentsInChildren<Collider>())
            {
                if (collider.GetComponentInParent<ActivityStation>() != owner)
                    throw new InvalidOperationException("Bed part has wrong action");
                Vector3 aim = collider.bounds.center;
                Vector3 eye = aim + Vector3.up * 1.4f;
                if (ActivityTargeting.Find(eye, Vector3.down, out var point) != owner || !ActivityTargeting.Visible(owner, eye, point))
                    throw new InvalidOperationException("Bed part targeting failed: " + collider.name);
            }
            var cart = GameObject.Find("Xoi street cart").transform;
            foreach (var collider in cart.GetComponentsInChildren<Collider>())
                if (collider.GetComponentInParent<ActivityStation>() != cart.GetComponent<ActivityStation>())
                    throw new InvalidOperationException("Cart part not bound");
            foreach (Vector3 aim in new[] { new Vector3(0, .85f, -.4f), new Vector3(0, 2.38f, -.47f), new Vector3(-.85f, .37f, .12f) })
            {
                var world = cart.TransformPoint(aim);
                var eye = world + cart.TransformDirection(Vector3.back) * 1.5f;
                if (ActivityTargeting.Find(eye, (world-eye).normalized, out var point) != cart.GetComponent<ActivityStation>()
                    || !ActivityTargeting.Visible(cart.GetComponent<ActivityStation>(), eye, point))
                    throw new InvalidOperationException("Cart targeting failed");
            }
            foreach (var station in UnityEngine.Object.FindObjectsByType<ActivityStation>())
                if (station.Activity == MorningActivity.Breakfast && station.transform != cart)
                    throw new InvalidOperationException("Old breakfast station remains active");
            Debug.Log("[Street] Whole bed/cart ownership and rays passed; breakfast only at cart.");
        }
    }
}
