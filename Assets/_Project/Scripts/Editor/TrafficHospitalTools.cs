using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static QuaThangNayDa.Editor.ArtSliceTools;

namespace QuaThangNayDa.Editor
{
    public static class TrafficHospitalTools
    {
        public const string ScenePath = "Assets/_Project/Scenes/MainMorning.unity";
        [MenuItem("Qua Thang Nay Da/Open Main Morning - Traffic and Hospital")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); EditorSceneManager.OpenScene(ScenePath);
        }

        public static void BuildBatch()
        {
            MorningChecks.Run(); CommitmentChecks.Run(); SaveChecks.Run(); HospitalChecks.Run();
            Create(); Check(); ActivityTargetingChecks.Run(); TrafficRuntimeChecks.Run();
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-m6-build-output");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing M6 output");
            string output = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath },
                locationPathName = output, target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging });
            Debug.Log($"[M6] Build {report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("M6 build failed");
        }

        private static Transform Marker(Transform root, string name, Vector3 at)
        {
            var item = new GameObject(name).transform; item.SetParent(root, false); item.localPosition = at;
            item.localRotation = Quaternion.Euler(0, 180, 0); return item;
        }

        private static void Create()
        {
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                UpgradeHospital();
                return;
            }
            if (!File.Exists(CafeManagerTools.ScenePath)) throw new FileNotFoundException("Latest art scene missing");
            var scene = EditorSceneManager.OpenScene(CafeManagerTools.ScenePath);
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            session.Saves.UseTrafficSlots = true;
            var hospital = new GameObject("Hospital session", typeof(HospitalSession)).GetComponent<HospitalSession>();
            hospital.Session = session; session.Hospital = hospital;
            var old = GameObject.Find("Building right 3"); if (old != null) old.SetActive(false);
            var root = new GameObject("Hospital - two floors").transform;
            root.position = new Vector3(10, 0, 28); root.rotation = Quaternion.Euler(0, 90, 0);
            var plaster = Surface("HospitalPlaster", "#DDDCD0");
            var teal = Surface("HospitalTeal", "#518F95");
            var floor = Surface("HospitalFloor", "#AAAFA9");
            var dark = Surface("Metal", "#343C40");
            Piece(root, "Ground floor", new Vector3(0, -.05f, 0), new Vector3(8, .1f, 6), floor);
            Piece(root, "Upper floor - stair opening", new Vector3(-1, 3.125f, 0), new Vector3(6, .15f, 6), floor);
            Piece(root, "Upper landing", new Vector3(3, 3.125f, 2.65f), new Vector3(2, .15f, .7f), floor);
            Piece(root, "Rear wall", new Vector3(0, 3.2f, 3), new Vector3(8, 6.4f, .15f), plaster);
            foreach (float x in new[] { -4f, 4f })
                Piece(root, "Side wall", new Vector3(x, 3.2f, 0), new Vector3(.15f, 6.4f, 6), plaster);
            foreach (float x in new[] { -2.5f, 2.5f })
                Piece(root, "Front wall", new Vector3(x, 3.2f, -3), new Vector3(3, 6.4f, .15f), plaster);
            Piece(root, "Door lintel", new Vector3(0, 4.5f, -3), new Vector3(2, 3.8f, .15f), plaster);
            Piece(root, "Roof", new Vector3(0, 6.5f, 0), new Vector3(8.2f, .2f, 6.2f), teal);
            Piece(root, "Hospital sign", new Vector3(0, 2.85f, -3.12f), new Vector3(5.5f, .5f, .1f), teal, solid: false);
            Sign(root, "BENH VIEN SINH VIEN", new Vector3(0, 2.85f, -3.18f), .05f);
            Sign(root, "TANG 1: TIEP NHAN  |  TANG 2: DIEU TRI", new Vector3(0, 2.45f, -3.18f), .023f);
            Piece(root, "Sidewalk", new Vector3(0, .04f, -3.8f), new Vector3(8, .08f, 1.5f), floor);
            for (int step = 0; step < 16; step++)
            {
                float top = (step + 1) * .2f;
                Piece(root, "Stair " + step, new Vector3(3, top * .5f, -2.5f + step * .32f), new Vector3(1.7f, top, .32f), floor);
            }
            Piece(root, "Upper safety barrier", new Vector3(2.05f, 3.75f, -.5f), new Vector3(.1f, 1.1f, 5.2f), teal);
            for (int n = 0; n < 6; n++)
                Piece(root, "Stair rail post", new Vector3(2.12f, .7f + n * .5f, -2.5f + n * .8f), new Vector3(.06f, 1, .06f), teal);
            var rail = Piece(root, "Stair handrail", new Vector3(2.12f, 2.1f, -.1f), new Vector3(.08f, .08f, 5.6f), teal);
            rail.transform.localRotation = Quaternion.Euler(-32, 0, 0);
            Piece(root, "Reception counter", new Vector3(-2.5f, .5f, 1.6f), new Vector3(2, 1, .65f), teal);
            Sign(root, "TIEP NHAN", new Vector3(-2.5f, 1.8f, 2.88f), .04f);
            foreach (float z in new[] { -.8f, 1.6f })
            {
                Piece(root, "Ward bed frame", new Vector3(-2.6f, 3.45f, z), new Vector3(1.4f, .5f, 1.9f), teal);
                Piece(root, "Ward mattress", new Vector3(-2.6f, 3.78f, z), new Vector3(1.35f, .16f, 1.85f), plaster);
                Piece(root, "Ward pillow", new Vector3(-2.6f, 3.9f, z + .6f), new Vector3(.7f, .12f, .35f), plaster, solid: false);
            }
            Sign(root, "PHONG DIEU TRI", new Vector3(0, 5.2f, 2.88f), .04f);
            hospital.Reception = Marker(root, "Admission point", new Vector3(-1, .12f, -.8f));
            hospital.Ward = Marker(root, "Ward admission point", new Vector3(0, 3.3f, .5f));
            hospital.Exit = Marker(root, "Discharge point", new Vector3(0, .14f, -3.8f));
            hospital.BikeParking = Marker(root, "Hospital bike parking", new Vector3(2, .55f, -4.3f));

            var paint = Surface("RoadPaint", "#DBD6BA");
            for (int z = -16; z < 38; z += 5)
                Piece(null, "Centre lane dash", new Vector3(0, .008f, z), new Vector3(.08f, .012f, 2.5f), paint, solid: false);
            for (int i = 0; i < 4; i++)
            {
                bool reverse = i >= 2, car = i % 2 == 0;
                float x = reverse ? -2.2f : 2.2f;
                var vehicle = new GameObject(car ? "Traffic car " + i : "Traffic motorbike " + i, typeof(BoxCollider), typeof(Rigidbody), typeof(TrafficVehicle));
                vehicle.transform.position = new Vector3(x, car ? .65f : .6f, -12 + i * 11);
                vehicle.transform.rotation = Quaternion.Euler(0, reverse ? 180 : 0, 0);
                var box = vehicle.GetComponent<BoxCollider>(); box.size = car ? new Vector3(1.55f, 1.1f, 3) : new Vector3(.65f, 1.05f, 1.7f);
                var driver = vehicle.GetComponent<TrafficVehicle>(); driver.Session = session; driver.Hospital = hospital;
                driver.StartPoint = new Vector3(x, vehicle.transform.position.y, reverse ? 36 : -16);
                driver.EndPoint = new Vector3(x, vehicle.transform.position.y, reverse ? -16 : 36);
                driver.CruiseSpeed = car ? 6 : 5;
                var color = Surface(car ? "TrafficCar" : "TrafficBike", car ? "#B09A70" : "#526C81");
                Piece(vehicle.transform, "Body", Vector3.zero, car ? new Vector3(1.5f, .6f, 2.9f) : new Vector3(.5f, .5f, 1.5f), color, solid: false);
                Piece(vehicle.transform, car ? "Cabin" : "Rider", new Vector3(0, car ? .55f : .65f, -.15f),
                    car ? new Vector3(1.25f, .65f, 1.5f) : new Vector3(.4f, .7f, .4f), teal,
                    car ? PrimitiveType.Cube : PrimitiveType.Capsule, false);
                foreach (float axle in new[] { -.6f, .6f })
                foreach (float side in car ? new[] { -.73f, .73f } : new[] { 0f })
                {
                    var wheel = Piece(vehicle.transform, "Wheel", new Vector3(side, -.32f, axle), new Vector3(.55f, .07f, .55f), dark, PrimitiveType.Cylinder, false);
                    wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
            }
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save main scene");
            AssetDatabase.SaveAssets();
            UpgradeHospital();
        }

        // Targeted, repeatable upgrade; preserve the rest of the authored main scene.
        private static void UpgradeHospital()
        {
            var root = GameObject.Find("Hospital - two floors").transform;
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            session.Hospital.Ward.localPosition = new Vector3(-1.25f, 3.3f, -.8f);
            session.Hospital.Ward.localRotation = Quaternion.Euler(0, 270, 0);
            session.Hospital.WardBedView = root.Find("Ward bed view") ??
                Marker(root, "Ward bed view", new Vector3(-2.6f, 4.05f, -.2f));
            session.Hospital.WardBedView.localRotation = Quaternion.Euler(-90, 180, 0);
            var room = GameObject.Find("M5 - Student room");
            if (room == null)
            {
                foreach (var station in UnityEngine.Object.FindObjectsByType<ActivityStation>())
                    if (station.Activity == MorningActivity.Rest && station.transform.parent != null &&
                        station.transform.parent.Find("Pillow") != null) { room = station.transform.parent.gameObject; break; }
            }
            if (room == null) throw new InvalidOperationException("Bedroom missing");
            session.BedView = room.transform.Find("Sleeping view") ?? Marker(room.transform, "Sleeping view", new Vector3(-2.4f, 1.02f, 1.05f));
            session.BedView.localRotation = Quaternion.Euler(-25, 180, 8);
            session.BedExit = room.transform.Find("Bed exit") ?? Marker(room.transform, "Bed exit", new Vector3(-1.1f, .12f, .4f));
            session.BedExit.localRotation = Quaternion.Euler(0, 180, 0);
            for (int n = 0; n < 16; n++)
            {
                var step = root.Find("Stair " + n);
                step.localPosition = new Vector3(3, (n + 1) * .1f, -1.6f + n * .26f);
                step.localScale = new Vector3(1.7f, (n + 1) * .2f, .26f);
            }
            int postIndex = 0;
            foreach (Transform child in root)
                if (child.name == "Stair rail post")
                {
                    child.localPosition = new Vector3(2.12f, .7f + postIndex * .4923f, -1.6f + postIndex * .64f);
                    postIndex++;
                }
            var rail = root.Find("Stair handrail");
            rail.localPosition = new Vector3(2.12f, 2.1808f, 0);
            rail.localScale = new Vector3(.08f, .08f, Mathf.Sqrt(2.4615f * 2.4615f + 3.2f * 3.2f));
            rail.localRotation = Quaternion.Euler(-Mathf.Atan2(3, 3.9f) * Mathf.Rad2Deg, 0, 0);

            var counter = root.Find("Reception counter").gameObject;
            var counterStation = counter.GetComponent<ActivityStation>() ?? counter.AddComponent<ActivityStation>();
            counterStation.HospitalReception = true;
            counterStation.Activity = MorningActivity.None;
            counterStation.Label = "Tiep nhan - hoi vien phi / xem no";
            if (root.Find("Hospital receptionist") == null)
            {
                var receptionist = UnityEngine.Object.Instantiate(GameObject.Find("Xoi vendor"), root);
                receptionist.name = "Hospital receptionist";
                receptionist.transform.localPosition = new Vector3(-2.5f, .05f, 2.35f);
                receptionist.transform.localRotation = Quaternion.Euler(0, 180, 0);
                receptionist.transform.localScale = Vector3.one;
                var station = receptionist.GetComponent<ActivityStation>();
                station.Activity = MorningActivity.None;
                station.HospitalReception = true;
                station.Label = "Nhan vien tiep nhan - hoi vien phi / xem no";
                var white = root.Find("Front wall").GetComponent<Renderer>().sharedMaterial;
                var teal = counter.GetComponent<Renderer>().sharedMaterial;
                foreach (Transform part in receptionist.transform)
                {
                    if (part.name == "Shirt" || part.name == "Sleeve" || part.name == "Apron")
                        part.GetComponent<Renderer>().sharedMaterial = white;
                    if (part.name == "Apron pocket") part.GetComponent<Renderer>().sharedMaterial = teal;
                }
                Piece(receptionist.transform, "Staff badge", new Vector3(.09f, 1.15f, .17f), new Vector3(.11f, .07f, .02f), teal, solid: false);
            }
            if (!EditorSceneManager.SaveScene(root.gameObject.scene, ScenePath)) throw new IOException("Cannot save hospital upgrade");
        }

        private static void WalkTo(CharacterController cc, Transform building, Vector3 local)
        {
            Vector3 target = building.TransformPoint(local);
            for (int n = 0; n < 500; n++)
            {
                Vector3 delta = target - cc.transform.position;
                delta.y = 0;
                if (delta.magnitude < .04f) return;
                cc.Move(Vector3.ClampMagnitude(delta, .035f) + Vector3.down * .04f);
            }
            throw new InvalidOperationException("Hospital walking route blocked at " + local + "; reached " + building.InverseTransformPoint(cc.transform.position));
        }

        private static void Check()
        {
            EditorSceneManager.OpenScene(ScenePath); Physics.SyncTransforms();
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            if (session.Hospital == null || session.Hospital.Session != session || session.Hospital.Ward == null || session.Hospital.Reception == null)
                throw new InvalidOperationException("Hospital references missing");
            if (UnityEngine.Object.FindObjectsByType<TrafficVehicle>().Length != 4) throw new InvalidOperationException("Traffic missing");
            var building = GameObject.Find("Hospital - two floors").transform;
            foreach (var point in new[] { session.Hospital.Reception, session.Hospital.Ward, session.Hospital.Exit, session.BedExit })
            {
                if (Physics.CheckCapsule(point.position + Vector3.up * .35f, point.position + Vector3.up * 1.45f, .3f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) throw new InvalidOperationException("Hospital spawn blocked: " + point.name);
            }
            for (int n = 0; n < 16; n++)
            {
                var origin = building.TransformPoint(new Vector3(3, 4.5f, -1.6f + n * .26f));
                if (!Physics.Raycast(origin, Vector3.down, out var hit, 5) || Mathf.Abs(hit.point.y - (n + 1) * .2f) > .02f)
                    throw new InvalidOperationException("Stair step unsupported: " + n);
            }
            // Probe actual CharacterController traversal, not just the presence of stair meshes.
            var probe = new GameObject("Temporary stair traversal probe", typeof(CharacterController));
            try
            {
                var cc = probe.GetComponent<CharacterController>(); cc.height = 1.8f; cc.radius = .3f;
                cc.center = new Vector3(0, .9f, 0); cc.stepOffset = .25f; cc.skinWidth = .03f;
                probe.transform.position = building.TransformPoint(new Vector3(0, .1f, -3.8f));
                Physics.SyncTransforms();
                WalkTo(cc, building, new Vector3(0, 0, -2.25f));
                WalkTo(cc, building, new Vector3(3, 0, -2.25f));
                WalkTo(cc, building, new Vector3(3, 0, 2.55f));
                if (probe.transform.position.y < 3.1f) throw new InvalidOperationException("Controller cannot climb hospital stairs");
                WalkTo(cc, building, new Vector3(1.3f, 0, 2.55f));
                WalkTo(cc, building, building.InverseTransformPoint(session.Hospital.Ward.position));
                WalkTo(cc, building, new Vector3(1.3f, 0, 2.55f));
                WalkTo(cc, building, new Vector3(3, 0, 2.55f));
                WalkTo(cc, building, new Vector3(3, 0, -2.25f));
                if (probe.transform.position.y > .3f) throw new InvalidOperationException("Controller cannot descend hospital stairs");
                WalkTo(cc, building, new Vector3(0, 0, -2.25f));
                WalkTo(cc, building, new Vector3(0, 0, -3.8f));
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); }
            var bedroomProbe = new GameObject("Temporary bedroom walking probe", typeof(CharacterController));
            try
            {
                var cc = bedroomProbe.GetComponent<CharacterController>();
                cc.height = 1.8f; cc.radius = .3f; cc.center = new Vector3(0, .9f, 0);
                cc.stepOffset = .25f; cc.skinWidth = .03f;
                bedroomProbe.transform.position = session.BedExit.position;
                var room = session.BedExit.parent;
                Physics.SyncTransforms();
                WalkTo(cc, room, new Vector3(-1.1f, 0, -1));
                WalkTo(cc, room, new Vector3(0, 0, -1));
                WalkTo(cc, room, new Vector3(0, 0, -3.8f));
                WalkTo(cc, room, new Vector3(0, 0, -1));
                WalkTo(cc, room, new Vector3(-1.1f, 0, -1));
                WalkTo(cc, room, room.InverseTransformPoint(session.BedExit.position));
            }
            finally { UnityEngine.Object.DestroyImmediate(bedroomProbe); }
            foreach (var target in new[] { building.Find("Reception counter"), building.Find("Hospital receptionist") })
            {
                var station = target.GetComponent<ActivityStation>();
                Vector3 eye = building.TransformPoint(new Vector3(-2.5f, 1.65f, .3f));
                Vector3 aim = target.position + Vector3.up * (target.name == "Reception counter" ? 0 : 1.45f);
                if (!station.HospitalReception || ActivityTargeting.Find(eye, (aim - eye).normalized, out var point) != station ||
                    !ActivityTargeting.Visible(station, eye, point)) throw new InvalidOperationException("Reception interaction unavailable: " + target.name);
            }
            Debug.Log("[M6] Hospital entrance-to-ward round trip, stair ascent/descent, receptionist/counter targeting and spawn clearance passed.");
        }
    }
}
