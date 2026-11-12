using UnityEngine;
using UnityEngine.InputSystem;

// First-Person-Controller: WASD laufen, Maus umschauen, Leertaste springen.
// Double Jump, Dash usw. kommen in den naechsten Issues dazu.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Bewegung")]
    public float moveSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -25f;

    [Header("Maus")]
    public float mouseSensitivity = 0.1f;
    public Transform cameraTransform;

    CharacterController controller;
    Vector3 velocity;
    float pitch;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        LockCursor(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || Mouse.current == null) return;

        // Esc gibt die Maus frei, Klick fangt sie wieder ein
        if (kb.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (Mouse.current.leftButton.wasPressedThisFrame) LockCursor(true);

        if (Cursor.lockState == CursorLockMode.Locked) Look();
        Move(kb);
    }

    void Look()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up * delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, -89f, 89f);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void Move(Keyboard kb)
    {
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 input = transform.right * x + transform.forward * z;
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (controller.isGrounded && kb.spaceKey.wasPressedThisFrame)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        Vector3 move = input * moveSpeed + Vector3.up * velocity.y;
        controller.Move(move * Time.deltaTime);
    }

    static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
