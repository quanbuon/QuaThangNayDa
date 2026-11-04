using UnityEngine;

namespace QuaThangNayDa
{
    [RequireComponent(typeof(CharacterController), typeof(TravelInput))]
    public sealed class TravelController : MonoBehaviour
    {
        public Camera View;
        public ArcadeScooter Scooter;
        private TravelInput _input;
        private CharacterController _controller;
        private bool _riding, _paused;
        private float _yaw, _pitch, _verticalSpeed;
        private Vector3 _spawn;
        private string _message = "";

        private void Awake()
        {
            _input = GetComponent<TravelInput>();
            _controller = GetComponent<CharacterController>();
            _spawn = transform.position;
            _yaw = transform.eulerAngles.y;
            Application.targetFrameRate = 60;
            SetPaused(false);
        }
        private void Update()
        {
            if (_input.Pause) SetPaused(!_paused);
            if (_paused) return;
            Vector2 look = _input.Look * 0.09f;
            _yaw += look.x;
            _pitch = Mathf.Clamp(_pitch - look.y, -65, 65);
            if (_input.Interact) ToggleRide();
            if (_input.Recover)
            {
                if (_riding) Scooter.Recover();
                else { _controller.enabled = false; transform.position = _spawn; _controller.enabled = true; }
                _message = "Da dua ve diem an toan cua khu thu.";
            }
            if (_riding) Scooter.SetInput(_input.Move, _input.Brake);
            else
            {
                transform.rotation = Quaternion.Euler(0, _yaw, 0);
                if (_controller.isGrounded && _verticalSpeed < 0) _verticalSpeed = -2;
                _verticalSpeed += Physics.gravity.y * Time.deltaTime;
                Vector2 move = Vector2.ClampMagnitude(_input.Move, 1);
                Vector3 motion = (transform.forward * move.y + transform.right * move.x) * 3.5f;
                _controller.Move((motion + Vector3.up * _verticalSpeed) * Time.deltaTime);
                if (transform.position.y < -10) { _controller.enabled = false; transform.position = _spawn; _controller.enabled = true; }
            }
        }

        private void LateUpdate()
        {
            Vector3 position = _riding ? Scooter.Seat.position : transform.position + Vector3.up * 1.65f;
            float yaw = _riding ? Scooter.transform.eulerAngles.y + Mathf.Clamp(_yaw, -85, 85) : _yaw;
            if (_riding) _yaw = Mathf.Clamp(_yaw, -85, 85);
            float blend = 1 - Mathf.Exp(-16 * Time.unscaledDeltaTime);
            View.transform.position = Vector3.Lerp(View.transform.position, position, blend);
            View.transform.rotation = Quaternion.Slerp(View.transform.rotation, Quaternion.Euler(_pitch, yaw, 0), blend);
        }

        private void ToggleRide()
        {
            if (!_riding)
            {
                if (Vector3.Distance(transform.position, Scooter.transform.position) > 3) { _message = "Lai gan xe (duoi 3m) roi bam E."; return; }
                Vector3 direction = Scooter.Seat.position - View.transform.position;
                if (Physics.Raycast(View.transform.position, direction.normalized, out RaycastHit hit, direction.magnitude,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(Scooter.transform))
                { _message = "Co vat can giua ban va xe."; return; }
                _controller.enabled = false;
                _riding = Scooter.Mounted = true;
                _yaw = 0;
            }
            else
            {
                if (Scooter.SpeedKph > 2) { _message = "Phanh dung xe truoc khi xuong."; return; }
                bool found = false;
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 point = Scooter.transform.position + Scooter.transform.right * side * 1.3f;
                    if (!Physics.Raycast(point + Vector3.up * 2, Vector3.down, out RaycastHit ground, 4,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || ground.normal.y < 0.7f) continue;
                    point = ground.point + Vector3.up * 0.08f;
                    if (Physics.CheckCapsule(point + Vector3.up * 0.35f, point + Vector3.up * 1.45f, 0.3f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                    transform.position = point;
                    found = true;
                    break;
                }
                if (!found) { _message = "Khong du cho xuong xe. Di ra cho rong hoac bam R."; return; }
                _yaw += Scooter.transform.eulerAngles.y;
                _riding = Scooter.Mounted = false;
                _controller.enabled = true;
                _verticalSpeed = 0;
                Scooter.SetInput(Vector2.zero, true);
            }
            _message = "";
        }

        private void SetPaused(bool paused)
        {
            _paused = paused;
            Time.timeScale = paused ? 0 : 1;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) SetPaused(true); }
        private void OnDisable() { Time.timeScale = 1; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 550, 200), GUI.skin.box);
            GUILayout.Label("M1 / KHU THU DIEU KHIEN - CHUA PHAI MY THUAT CHINH THUC");
            GUILayout.Label("WASD: di/lai | Chuot: nhin | E: len/xuong xe | Space: phanh");
            GUILayout.Label("R: ve diem an toan | Esc: tam dung / tha chuot");
            GUILayout.Label(_riding ? $"Dang lai: {Scooter.SpeedKph:0} km/h" : "Dang di bo - den gan xe va bam E");
            GUILayout.Label($"Va cham du nguong: {Scooter.CollisionCount} (cach nhau it nhat 2 giay)");
            if (Time.time - Scooter.LastImpactTime < 0.8f) GUILayout.Label("VA CHAM - GIAM TOC!");
            GUILayout.Label(_message);
            if (_paused && GUILayout.Button("TIEP TUC")) SetPaused(false);
            GUILayout.EndArea();
        }
    }
}
