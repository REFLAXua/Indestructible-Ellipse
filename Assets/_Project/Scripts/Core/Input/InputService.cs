using UnityEngine;

namespace Core.Input
{
    public interface IInputService
    {
        Vector2 MoveInput { get; }
        bool IsJumpPressed { get; }
        bool IsSprintPressed { get; }
        bool IsAttackPressed { get; }
        bool IsAimPressed { get; }
        bool IsInteractPressed { get; }
        bool IsInteractHeld { get; }
        bool IsCancelPressed { get; }
        Vector2 LookInput { get; }
    }

    public class PlayerInputService : IInputService
    {
        public Vector2 MoveInput => new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
        public bool IsJumpPressed => UnityEngine.Input.GetButtonDown("Jump");
        public bool IsSprintPressed => UnityEngine.Input.GetKey(KeyCode.LeftShift);
        public bool IsAttackPressed => UnityEngine.Input.GetButtonDown("Fire1");
        public bool IsAimPressed => UnityEngine.Input.GetMouseButton(1);
        public bool IsInteractPressed => UnityEngine.Input.GetKeyDown(KeyCode.E);
        public bool IsInteractHeld => UnityEngine.Input.GetKey(KeyCode.E);
        public bool IsCancelPressed => UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        public Vector2 LookInput => new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
    }
}
