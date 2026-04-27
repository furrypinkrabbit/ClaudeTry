using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace GuJian.Controllers {
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class TopDownCameraFollow : MonoBehaviour {

        [SerializeField] string playerTag    = "Player";
        [SerializeField] float  sensitivity  = 0.25f;
        [SerializeField] float  pitchMin     = 5f;
        [SerializeField] float  pitchMax     = 75f;
        [SerializeField] float  initPitch    = 30f;
        [SerializeField] float  initYaw      = 0f;
        [SerializeField] float  distance     = 8f;
        [SerializeField] float  distMin      = 3f;
        [SerializeField] float  distMax      = 18f;
        [SerializeField] float  smooth       = 12f;

        CinemachineVirtualCamera _vcam;
        CinemachineTransposer    _body;

        float _yaw, _pitch, _tYaw, _tPitch, _tDist;

        void Awake() {
            _vcam  = GetComponent<CinemachineVirtualCamera>();
            _tYaw  = _yaw   = initYaw;
            _tPitch= _pitch = initPitch;
            _tDist = distance;
        }

        void Start() {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) { _vcam.Follow = go.transform; _vcam.LookAt = go.transform; }

            _body = _vcam.GetCinemachineComponent<CinemachineTransposer>();
            if (_body != null)
                _body.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        }

        void Update() {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.isPressed) {
                var d = mouse.delta.ReadValue();
                _tYaw    += d.x * sensitivity;
                _tPitch   = Mathf.Clamp(_tPitch - d.y * sensitivity, pitchMin, pitchMax);
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f)
                _tDist = Mathf.Clamp(_tDist - scroll * 0.01f, distMin, distMax);

            float dt = Time.deltaTime;
            _yaw     = Mathf.LerpAngle(_yaw,    _tYaw,   smooth * dt);
            _pitch   = Mathf.LerpAngle(_pitch,  _tPitch, smooth * dt);
            distance = Mathf.Lerp(distance, _tDist, smooth * dt);

            if (_body == null) return;
            float ry = _yaw   * Mathf.Deg2Rad;
            float rp = _pitch * Mathf.Deg2Rad;
            _body.m_FollowOffset = new Vector3(
                 distance * Mathf.Sin(ry) * Mathf.Cos(rp),
                 distance * Mathf.Sin(rp),
                -distance * Mathf.Cos(ry) * Mathf.Cos(rp)
            );

            if (_vcam.Follow) transform.LookAt(_vcam.Follow.position);
        }
    }
}