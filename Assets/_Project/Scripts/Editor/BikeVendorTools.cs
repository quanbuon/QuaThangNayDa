using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static QuaThangNayDa.Editor.ArtSliceTools;

namespace QuaThangNayDa.Editor
{
    public static class BikeVendorTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/BikeVendorMorning.unity";
        [MenuItem("Qua Thang Nay Da/M5/Open Bike and Vendor Morning (latest)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        private static GameObject Visual(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
            PrimitiveType shape = PrimitiveType.Sphere)
            => Piece(parent, name, position, scale, material, shape, false);

        public static void Create()
        {
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); Check(); return; }
            var scene = EditorSceneManager.OpenScene(StreetBreakfastTools.ScenePath);
            var bike = UnityEngine.Object.FindAnyObjectByType<ArcadeScooter>();
            if (bike == null || bike.Seat == null) throw new InvalidOperationException("Bike references missing");
            foreach (string name in new[] { "Body", "Handlebar" })
            {
                var old = bike.transform.Find(name);
                if (old != null) old.gameObject.SetActive(false);
            }
            var root = new GameObject("Red black underbone - visuals only").transform;
            root.SetParent(bike.transform, false);
            var red = Surface("BikeRed", "#B91F34");
            var black = Surface("BikeBlack", "#202429");
            var tire = Surface("CartRubber", "#252B2D");
            var silver = Surface("CartSteel", "#B6C3C5");
            var light = Surface("BikeLamp", "#E6EAD9");
            var amber = Surface("BikeIndicator", "#EBA33C");
            // Keep the original single physics box, seat and safe spot unchanged.
            // Cylinders point along local Y; rotate them onto the wheel axle X.
            foreach (float z in new[] { -.62f, .65f })
            {
                var wheel = Visual(root, "Tire", new Vector3(0, -.17f, z), new Vector3(.6f, .07f, .6f), tire, PrimitiveType.Cylinder);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var rim = Visual(root, "Alloy wheel", new Vector3(0, -.17f, z), new Vector3(.43f, .075f, .43f), silver, PrimitiveType.Cylinder);
                rim.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var hub = Visual(root, "Dark wheel centre", new Vector3(0, -.17f, z), new Vector3(.31f, .08f, .31f), black, PrimitiveType.Cylinder);
                hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
                for (int n = 0; n < 5; n++)
                {
                    var spoke = Visual(root, "Wheel spoke", new Vector3(0, -.17f, z), new Vector3(.165f, .035f, .41f), silver, PrimitiveType.Cube);
                    spoke.transform.localRotation = Quaternion.Euler(n * 36, 0, 0);
                }
            }
            Visual(root, "Engine", new Vector3(0, -.02f, -.12f), new Vector3(.36f, .38f, .45f), black);
            Visual(root, "Engine cover", new Vector3(.19f, -.03f, -.13f), new Vector3(.06f, .25f, .25f), silver);
            Visual(root, "Seat", new Vector3(0, .36f, -.38f), new Vector3(.43f, .19f, 1.03f), black);
            Visual(root, "Tail fairing", new Vector3(0, .23f, -.51f), new Vector3(.46f, .25f, .83f), red);
            Visual(root, "Tail lamp", new Vector3(0, .28f, -.94f), new Vector3(.27f, .09f, .05f), red);
            foreach (float x in new[] { -.2f, .2f })
            {
                var panel = Visual(root, "Leg shield", new Vector3(x, .28f, .35f), new Vector3(.15f, .68f, .39f), black);
                panel.transform.localRotation = Quaternion.Euler(-18, 0, x < 0 ? -12 : 12);
                var stripe = Visual(root, "Red side trim", new Vector3(x * 1.2f, .29f, .35f), new Vector3(.045f, .58f, .25f), red);
                stripe.transform.localRotation = panel.transform.localRotation;
                Visual(root, "Front fork", new Vector3(x * .6f, -.01f, .65f), new Vector3(.045f, .45f, .045f), silver, PrimitiveType.Cylinder);
                Visual(root, "Foot peg", new Vector3(x * 1.5f, -.12f, -.12f), new Vector3(.17f, .06f, .09f), black, PrimitiveType.Cube);
            }
            Visual(root, "Front mudguard", new Vector3(0, .16f, .67f), new Vector3(.24f, .16f, .68f), black);
            Visual(root, "Front red panel", new Vector3(0, .44f, .58f), new Vector3(.36f, .4f, .18f), red);
            Visual(root, "Headlight housing", new Vector3(0, .7f, .56f), new Vector3(.55f, .25f, .28f), black);
            Visual(root, "Headlight lens", new Vector3(0, .7f, .7f), new Vector3(.32f, .14f, .035f), light);
            Visual(root, "Instrument cluster", new Vector3(0, .78f, .43f), new Vector3(.24f, .045f, .16f), silver);
            Visual(root, "Handlebar", new Vector3(0, .69f, .43f), new Vector3(.83f, .055f, .07f), black, PrimitiveType.Cube);
            foreach (float x in new[] { -.35f, .35f })
            {
                Visual(root, "Mirror stalk", new Vector3(x, .88f, .48f), new Vector3(.018f, .25f, .018f), silver, PrimitiveType.Cylinder);
                Visual(root, "Mirror back", new Vector3(x, 1.12f, .49f), new Vector3(.18f, .13f, .055f), black);
                Visual(root, "Mirror surface", new Vector3(x, 1.12f, .455f), new Vector3(.15f, .1f, .012f), silver);
                Visual(root, "Turn indicator", new Vector3(x * .65f, .57f, .65f), new Vector3(.09f, .07f, .08f), amber);
            }
            Visual(root, "Exhaust", new Vector3(.29f, -.18f, -.52f), new Vector3(.12f, .13f, .7f), silver);

            var cart = GameObject.Find("Xoi street cart").transform;
            var vendor = new GameObject("Xoi vendor").transform;
            vendor.position = cart.TransformPoint(new Vector3(1.65f, 0, .3f));
            vendor.rotation = cart.rotation * Quaternion.Euler(0, 180, 0);
            var station = vendor.gameObject.AddComponent<ActivityStation>();
            station.Activity = MorningActivity.Breakfast; station.Label = "Co ban xoi - mua an sang";
            var skin = Surface("VendorSkin", "#C78F6C");
            var shirt = Surface("VendorShirt", "#DDD3B8");
            var apron = Surface("VendorApron", "#346E67");
            var hair = Surface("VendorHair", "#352925");
            Piece(vendor, "Trousers", new Vector3(0, .67f, 0), new Vector3(.34f, .25f, .24f), black);
            foreach (float x in new[] { -.1f, .1f })
            {
                Piece(vendor, "Leg", new Vector3(x, .34f, 0), new Vector3(.15f, .3f, .16f), black, PrimitiveType.Capsule);
                Piece(vendor, "Shoe", new Vector3(x, .07f, .05f), new Vector3(.17f, .12f, .28f), black, PrimitiveType.Sphere);
                Piece(vendor, "Sleeve", new Vector3(x * 2.3f, 1.12f, 0), new Vector3(.17f, .18f, .18f), shirt, PrimitiveType.Capsule);
                Piece(vendor, "Forearm", new Vector3(x * 2.3f, .87f, .03f), new Vector3(.11f, .16f, .12f), skin, PrimitiveType.Capsule);
                Piece(vendor, "Hand", new Vector3(x * 2.3f, .69f, .04f), new Vector3(.11f, .15f, .09f), skin, PrimitiveType.Sphere);
            }
            Piece(vendor, "Shirt", new Vector3(0, 1.05f, 0), new Vector3(.39f, .51f, .25f), shirt, PrimitiveType.Sphere);
            Piece(vendor, "Apron", new Vector3(0, .91f, .14f), new Vector3(.3f, .58f, .035f), apron);
            Piece(vendor, "Apron pocket", new Vector3(0, .85f, .163f), new Vector3(.2f, .12f, .02f), shirt);
            Piece(vendor, "Neck", new Vector3(0, 1.34f, 0), new Vector3(.12f, .1f, .12f), skin, PrimitiveType.Cylinder);
            Piece(vendor, "Head", new Vector3(0, 1.5f, 0), new Vector3(.25f, .31f, .24f), skin, PrimitiveType.Sphere);
            Piece(vendor, "Hair", new Vector3(0, 1.59f, -.025f), new Vector3(.27f, .19f, .25f), hair, PrimitiveType.Sphere);
            Piece(vendor, "Hair bun", new Vector3(0, 1.56f, -.15f), new Vector3(.13f, .13f, .13f), hair, PrimitiveType.Sphere);
            foreach (float x in new[] { -.05f, .05f })
                Piece(vendor, "Eye", new Vector3(x, 1.52f, .113f), new Vector3(.022f, .025f, .015f), black, PrimitiveType.Sphere);
            Piece(vendor, "Nose", new Vector3(0, 1.47f, .12f), new Vector3(.035f, .045f, .04f), skin, PrimitiveType.Sphere);
            Physics.SyncTransforms(); Check();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save bike/vendor scene");
            AssetDatabase.SaveAssets();
        }

        private static void Check()
        {
            Physics.SyncTransforms();
            var bike = UnityEngine.Object.FindAnyObjectByType<ArcadeScooter>();
            if (bike.Seat == null || bike.SafeSpot == null || bike.GetComponentsInChildren<Collider>().Length != 1)
                throw new InvalidOperationException("Bike visual upgrade changed collision rig");
            var vendor = GameObject.Find("Xoi vendor").transform;
            var station = vendor.GetComponent<ActivityStation>();
            foreach (float height in new[] { .85f, 1.5f })
            {
                Vector3 eye = vendor.TransformPoint(new Vector3(0, height, 1.5f));
                if (ActivityTargeting.Find(eye, -vendor.forward, out var point) != station || !ActivityTargeting.Visible(station, eye, point))
                    throw new InvalidOperationException("Vendor interaction failed");
            }
            Debug.Log("[BikeVendor] Single original bike collider/seat/recovery preserved; vendor interaction rays passed.");
        }
    }
}
