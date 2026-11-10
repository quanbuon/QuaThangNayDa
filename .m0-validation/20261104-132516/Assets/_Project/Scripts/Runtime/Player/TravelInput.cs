using UnityEngine;
using UnityEngine.InputSystem;

namespace QuaThangNayDa
{
    // Actions isolate device bindings from movement rules; gamepad can be bound later.
    public sealed class TravelInput : MonoBehaviour
    {
        private InputActionMap _map;
        private InputAction _move, _look, _interact, _brake, _recover, _pause;
        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 Look => _look.ReadValue<Vector2>();
        public bool Interact => _interact.WasPressedThisFrame();
        public bool Brake => _brake.IsPressed();
        public bool Recover => _recover.WasPressedThisFrame();
        public bool Pause => _pause.WasPressedThisFrame();

        private void Awake()
        {
            _map = new InputActionMap("Travel");
            _move = _map.AddAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _look = _map.AddAction("Look", InputActionType.Value, "<Mouse>/delta");
            _interact = _map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            _brake = _map.AddAction("Brake", InputActionType.Button, "<Keyboard>/space");
            _recover = _map.AddAction("Recover", InputActionType.Button, "<Keyboard>/r");
            _pause = _map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        }
        private void OnEnable() => _map.Enable();
        private void OnDisable() => _map.Disable();
        private void OnDestroy() => _map.Dispose();
    }
}
