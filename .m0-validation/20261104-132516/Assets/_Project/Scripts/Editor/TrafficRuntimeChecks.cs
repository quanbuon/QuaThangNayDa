using System;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    // Runs only in the disposable validation project. Exercises Unity adapters,
    // in addition to pure simulation tests; does not claim visual Play acceptance.
    public static class TrafficRuntimeChecks
    {
        private static void Call(object component, string method) => component.GetType()
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
        private static MorningSession Prepare(bool wake = true)
        {
            EditorSceneManager.OpenScene(TrafficHospitalTools.ScenePath);
            var session = UnityEngine.Object.FindAnyObjectByType<MorningSession>();
            Call(session.Player.Scooter, "Awake");
            Call(session.Player, "Awake");
            Call(session, "Start");
            if (wake) { session.WakeMorning(); session.StopBedAction(); }
            return session;
        }
        private static void Require(bool value, string message)
        { if (!value) throw new InvalidOperationException("[Traffic Runtime] " + message); }

        private static void RequireHospitalBed(MorningSession session)
        {
            Require(session.Player.IsInBed && !session.Player.GetComponent<CharacterController>().enabled &&
                Vector3.Distance(session.Player.View.transform.position, session.Hospital.WardBedView.position) < .01f &&
                session.Player.View.transform.position.y > 3.8f &&
                Vector3.Dot(session.Player.View.transform.forward, Vector3.up) > .99f,
                "Hospital patient lies on upstairs mattress looking at ceiling");
            session.StopBedAction();
            Require(session.Player.IsInBed, "Cannot leave hospital bed via ordinary stop action");
        }

        private static void ReloadAndResume(MorningSession session)
        {
            var save = JsonUtility.FromJson<SaveGame>(JsonUtility.ToJson(session.CaptureSave()));
            save.Validate();
            session.RestoreSave(save);
            Require(session.Player.IsPaused && session.Simulation.Paused, "Reload waits for player confirmation");
            typeof(TravelController).GetMethod("SetPaused", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(session.Player, new object[] { false });
            session.Simulation.Paused = false;
        }

        private static void AdvanceTo(MorningSession session, int minute)
        {
            while (session.Simulation.Minute < minute)
                Require(session.Simulation.Advance(minute - session.Simulation.Minute) > 0, "Journey clock can progress");
        }

        private static void MorningJourney()
        {
            var session = Prepare();
            Require(session.Simulation.Start(Core.MorningActivity.Breakfast, out _), "Journey breakfast starts");
            session.Simulation.Advance(7);
            ReloadAndResume(session);
            Require(session.Simulation.Advance(100) == 8 && session.Simulation.Money == 80000,
                "Journey breakfast finishes after load without duplicate cost");
            AdvanceTo(session, 480);
            Require(session.Simulation.Start(Core.MorningActivity.Study, out _), "Journey arrives at class");
            Require(session.Simulation.Advance(30) == 10 && session.Simulation.StoryPending,
                "Journey phone interrupts study at ten minutes");
            ReloadAndResume(session);
            Require(session.Simulation.StoryPending && session.Simulation.Advance(10) == 0,
                "Story choice remains blocking after reload");
            Require(session.Simulation.ChooseCommitment(Core.CommitmentChoice.CoverShift), "Journey accepts manager request");
            session.Simulation.HospitalRules.SevereTreatmentMinutes = 60;
            var vehicle = UnityEngine.Object.FindAnyObjectByType<TrafficVehicle>();
            // This minute-driven fixture models travel after the reload's short real-time protection.
            typeof(HospitalSession).GetField("<ProtectionRemaining>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(session.Hospital, 0f);
            session.Hospital.Impact(vehicle, 12);
            Require(session.Simulation.Hospitalized && session.Simulation.Money == 20000,
                "Journey accident charges admission once");
            session.Simulation.Treat(10);
            ReloadAndResume(session);
            while (session.Simulation.TreatmentRemaining > 0)
                Require(session.Simulation.Treat(10) > 0, "Journey treatment crosses deadlines");
            Require(session.Simulation.Minute == 550 && session.Simulation.Learning == 37 &&
                session.Simulation.Commitment.State == Core.CommitmentState.Failed && session.Simulation.Relations == 46,
                "Journey missed school and broken commitment settle exactly once");
            Require(session.Hospital.DischargeToWard(), "Journey discharge upstairs");
            ReloadAndResume(session);
            Require(session.Simulation.Money == 20000 && session.Player.transform.position.y > 3,
                "Journey discharged save keeps location and paid fee");
            AdvanceTo(session, 570);
            Require(session.Simulation.Start(Core.MorningActivity.Work, out _), "Journey starts regular work");
            session.Simulation.Advance(25);
            ReloadAndResume(session);
            AdvanceTo(session, 630);
            Require(session.Simulation.WorkMinutes == 60 && session.Simulation.Money == 50000 && session.Simulation.Relations == 46,
                "Journey wages paid once for actual work despite earlier accident");
            ReloadAndResume(session);
            // Return positioning is separate from the real controller route probes below.
            session.Player.TransferToHospital(session.BedExit, null);
            Require(session.StartBedRest(), "Journey returns to bed");
            session.Simulation.Advance(20);
            ReloadAndResume(session);
            Require(session.Player.IsInBed && session.Simulation.Activity == Core.MorningActivity.None,
                "Journey rests then stays in bed after reload");
            session.StopBedAction();
            AdvanceTo(session, 720);
            ReloadAndResume(session);
            Require(session.Simulation.Ended && session.Simulation.Money == 50000 &&
                session.Simulation.Learning == 37 && session.Simulation.Advance(1) == 0,
                "Journey morning end and settlements remain stable after reload");
            Debug.Log("[Morning Journey] PASS wake, breakfast, class/story, accident, treatment/deadlines, discharge, work, return/rest, ending and save/load checkpoints.");
        }

        public static void Run()
        {
            var previous = Physics.simulationMode;
            try
            {
                MorningJourney();
                var bedroom = Prepare(false);
                Require(bedroom.Player.IsInBed && !bedroom.HasStarted &&
                    Vector3.Distance(bedroom.Player.View.transform.position, bedroom.BedView.position) < .01f,
                    "New morning starts lying in bedroom");
                bedroom.WakeMorning();
                Require(bedroom.Player.IsInBed && bedroom.HasStarted, "Waking does not automatically leave bed");
                bedroom.StopBedAction();
                Require(!bedroom.Player.IsInBed && Vector3.Distance(bedroom.Player.transform.position, bedroom.BedExit.position) < .01f,
                    "Explicit stop places player safely beside bed");
                Require(bedroom.StartBedRest() && bedroom.Player.IsInBed, "Rest enters lying pose");
                bedroom.Simulation.Advance(7);
                var sleepingSave = JsonUtility.FromJson<SaveGame>(JsonUtility.ToJson(bedroom.CaptureSave()));
                sleepingSave.Validate();
                bedroom.RestoreSave(sleepingSave);
                Require(bedroom.Player.IsInBed && bedroom.Player.IsPaused && bedroom.CaptureSave().BedSleeping,
                    "Mid-sleep load restores bed pose, sleep effect and pause");
                bedroom.StopBedAction();
                Require(bedroom.Player.IsInBed, "Pause prevents getting out of bed");
                typeof(TravelController).GetMethod("SetPaused", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bedroom.Player, new object[] { false });
                bedroom.Simulation.Paused = false;
                Require(bedroom.Simulation.Advance(100) == 13 && bedroom.Player.IsInBed &&
                    bedroom.Simulation.Activity == Core.MorningActivity.None, "Recovery finishes once but player remains lying down");
                var restedSave = JsonUtility.FromJson<SaveGame>(JsonUtility.ToJson(bedroom.CaptureSave()));
                bedroom.RestoreSave(restedSave);
                Require(bedroom.Player.IsInBed && bedroom.CaptureSave().BedSleeping, "Completed rest remains in bed after load");
                typeof(TravelController).GetMethod("SetPaused", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bedroom.Player, new object[] { false });
                bedroom.Simulation.Paused = false;
                bedroom.StopBedAction();
                Require(!bedroom.Player.IsInBed && !bedroom.CaptureSave().BedSleeping && bedroom.Player.GetComponent<CharacterController>().enabled,
                    "Stop after completion restores walking and clears sleep state");
                var session = Prepare();
                var vehicle = UnityEngine.Object.FindAnyObjectByType<TrafficVehicle>();
                Call(vehicle, "Awake");
                typeof(TrafficVehicle).GetMethod("Report", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(vehicle, new object[] { session.Player.GetComponent<CharacterController>(), 6f });
                Require(session.Simulation.Hospitalized && !session.Player.IsRiding, "Pedestrian admitted");
                RequireHospitalBed(session);
                int chargedMoney = session.Simulation.Money;
                session.Hospital.Impact(vehicle, 12);
                Require(session.Simulation.Money == chargedMoney, "Repeat contact cannot bill again");
                var save = session.CaptureSave(); save.Validate();
                var roundtrip = JsonUtility.FromJson<SaveGame>(JsonUtility.ToJson(save));
                session.RestoreSave(roundtrip);
                RequireHospitalBed(session);
                // Old saves did not record a lying pose; they must also load into the ward bed.
                var legacyAdmission = session.CaptureSave();
                legacyAdmission.Travel.InBed = false;
                legacyAdmission.Travel.Position = session.Hospital.Reception.position;
                session.RestoreSave(legacyAdmission);
                RequireHospitalBed(session);
                Require(session.Simulation.Hospitalized && session.Simulation.Money == chargedMoney && session.Player.IsPaused,
                    "Hospital world save/load preserves fee and pause");
                typeof(TravelController).GetMethod("SetPaused", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(session.Player, new object[] { false });
                session.Simulation.Paused = false;
                Require(session.Hospital.DischargeToWard() && !session.Hospital.DischargeToWard() &&
                    Vector3.Distance(session.Player.transform.position, session.Hospital.Ward.position) < .01f &&
                    !session.Player.IsInBed && session.Player.GetComponent<CharacterController>().enabled &&
                    session.Player.transform.position.y > 3 && session.Simulation.Money == chargedMoney,
                    "Light discharge stands by upstairs bed without extra fee");

                session = Prepare();
                vehicle = UnityEngine.Object.FindAnyObjectByType<TrafficVehicle>();
                var travel = session.Player.CaptureTravel(); travel.Riding = true;
                session.Player.RestoreTravel(travel);
                typeof(TravelController).GetMethod("SetPaused", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(session.Player, new object[] { false });
                Call(vehicle, "Awake");
                session.Player.Scooter.GetComponent<Rigidbody>().linearVelocity = -(vehicle.EndPoint - vehicle.StartPoint).normalized * 9;
                typeof(TrafficVehicle).GetMethod("Report", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(vehicle, new object[] { session.Player.Scooter.GetComponent<BoxCollider>(), 6f });
                Require(session.Simulation.Hospitalized && session.Simulation.TreatmentRemaining > 0 && !session.Player.IsRiding, "Rider admitted and dismounted");
                RequireHospitalBed(session);
                Require(Vector3.Distance(session.Player.Scooter.GetComponent<Rigidbody>().position, session.Hospital.BikeParking.position) < .01f, "Bike parked safely");
                session.CaptureSave().Validate();
                while (session.Simulation.TreatmentRemaining > 0)
                    Require(session.Simulation.Treat(10) > 0, "Severe treatment progresses");
                Require(session.Hospital.DischargeToWard() &&
                    Vector3.Distance(session.Player.transform.position, session.Hospital.Ward.position) < .01f,
                    "Severe discharge also stays upstairs beside bed");

                session = Prepare();
                var vehicles = UnityEngine.Object.FindObjectsByType<TrafficVehicle>();
                foreach (var item in vehicles) Call(item, "Awake");
                // Keep the player's bike stationary during the traffic movement probe.
                session.Player.Scooter.GetComponent<Rigidbody>().isKinematic = true;
                vehicle = GameObject.Find("Traffic car 0").GetComponent<TrafficVehicle>();
                Vector3 start = vehicle.transform.position;
                Physics.simulationMode = SimulationMode.Script;
                Physics.SyncTransforms();
                for (int i = 0; i < 100; i++)
                {
                    foreach (var item in vehicles) Call(item, "FixedUpdate");
                    Physics.Simulate(.02f);
                }
                Require(Vector3.Distance(vehicle.GetComponent<Rigidbody>().position, start) > 1, "Traffic actually advances");
                var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    blocker.transform.position = vehicle.GetComponent<Rigidbody>().position + vehicle.transform.forward * 3;
                    blocker.transform.localScale = new Vector3(2, 2, .3f);
                    Physics.SyncTransforms();
                    for (int i = 0; i < 100; i++) { Call(vehicle, "FixedUpdate"); Physics.Simulate(.02f); }
                    Require(vehicle.Speed < .05f, "Traffic brakes for obstacle");
                }
                finally { UnityEngine.Object.DestroyImmediate(blocker); }
                Debug.Log("[Traffic Runtime] PASS bedroom wake/sleep/explicit stop/save-load, upstairs discharge, pedestrian/rider admission, one fee, parking, movement and braking.");
            }
            finally
            {
                Physics.simulationMode = previous; Time.timeScale = 1;
                EditorSceneManager.OpenScene(TrafficHospitalTools.ScenePath);
            }
        }
    }
}
