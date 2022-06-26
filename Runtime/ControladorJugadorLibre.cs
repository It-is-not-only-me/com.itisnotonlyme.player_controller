using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItIsNotOnlyMe.PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Controladores/Controlador jugador libre")]
    public class ControladorJugadorLibre : MonoBehaviour
    {
        [SerializeField] private InputSystemSO _inputJugador;

        [Space]

        [SerializeField] private Camera _camara;

        [Space]

        [SerializeField] private float _rapidezAlCaminar = 3;
        [SerializeField] private float _tiempoDeTransicionEnMovimiento = 0f;
        [SerializeField] private float _rapidezVertical = 3;

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

        private float _ultimoTiempoEnElPiso;
        private Vector3 _direccionDelInput;

        private bool _saltando;
        private bool _agachando;

        private void OnEnable()
        {
            _inputJugador.EventoMoverse += ActualizarInput;
            _inputJugador.EventoSaltarEmpiezar += SaltarEmpezar;
            _inputJugador.EventoSaltarTerminar += SaltarTerminar;
            _inputJugador.EventoAgacharseEmpiezar += AgacharEmpezar;
            _inputJugador.EventoAgacharseTerminar += AgacharTerminar;
            _inputJugador.EventoRotar += Rotar;
        }

        private void OnDisable()
        {
            _inputJugador.EventoMoverse -= ActualizarInput;
            _inputJugador.EventoSaltarEmpiezar -= SaltarEmpezar;
            _inputJugador.EventoSaltarTerminar -= SaltarTerminar;
            _inputJugador.EventoAgacharseEmpiezar -= AgacharEmpezar;
            _inputJugador.EventoAgacharseTerminar -= AgacharTerminar;
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

        void Update()
        {
            Mover();
        }

        private void SaltarEmpezar() => _saltando = true;
        private void SaltarTerminar() => _saltando = false;

        private void AgacharEmpezar() => _agachando = true;
        private void AgacharTerminar() => _agachando = false;

        void ActualizarInput(Vector2 movimiento)
        {
            _direccionDelInput = new Vector3(movimiento.x, 0, movimiento.y).normalized;
        }

        void Mover()
        {
            Vector3 worldInputDir = transform.TransformDirection(_direccionDelInput);
            Vector3 targetVelocity = worldInputDir * _rapidezAlCaminar;
            _velocidad = Vector3.SmoothDamp(_velocidad, targetVelocity, ref _smoothV, _tiempoDeTransicionEnMovimiento);


            if (!_saltando && !_agachando)
                _velocidadVertical = 0;
            if (_saltando)
                _velocidadVertical += _rapidezVertical * Time.deltaTime;
            if (_agachando)
                _velocidadVertical -= _rapidezVertical * Time.deltaTime;

            _velocidad = new Vector3(_velocidad.x, _velocidadVertical, _velocidad.z);

            CollisionFlags flags = _controlador.Move(_velocidad * Time.deltaTime);
            if (flags == CollisionFlags.Below)
            {
                _saltando = false;
                _ultimoTiempoEnElPiso = Time.time;
                _velocidadVertical = 0;
            }
        }

        void Rotar(Vector2 rotacion)
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