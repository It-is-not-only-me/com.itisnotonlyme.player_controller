using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItIsNotOnlyMe.PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Controladores/Controlador jugador")]
    public class ControladorJugador : MonoBehaviour
    {
        [SerializeField] private InputSystemSO _inputJugador;

        [Space]

        [SerializeField] private Camera _camara;

        [Space]

        [SerializeField] private float _rapidezAlCaminar = 3;
        [SerializeField] private float _tiempoDeTransicionEnMovimiento = 0f;
        [SerializeField] private float _fuerzaDeSalto = 8;
        [SerializeField] private float _gravedad = 18;

        [SerializeField] private bool _lockCursor;
        [SerializeField] private float _sensiblidadDelMouse = 10;
        [SerializeField] private Vector2 _minMaxpitch = new Vector2(-40, 85);
        [SerializeField] private float _tiempoDeTransicionEnRotacion = 0f;

        [SerializeField] private float _yaw;
        [SerializeField] private float _pitch;
        private CharacterController _controlador;
        private float _smoothYaw;
        private float _smoothPitch;

        private float _yawSmoothV;
        private float _pitchSmoothV;
        private float _velocidadVertical;
        private Vector3 _velocidad;
        private Vector3 _smoothV;

        private bool _saltando;
        private float _ultimoTiempoEnElPiso;
        private Vector3 _direccionDelInput;

        private void OnEnable()
        {
            _inputJugador.EventoMoverse += ActualizarInput;
            _inputJugador.EventoSaltarEmpiezar += Saltar;
            _inputJugador.EventoRotar += Rotar;
        }

        private void OnDisable()
        {
            _inputJugador.EventoMoverse -= ActualizarInput;
            _inputJugador.EventoSaltarEmpiezar -= Saltar;
            _inputJugador.EventoRotar -= Rotar;
        }

        private void Awake()
        {
            if (_camara == null)
                _camara = Camera.main;
        }

        private void Start()
        {
            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _controlador = GetComponent<CharacterController>();

            _yaw = transform.eulerAngles.y;
            _pitch = _camara.transform.localEulerAngles.x;
            _smoothYaw = _yaw;
            _smoothPitch = _pitch;
        }

        private void Update()
        {
            Mover();
        }

        private void ActualizarInput(Vector2 movimiento)
        {
            _direccionDelInput = new Vector3(movimiento.x, 0, movimiento.y).normalized;
        }

        private void Mover()
        {
            Vector3 worldInputDir = transform.TransformDirection(_direccionDelInput);
            Vector3 targetVelocity = worldInputDir * _rapidezAlCaminar;
            _velocidad = Vector3.SmoothDamp(_velocidad, targetVelocity, ref _smoothV, _tiempoDeTransicionEnMovimiento);

            _velocidadVertical -= _gravedad * Time.deltaTime;
            _velocidad = new Vector3(_velocidad.x, _velocidadVertical, _velocidad.z);

            var flags = _controlador.Move(_velocidad * Time.deltaTime);
            if (flags == CollisionFlags.Below)
            {
                _saltando = false;
                _ultimoTiempoEnElPiso = Time.time;
                _velocidadVertical = 0;
            }
        }

        private void Saltar()
        {
            float timeSinceLastTouchedGround = Time.time - _ultimoTiempoEnElPiso;
            if (_controlador.isGrounded || (!_saltando && timeSinceLastTouchedGround < 0.15f))
            {
                _saltando = true;
                _velocidadVertical = _fuerzaDeSalto;
            }
        }

        private void Rotar(Vector2 rotacion)
        {
            _yaw += rotacion.x * _sensiblidadDelMouse / 10;
            _pitch -= rotacion.y * _sensiblidadDelMouse / 10;
            _pitch = Mathf.Clamp(_pitch, _minMaxpitch.x, _minMaxpitch.y);
            _smoothPitch = Mathf.SmoothDampAngle(_smoothPitch, _pitch, ref _pitchSmoothV, _tiempoDeTransicionEnRotacion);
            _smoothYaw = Mathf.SmoothDampAngle(_smoothYaw, _yaw, ref _yawSmoothV, _tiempoDeTransicionEnRotacion);

            transform.eulerAngles = Vector3.up * _smoothYaw;
            _camara.transform.localEulerAngles = Vector3.right * _smoothPitch;
        }
    }
}
