using System;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class ActivityTargetingChecks
    {
        public static void Run()
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var neighbour = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 eye = new Vector3(1000, 1000, 1000);
            var station = target.AddComponent<ActivityStation>();
            wall.SetActive(false);
            neighbour.SetActive(false);
            try
            {
                foreach (float distance in new[] { .05f, .25f, .5f, 1f, 2f, 2.99f, 3f, 3.1f })
                {
                    target.transform.position = eye + Vector3.forward * (distance + .5f);
                    Physics.SyncTransforms();
                    var found = ActivityTargeting.Find(eye, Vector3.forward, out var point);
                    bool expected = distance <= 3;
                    if ((found == station) != expected || (expected && !ActivityTargeting.Visible(station, eye, point)))
                        throw new InvalidOperationException("Interaction distance failed: " + distance);
                }
                target.transform.position = eye + new Vector3(0, -1.1f, .8f);
                target.transform.localScale = new Vector3(1, .2f, 1);
                Physics.SyncTransforms();
                if (ActivityTargeting.Find(eye, (target.transform.position - eye).normalized, out _) != station)
                    throw new InvalidOperationException("Close low furniture not detected");
                if (ActivityTargeting.Find(eye, Vector3.forward, out _) != null)
                    throw new InvalidOperationException("Selected furniture outside crosshair");
                if (ActivityTargeting.Find(eye, Vector3.back, out _) != null)
                    throw new InvalidOperationException("Selected behind player");
                wall.SetActive(true);
                wall.transform.position = eye + new Vector3(0, -.5f, .15f);
                wall.transform.localScale = new Vector3(3, 3, .1f);
                Physics.SyncTransforms();
                if (ActivityTargeting.Find(eye, Vector3.forward, out _) != null)
                    throw new InvalidOperationException("Interaction through wall");
                wall.SetActive(false);
                target.transform.position = eye + Vector3.forward * 2;
                target.transform.localScale = Vector3.one;
                neighbour.SetActive(true);
                neighbour.transform.position = eye + new Vector3(1, 0, 1);
                neighbour.AddComponent<ActivityStation>();
                Physics.SyncTransforms();
                if (ActivityTargeting.Find(eye, Vector3.forward, out _) != station)
                    throw new InvalidOperationException("Closer neighbour stole crosshair target");
                wall.SetActive(true);
                wall.transform.position = eye + Vector3.forward;
                wall.transform.localScale = new Vector3(.5f, 1, .1f);
                Physics.SyncTransforms();
                if (ActivityTargeting.Find(eye, Vector3.forward, out _) != null)
                    throw new InvalidOperationException("Non-interactive surface substituted by neighbour");
                Debug.Log("[Interaction] 0.05-3m acquisition/confirmation, >3m rejection, low furniture aim, multiple targets and occlusion passed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(wall); UnityEngine.Object.DestroyImmediate(neighbour); }
        }
    }
}
