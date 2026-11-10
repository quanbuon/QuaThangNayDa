using System;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    // Run only in the disposable validation project, not the user's open scene.
    public static class ScooterPhysicsCheck
    {
        public static void RunBatch()
        {
            EditorSceneManager.OpenScene(TravelTools.ScenePath);
            var scooter = UnityEngine.Object.FindAnyObjectByType<ArcadeScooter>();
            if (scooter == null) throw new InvalidOperationException("Scooter missing.");
            var previous = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            try
            {
                scooter.SendMessage("Awake");
                Physics.SyncTransforms();
                for (int i = 0; i < 50; i++) Physics.Simulate(0.02f);
                Vector3 start = scooter.transform.position;
                scooter.Mounted = true;
                scooter.SetInput(Vector2.up, false);
                for (int i = 0; i < 150; i++)
                {
                    scooter.SendMessage("FixedUpdate");
                    Physics.Simulate(0.02f);
                }
                float distance = scooter.transform.position.z - start.z;
                float speed = scooter.SpeedKph;
                Debug.Log($"[ScooterCheck] forwardDistance={distance:F3}m speed={speed:F3}km/h");
                if (distance < 3 || speed < 5) throw new InvalidOperationException("Scooter cannot accelerate on the actual scene floor.");
                scooter.SetInput(Vector2.zero, true);
                for (int i = 0; i < 75; i++)
                {
                    scooter.SendMessage("FixedUpdate");
                    Physics.Simulate(0.02f);
                }
                Debug.Log($"[ScooterCheck] brakingSpeed={scooter.SpeedKph:F3}km/h");
                if (scooter.SpeedKph > 1) throw new InvalidOperationException("Scooter brake failed.");
                Debug.Log("[ScooterCheck] PASS acceleration and braking.");
            }
            finally { Physics.simulationMode = previous; }
        }
    }
}
