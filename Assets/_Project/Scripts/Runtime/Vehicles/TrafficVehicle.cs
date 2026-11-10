using UnityEngine;

namespace QuaThangNayDa
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class TrafficVehicle : MonoBehaviour
    {
        public MorningSession Session;
        public HospitalSession Hospital;
        public Vector3 StartPoint, EndPoint;
        public float CruiseSpeed = 6, Acceleration = 2, Braking = 6;
        public float Speed { get; private set; }
        private Rigidbody _body;
        private BoxCollider _box;
        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Collider[] _spawnOverlaps = new Collider[32];
        private Vector3 _direction;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>(); _box = GetComponent<BoxCollider>();
            _body.isKinematic = true; _body.useGravity = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _direction = (EndPoint - StartPoint).normalized;
        }

        private void FixedUpdate()
        {
            var sim = Session.Simulation;
            if (sim == null || !Session.HasStarted || sim.Paused || sim.StoryPending || sim.Hospitalized || sim.Ended || Session.Player.IsPaused)
            { Speed = 0; return; }
            float toEnd = Vector3.Dot(EndPoint - _body.position, _direction);
            if (toEnd <= .1f)
            {
                int occupied = Physics.OverlapBoxNonAlloc(StartPoint, _box.size * .5f + Vector3.one * .3f,
                    _spawnOverlaps, _body.rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < occupied; i++)
                    if (_spawnOverlaps[i] != _box && !IsGround(_spawnOverlaps[i])) { Speed = 0; return; }
                _body.position = StartPoint; Speed = 0; return;
            }
            float previous = Speed;
            // Brake ahead of traffic/obstacles; a late crossing can still reach contact.
            float sensor = Mathf.Max(2, Speed * Speed / (2 * Braking) + .7f);
            int count = Physics.BoxCastNonAlloc(_body.position + _box.center, _box.size * .5f * .96f,
                _direction, _hits, _body.rotation, sensor, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float clearance = sensor;
            Collider obstacle = null;
            for (int i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit.collider == _box || IsGround(hit.collider) || hit.distance >= clearance) continue;
                clearance = hit.distance; obstacle = hit.collider;
            }
            Speed = Mathf.MoveTowards(Speed, obstacle != null ? 0 : CruiseSpeed,
                (obstacle != null ? Braking : Acceleration) * Time.fixedDeltaTime);
            float step = Mathf.Min(Speed * Time.fixedDeltaTime, toEnd);
            if (obstacle != null && clearance <= step + .05f)
            {
                Report(obstacle, previous); step = Mathf.Max(0, clearance - .04f); Speed = 0;
            }
            _body.MovePosition(_body.position + _direction * step);
        }

        private static bool IsGround(Collider collider) => collider.bounds.max.y < .15f;
        private void Report(Collider other, float ownSpeed)
        {
            bool walking = other.GetComponentInParent<TravelController>() == Session.Player && !Session.Player.IsRiding;
            bool riding = other.GetComponentInParent<ArcadeScooter>() == Session.Player.Scooter && Session.Player.IsRiding;
            if (!walking && !riding) return;
            Vector3 playerVelocity = riding ? Session.Player.Scooter.GetComponent<Rigidbody>().linearVelocity : Session.Player.GetComponent<CharacterController>().velocity;
            Hospital.Impact(this, (playerVelocity - _direction * ownSpeed).magnitude);
        }

        private void OnCollisionEnter(Collision collision) => Report(collision.collider, Speed);
        public void WalkingContact(ControllerColliderHit hit) => Report(hit.controller, Speed);
    }
}
