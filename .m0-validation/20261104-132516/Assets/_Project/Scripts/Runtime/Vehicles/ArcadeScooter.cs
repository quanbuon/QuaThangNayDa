using UnityEngine;

namespace QuaThangNayDa
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class ArcadeScooter : MonoBehaviour
    {
        public Transform Seat;
        public Transform SafeSpot;
        public float SpeedKph => _body.linearVelocity.magnitude * 3.6f;
        public bool Mounted { get; set; }
        public int CollisionCount { get; private set; }
        public float LastImpactTime { get; private set; } = -100;
        private Rigidbody _body;
        private Vector2 _drive;
        private bool _brake;
        private Vector3 _safePosition;
        private Quaternion _safeRotation;
        private PhysicsMaterial _contactMaterial;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.mass = 140;
            _body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // This arcade body slides on the floor instead of rolling on wheel colliders.
            // Default static friction cancels the 4 m/s² acceleration every physics step.
            // Grip, coasting and braking are controlled explicitly in FixedUpdate instead.
            _contactMaterial = new PhysicsMaterial("Scooter arcade contact")
            {
                staticFriction = 0,
                dynamicFriction = 0,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            GetComponent<BoxCollider>().sharedMaterial = _contactMaterial;
            _safePosition = SafeSpot != null ? SafeSpot.position : transform.position;
            _safeRotation = SafeSpot != null ? SafeSpot.rotation : transform.rotation;
        }
        public void SetInput(Vector2 drive, bool brake) { _drive = drive; _brake = brake; }

        private void OnDestroy()
        {
            if (_contactMaterial == null) return;
            if (Application.isPlaying) Destroy(_contactMaterial);
            else DestroyImmediate(_contactMaterial);
        }

        private void FixedUpdate()
        {
            if (!Physics.Raycast(_body.position + Vector3.up * 0.1f, Vector3.down, 0.9f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return;
            Vector3 velocity = _body.linearVelocity;
            // Physics uses the current simulation pose, not the interpolated render Transform.
            Vector3 forwardAxis = _body.rotation * Vector3.forward;
            Vector3 rightAxis = _body.rotation * Vector3.right;
            float forward = Vector3.Dot(velocity, forwardAxis);
            float target = Mounted && !_brake ? _drive.y * (_drive.y >= 0 ? 9f : 2f) : 0;
            float acceleration = _brake || !Mounted ? 12 : Mathf.Abs(_drive.y) < 0.01f ? 2 : 4;
            forward = Mathf.MoveTowards(forward, target, acceleration * Time.fixedDeltaTime);
            Vector3 lateral = Vector3.Project(velocity, rightAxis);
            _body.linearVelocity = forwardAxis * forward + lateral * 0.75f + Vector3.up * velocity.y;
            float turn = Mounted ? _drive.x * Mathf.Clamp(forward / 3, -1, 1) * 65 : 0;
            // At high speed the wider turning radius encourages braking before corners.
            _body.MoveRotation(_body.rotation * Quaternion.Euler(0, turn * Time.fixedDeltaTime, 0));
            _body.angularVelocity = Vector3.zero;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!Mounted || collision.relativeVelocity.magnitude < 2 || Time.time - LastImpactTime < 2) return;
            bool wall = false;
            foreach (ContactPoint contact in collision.contacts)
                if (Mathf.Abs(contact.normal.y) < 0.5f) wall = true;
            if (!wall) return;
            LastImpactTime = Time.time;
            CollisionCount++;
            _body.linearVelocity *= 0.25f;
        }

        public void Recover()
        {
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.position = _safePosition;
            _body.rotation = _safeRotation;
        }

        public void CaptureInto(TravelSnapshot data)
        {
            data.ScooterPosition = _body.position;
            data.ScooterRotation = _body.rotation;
            data.ScooterVelocity = _body.linearVelocity;
            data.ScooterAngularVelocity = _body.angularVelocity;
            data.CollisionCount = CollisionCount;
            data.CollisionCooldown = Mathf.Clamp(2 - (Time.time - LastImpactTime), 0, 2);
        }
        public void RestoreFrom(TravelSnapshot data)
        {
            _body.position = data.ScooterPosition;
            _body.rotation = data.ScooterRotation;
            transform.SetPositionAndRotation(data.ScooterPosition, data.ScooterRotation);
            _body.linearVelocity = data.ScooterVelocity;
            _body.angularVelocity = data.ScooterAngularVelocity;
            CollisionCount = data.CollisionCount;
            LastImpactTime = Time.time - (2 - data.CollisionCooldown);
            Mounted = data.Riding;
            _drive = Vector2.zero;
            _brake = false;
        }
    }
}
