using UnityEngine;

namespace QuaThangNayDa
{
    public static class ActivityTargeting
    {
        public const float Reach = 3f;

        public static bool Visible(ActivityStation station, Vector3 eye, Vector3 point)
        {
            if (station == null || !station.isActiveAndEnabled) return false;
            Vector3 delta = point - eye;
            if (delta.sqrMagnitude > Reach * Reach + .00001f) return false;
            if (delta.sqrMagnitude < .000001f) return true;
            return Physics.Raycast(eye, delta.normalized, out var hit, delta.magnitude + .01f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<ActivityStation>() == station;
        }

        public static ActivityStation Find(Vector3 eye, Vector3 forward, out Vector3 point)
        {
            point = default;
            // Precise aim wins. The ray length is a maximum, never a minimum distance.
            if (Physics.Raycast(eye, forward, out var hit, Reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                var direct = hit.collider.GetComponentInParent<ActivityStation>();
                if (direct != null) { point = hit.point; return direct; }
            }
            // Never substitute a nearby activity when the crosshair hits a wall,
            // a non-interactive prop, or empty space.
            return null;
        }
    }
}
