using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ItIsNotOnlyMe.PlayerController
{
    [CreateAssetMenu(fileName = "Input Event System", menuName = "Player Controller/Input Event System")]
    public class InputSystemSO : ScriptableObject, Player.IJugadorActions
    {
        public event Action EventoSaltarEmpieza;
        public event Action EventoSaltarTermina;
        public event Action EventoAgacharseEmpieza;
        public event Action EventoAgacharseTermina;
        public event Action<Vector2> EventoMoverse;
        public event Action<Vector2> EventoRotar;

        private Player _playerControls = null;
        private Player _controller
        {
            get
            {
                if (_playerControls == null)
                {
                    _playerControls = new Player();
                    _playerControls.Jugador.SetCallbacks(this);
                }
                return _playerControls;
            }
        }

        private void OnEnable() => _controller.Jugador.Enable();
        private void OnDisable() => _controller.Jugador.Disable();


        public void OnSaltar(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                EventoSaltarEmpieza?.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                EventoSaltarTermina?.Invoke();
        }

        public void OnAgacharse(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                EventoAgacharseEmpieza?.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                EventoAgacharseTermina?.Invoke();
        }

        public void OnDirecciones(InputAction.CallbackContext context)
        {
            EventoMoverse?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnRotar(InputAction.CallbackContext context)
        {
            EventoRotar?.Invoke(context.ReadValue<Vector2>());
        }
    }
}
