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
    public static class CafeManagerTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/CafeManagerMorning.unity";
        [MenuItem("Qua Thang Nay Da/M5/Open Cafe Manager Morning (latest)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        public static void Create()
        {
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); Check(); return; }
            var scene = EditorSceneManager.OpenScene(BikeVendorTools.ScenePath);
            var old = UnityEngine.Object.FindObjectsByType<ActivityStation>()
                .Single(s => s.Activity == MorningActivity.CoverShift);
            var vendor = GameObject.Find("Xoi vendor");
            if (vendor == null) throw new InvalidOperationException("Vendor model missing");
            // Reuse the same authored body proportions and materials, not its gameplay identity.
            var manager = UnityEngine.Object.Instantiate(vendor);
            manager.name = "Cafe manager";
            manager.transform.position = new Vector3(old.transform.position.x, .05f, old.transform.position.z);
            var cafe = GameObject.Find("M5 - Cafe frontage").transform;
            manager.transform.rotation = cafe.rotation * Quaternion.Euler(0, 180, 0);
            manager.transform.localScale = new Vector3(1.12f, 1.07f, 1.05f);
            var station = manager.GetComponent<ActivityStation>();
            station.Activity = MorningActivity.CoverShift;
            station.Label = "Quan ly ca phe - lam thay";
            old.gameObject.SetActive(false);
            var shirt = Surface("ManagerShirt", "#8099A7");
            var apron = Surface("ManagerApron", "#634633");
            var trim = Surface("ManagerTrim", "#EEE2C9");
            var dark = Surface("VendorHair", "#352925");
            foreach (Transform part in manager.transform)
            {
                if (part.name == "Shirt" || part.name == "Sleeve") part.GetComponent<Renderer>().sharedMaterial = shirt;
                if (part.name == "Apron") part.GetComponent<Renderer>().sharedMaterial = apron;
                if (part.name == "Apron pocket") part.GetComponent<Renderer>().sharedMaterial = trim;
            }
            manager.transform.Find("Hair bun").gameObject.SetActive(false);
            manager.transform.Find("Hair").localScale = new Vector3(.265f, .13f, .245f);
            manager.transform.Find("Hair").localPosition = new Vector3(0, 1.61f, -.02f);
            foreach (float x in new[] { -.065f, .065f })
            {
                var collar = Piece(manager.transform, "Shirt collar", new Vector3(x, 1.28f, .105f), new Vector3(.1f, .11f, .035f), trim);
                collar.transform.localRotation = Quaternion.Euler(0, 0, x < 0 ? 25 : -25);
                Piece(manager.transform, "Eyebrow", new Vector3(x * .77f, 1.55f, .115f), new Vector3(.045f, .012f, .012f), dark);
            }
            Piece(manager.transform, "Name badge", new Vector3(.09f, 1.12f, .165f), new Vector3(.1f, .045f, .014f), trim);
            Physics.SyncTransforms(); Check();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save manager scene");
            AssetDatabase.SaveAssets();
        }

        private static void Check()
        {
            Physics.SyncTransforms();
            var all = UnityEngine.Object.FindObjectsByType<ActivityStation>();
            var manager = all.Single(s => s.Activity == MorningActivity.CoverShift);
            if (manager.name != "Cafe manager" || manager.GetComponentsInChildren<Renderer>().Length < 10)
                throw new InvalidOperationException("Manager model missing");
            foreach (float height in new[] { .85f, 1.5f })
            {
                Vector3 eye = manager.transform.TransformPoint(new Vector3(0, height, 1.2f));
                if (ActivityTargeting.Find(eye, -manager.transform.forward, out var point) != manager
                    || !ActivityTargeting.Visible(manager, eye, point))
                    throw new InvalidOperationException("Manager interaction failed");
            }
            if (GameObject.Find("Xoi vendor").GetComponent<ActivityStation>().Activity != MorningActivity.Breakfast)
                throw new InvalidOperationException("Vendor activity changed");
            Debug.Log("[Manager] One active cover-shift NPC; torso/head interactions passed; vendor remains breakfast.");
        }
    }
}
